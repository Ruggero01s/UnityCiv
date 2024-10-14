using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

//TODO change all debug.log with notifications
//TODO disable end turn when combat is happening
public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn;
    public Player enemy;
    public Player player;

    private Unit selectedUnit;
    private City selectedCity;

    private CityHUDManager cityHUDManager;
    private UnitHUDManager unitHUDManager;
    private WeatherHUDManager weatherHUDManager;

    private GeneralHUDController HUDctrl;
    private GridController gridController;

    private int combatTime = 3;
    private int dyingTime = 3;

    public bool disablePlayerInput = false;

    public enum WeatherState { Clear, Rain, Snow }

    public Queue<WeatherState> weatherQueue;

    void Start()
    {
        currentTurn = TurnState.PlayerTurn;  // Player starts first
        gridController = FindObjectOfType<GridController>();
        cityHUDManager = FindObjectOfType<CityHUDManager>();
        unitHUDManager = FindObjectOfType<UnitHUDManager>();
        weatherHUDManager = FindObjectOfType<WeatherHUDManager>();

        HUDctrl = FindObjectOfType<GeneralHUDController>();


        // Load the unit lists from the GridController
        player = gridController.player;
        enemy = gridController.enemy;

        Array values = Enum.GetValues(typeof(WeatherState));
        Random random = new();

        weatherQueue = new Queue<WeatherState>();
        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
    }

    void Update()
    {
        if (currentTurn == TurnState.EnemyTurn)
        {
            HUDctrl.UpdateEndTurnButtonState(false);
        }
        else
        {
            HUDctrl.UpdateEndTurnButtonState(!disablePlayerInput);
        }
        HandleInput();  // Manage player input for selecting units and issuing commands
    }

    void HandleInput()
    {
        if (disablePlayerInput) return;

        if (currentTurn == TurnState.PlayerTurn)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
                {
                    // Handle player unit selection
                    if (hit.collider.CompareTag("PlayerUnit"))
                    {
                        Unit clickedUnit = hit.transform.GetComponent<Unit>();

                        // Select the player unit if it's not null and belongs to the player
                        if (clickedUnit != null && clickedUnit.owner == player)
                        {
                            if (selectedUnit != null)
                                selectedUnit.UnHighlight();
                            selectedUnit = clickedUnit;
                            selectedUnit.Highlight();
                            unitHUDManager.OpenUnitHUD(selectedUnit);
                            if (selectedCity != null)
                            {
                                cityHUDManager.CloseCityHUD();
                                selectedCity = null;
                            }
                        }
                    }

                    // Handle enemy unit selection for combat
                    if (hit.collider.CompareTag("EnemyUnit") && selectedUnit != null)
                    {
                        Unit clickedEnemyUnit = hit.transform.GetComponent<Unit>();

                        if (clickedEnemyUnit != null)
                        {
                            HexagonGame selectedUnitHex = gridController.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];
                            HexagonGame enemyUnitHex = gridController.gameHexagons[clickedEnemyUnit.coordinates.x, clickedEnemyUnit.coordinates.y];

                            // Check if enemy is on a neighboring hex
                            if (gridController.GetGameNeighbors(selectedUnitHex).Contains(enemyUnitHex) && !selectedUnit.hasAttacked)
                            {
                                // Initiate combat between selected unit and enemy unit
                                disablePlayerInput = true;
                                InitiateCombat(selectedUnit, clickedEnemyUnit, enemyUnitHex);
                                disablePlayerInput = false;
                            }
                        }
                    }

                    // Handle terrain click for movement
                    if (hit.collider.CompareTag("MovableTerrain") && selectedUnit != null)
                    {
                        HexagonGame targetHex = hit.transform.GetComponent<HexagonGame>();
                        GridController gridController = FindObjectOfType<GridController>();
                        HexagonGame startHex = gridController.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];

                        // Move the selected unit to the new terrain if it's a valid destination
                        if (targetHex != null && selectedUnit.movementExpended < selectedUnit.movementUnits && !selectedUnit.isMoving)
                        {
                            selectedUnit.SetDestination(gridController.pathfinder.FindPath(startHex, targetHex));
                        }
                    }

                    // Detect a city click
                    if (hit.collider.CompareTag("City") && !EventSystem.current.IsPointerOverGameObject())
                    {
                        selectedCity = hit.collider.GetComponent<City>();
                        if (selectedUnit != null)
                        {
                            unitHUDManager.CloseUnitHUD();
                            selectedUnit.UnHighlight();
                            selectedUnit = null;
                        }
                        cityHUDManager.OpenCityHUD(selectedCity); // Open HUD for the city
                    }

                    // Handle city attack
                    if (hit.collider.CompareTag("EnemyCity") && selectedUnit != null)
                    {
                        HexagonGame selectedUnitHex = gridController.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];
                        HexagonGame cityHex = hit.transform.GetComponent<HexagonGame>();

                        // Check if the city is on a neighboring hex
                        if (gridController.GetGameNeighbors(selectedUnitHex).Contains(cityHex))
                        {
                            // Initiate combat with the city
                            AttackCity(selectedUnit, cityHex.tag);
                        }
                    }
                }
            }
        }
    }

    //TODO do death method for units instead of destroy directly
    private void AttackCity(Unit attacker, string tag) //TODO test // add animations // add ignoreUserinput
    {
        attacker.hasAttacked = true;
        if (tag == "EnemyCity")
        {
            Vector3 cityPos = gridController.enemyCity.transform.position;
            cityPos.y = 2.2f;
            StartCoroutine(RotateTowards(attacker, cityPos));
            Vector3 direction = (attacker.gameObject.transform.position - gridController.enemyCity.gameObject.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));
            Instantiate(gridController.cityAttackParticle, gridController.enemyCity.gameObject.transform.position, lookRotation);

            // Example logic: reduce city health by the attacking unit's attack value
            gridController.enemyCity.defenseHp -= attacker.atk;

            // Check if the city has been destroyed
            if (gridController.enemyCity.defenseHp <= 0)
            {
                //TODO implement win 
            }
            else
            {
                attacker.hp -= gridController.enemyCity.defenseAtk;

                if (attacker.hp <= 0)
                {
                    Destroy(attacker.gameObject);  // Remove player unit from the game
                    StartCoroutine(HUDctrl.Notify(attacker.name + " has been defeated!"));
                }
            }
        }
        else
        {
            Vector3 cityPos = gridController.playerCity.transform.position;
            cityPos.y = 2.2f;
            StartCoroutine(RotateTowards(attacker, cityPos));
            Vector3 direction = (attacker.gameObject.transform.position - gridController.playerCity.gameObject.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));
            Instantiate(gridController.cityAttackParticle, gridController.playerCity.gameObject.transform.position, lookRotation);
            StartCoroutine(RotateTowards(attacker, gridController.playerCity.transform.position));

            // Example logic: reduce city health by the attacking unit's attack value
            gridController.playerCity.defenseHp -= attacker.atk;

            // Check if the city has been destroyed
            if (gridController.playerCity.defenseHp <= 0)
            {
                //TODO implement lose 
            }
            else
            {
                attacker.hp -= gridController.playerCity.defenseAtk;

                if (attacker.hp <= 0)
                {
                    Destroy(attacker.gameObject);  // Remove player unit from the game
                    StartCoroutine(HUDctrl.Notify(attacker.name + " has been defeated!"));
                }
            }
        }
    }

    void InitiateCombat(Unit attacker, Unit defender, HexagonGame defenderHex)
    {
        attacker.hasAttacked = true;
        disablePlayerInput = true;

        // Start coroutines to rotate both units towards each other
        StartCoroutine(RotateTowards(attacker, defender.transform.position));
        StartCoroutine(RotateTowards(defender, attacker.transform.position));

        // Delay combat slightly to allow the units to rotate before attacking
        StartCoroutine(DelayedCombat(attacker, defender, 0.5f, defenderHex));

        StartCoroutine(HUDctrl.Notify("Combat initiated between " + attacker.owner.playerName + " soldier and " + defender.owner.playerName + " unit"));
    }

    IEnumerator DelayedCombat(Unit attacker, Unit defender, float delay, HexagonGame defenderHex)
    {
        bool defenderDead = false;
        attacker.isFighting = true;
        defender.isFighting = true;

        yield return new WaitForSeconds(delay);

        // Calculate Terrain and Weather Modifiers
        float attackModifier = 1f;  // Default no modifier
        float defenseModifier = 1f; // Default no modifier

        // Determine Terrain-based Modifiers
        if (defenderHex.hexType.Equals(gridController.mountainHex))
        {
            attackModifier -= 0.2f;
            defenseModifier += 0.3f; // Mountain gives defender 30% defense boost
        }
        else if (defenderHex.hexType.Equals(gridController.forestHex))
        {
            defenseModifier += 0.2f; // Forest gives defender 20% defense boost
        }

        // Determine Weather-based Modifiers
        WeatherState currentWeather = weatherQueue.Peek();  // Get current weather

        if (currentWeather == WeatherState.Rain)
        {
            // Rain could weaken attackers (especially ranged units)
            attackModifier -= 0.1f;  // 10% attack penalty for rain
        }
        else if (currentWeather == WeatherState.Snow)
        {
            attackModifier -= 0.15f;  // 15% attack penalty for snow
            defenseModifier += 0.1f;  // Snow could increase defense as units are harder to hit
        }
        // Clear weather gives no modifiers

        // Attacker goes first, applying modifiers
        int finalAttackDamage = Mathf.RoundToInt(attacker.atk * attackModifier);
        int finalDefenseValue = Mathf.RoundToInt(defender.def * defenseModifier);

        defender.hp -= finalAttackDamage;
        StartCoroutine(HUDctrl.Notify(attacker.owner.playerName + " soldier deals " + finalAttackDamage + " damage to defender"));

        yield return new WaitForSeconds(combatTime);

        attacker.isFighting = false;
        defender.isFighting = false;

        // Check if enemy unit is still alive
        if (defender.hp <= 0)
        {
            StartCoroutine(HUDctrl.Notify(defender.name + " has been defeated!"));
            defenderDead = true;
            StartCoroutine(KillUnit(defender, dyingTime));
            yield return new WaitForSeconds(dyingTime);
        }

        if (!defenderDead) // No retaliation if enemy is defeated
        {
            // Enemy unit retaliates
            attacker.hp -= finalDefenseValue;  // Apply defender's attack modified by the terrain/weather

            StartCoroutine(HUDctrl.Notify(defender.owner.playerName + " unit retaliates with " + finalDefenseValue + " damage to attacker"));

            // Check if player unit is still alive
            if (attacker.hp <= 0)
            {
                StartCoroutine(HUDctrl.Notify(attacker.name + " has been defeated!"));
                StartCoroutine(KillUnit(attacker, dyingTime));
                yield return new WaitForSeconds(dyingTime);
            }
        }

        disablePlayerInput = false;  // Re-enable player input once combat is done
        // Update the End Turn Button state
    }

    public IEnumerator KillUnit(Unit unit, float dieTime)
    {
        unit.isDying = true;
        unit.owner.units.Remove(unit);
        gridController.gameHexagons[unit.coordinates.x, unit.coordinates.y].tag = "MovableTerrain";
        yield return new WaitForSeconds(dieTime);
        Instantiate(gridController.deathParticle, unit.gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
        Destroy(unit.gameObject); // Remove enemy unit from the game

    }

    IEnumerator RotateTowards(Unit unit, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - unit.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));

        float rotationSpeed = 5f;  // Rotation speed can be adjusted to be faster or slower
        float rotationProgress = 0f;

        while (rotationProgress < 1f && unit != null)
        {
            // Slerp the rotation over time
            unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, rotationProgress);
            rotationProgress += Time.deltaTime * rotationSpeed;
            yield return null;  // Wait for the next frame before continuing
        }

        // Ensure the final rotation is exactly the target rotation after the loop ends
        unit.transform.rotation = lookRotation;
    }

    bool AllPlayerUnitsMoved()
    {
        foreach (var unit in player.units)
        {
            if (unit.movementExpended < unit.movementUnits)
            {
                return false;  // If any player unit still has movement left
            }
        }
        return true;
    }

    bool AllEnemyUnitsMoved()
    {
        foreach (var unit in enemy.units)
        {
            if (unit.movementExpended < unit.movementUnits)
            {
                return false;  // If any enemy unit still has movement left
            }
        }
        return true;
    }

    public void EndPlayerTurn()
    {
        foreach (var unit in player.units)
        {
            unit.movementExpended = 0;  // Reset movement for all player units
            unit.hasAttacked = false;
        }

        currentTurn = TurnState.EnemyTurn;
        StartCoroutine(StartEnemyTurn());  // Start the enemy turn
    }

    public void EndEnemyTurn()
    {
        foreach (var unit in enemy.units)
        {
            unit.movementExpended = 0; // Reset movement for all enemy units
            unit.hasAttacked = false;
        }

        currentTurn = TurnState.PlayerTurn;
        StartPlayerTurn();  // Start the next player turn
    }

    void StartPlayerTurn()
    {
        player.GenerateFundsPerTurn();
        ProgressWeather();
        // Any logic to prepare the player’s turn, like refreshing UI
        StartCoroutine(HUDctrl.Notify(player.playerName + " turn starts"));
    }

    IEnumerator StartEnemyTurn()
    {
        StartCoroutine(HUDctrl.Notify(enemy.playerName + " turn starts"));

        enemy.GenerateFundsPerTurn();

        // Step 1: Loop through each enemy unit to move them
        foreach (Unit enemyUnit in enemy.units)
        {
            // Skip units that have already moved
            if (enemyUnit.movementExpended >= enemyUnit.movementUnits)
                continue;

            // Find the nearest player unit
            Unit closestPlayerUnit = null;
            float shortestDistance = float.MaxValue;

            foreach (Unit playerUnit in player.units)
            {
                float distance = Vector3.Distance(enemyUnit.transform.position, playerUnit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPlayerUnit = playerUnit;
                }
            }

            // Step 2: Check for player city if no player units are nearby
            HexagonGame enemyUnitHex = gridController.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];
            HexagonGame closestPlayerHex = gridController.gameHexagons[closestPlayerUnit.coordinates.x, closestPlayerUnit.coordinates.y];

            // If no units are close or the city is closer than any unit, prioritize attacking the city
            float distanceToPlayerCity = Vector3.Distance(enemyUnit.transform.position, gridController.playerCity.transform.position);

            if (closestPlayerUnit == null || distanceToPlayerCity < shortestDistance)
            {
                // Check if city is close and move toward it
                HexagonGame playerCityHex = gridController.gameHexagons[gridController.playerCity.gameHex.coordinates.x, gridController.playerCity.gameHex.coordinates.y];

                if (!gridController.GetGameNeighbors(enemyUnitHex).Contains(playerCityHex))
                {
                    // Move toward the player's city if not adjacent
                    if (enemyUnit.movementExpended < enemyUnit.movementUnits)
                    {
                        List<HexagonGame> pathToCity = gridController.pathfinder.FindPath(enemyUnitHex, playerCityHex);
                        if (pathToCity != null && pathToCity.Count > 0)
                        {
                            enemyUnit.SetDestination(pathToCity);
                        }
                    }
                }
                else
                {
                    // If adjacent to the player's city, attack it
                    AttackCity(enemyUnit, playerCityHex.tag);
                }
            }
            else
            {
                // Step 3: Move towards the closest player unit if not adjacent
                if (!gridController.GetGameNeighbors(enemyUnitHex).Contains(closestPlayerHex))
                {
                    if (enemyUnit.movementExpended < enemyUnit.movementUnits)
                    {
                        List<HexagonGame> pathToPlayer = gridController.pathfinder.FindPath(enemyUnitHex, closestPlayerHex);
                        if (pathToPlayer != null && pathToPlayer.Count > 0)
                        {
                            enemyUnit.SetDestination(pathToPlayer);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Wait until all units finish moving/attacking
        bool somethingStillMoving;
        do
        {
            somethingStillMoving = false;
            foreach (Unit enemyUnit in enemy.units)
            {
                if (enemyUnit.isMoving || enemyUnit.isFighting || enemyUnit.isDying)
                {
                    somethingStillMoving = true;
                    break;
                }
            }

            foreach (Unit unit in player.units)
            {
                if (unit.isMoving || unit.isFighting || unit.isDying)
                {
                    somethingStillMoving = true;
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        } while (somethingStillMoving);

        // Step 4: After moving, check for nearby player units and attack if possible
        foreach (Unit enemyUnit in enemy.units)
        {
            if (enemyUnit.hasAttacked)
                continue;

            Unit closestPlayerUnit = null;
            float shortestDistance = float.MaxValue;

            foreach (Unit playerUnit in player.units)
            {
                float distance = Vector3.Distance(enemyUnit.transform.position, playerUnit.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPlayerUnit = playerUnit;
                }
            }

            HexagonGame enemyUnitHex = gridController.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];
            HexagonGame closestPlayerHex = gridController.gameHexagons[closestPlayerUnit.coordinates.x, closestPlayerUnit.coordinates.y];

            if (gridController.GetGameNeighbors(enemyUnitHex).Contains(closestPlayerHex))
            {
                InitiateCombat(enemyUnit, closestPlayerUnit, closestPlayerHex);
            }
        }

        // Wait until all units finish moving/attacking
        do
        {
            somethingStillMoving = false;
            foreach (Unit enemyUnit in enemy.units)
            {
                if (enemyUnit.isMoving || enemyUnit.isFighting || enemyUnit.isDying)
                {
                    somethingStillMoving = true;
                    break;
                }
            }

            foreach (Unit unit in player.units)
            {
                if (unit.isMoving || unit.isFighting || unit.isDying)
                {
                    somethingStillMoving = true;
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        } while (somethingStillMoving);

        // Step 5: End the enemy turn after processing all units
        EndEnemyTurn();
    }




    void ProgressWeather()
    {
        Array values = Enum.GetValues(typeof(WeatherState));
        Random random = new Random();
        WeatherState randomWeather = (WeatherState)values.GetValue(random.Next(values.Length));

        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
        weatherQueue.Dequeue();
        weatherQueue.Enqueue(randomWeather);

        weatherHUDManager.UpdateQueue(weatherQueue);
    }
}
