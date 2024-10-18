using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;
//todo city killing an enemy unit breaks ai
//todo finish end game
public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public int turnCount = 0;
    public TurnState currentTurn;
    public Player enemy;
    public Player player;

    private Unit selectedUnit;
    private City selectedCity;

    public CityHUDManager cityHUDManager;
    public UnitHUDManager unitHUDManager;
    public WeatherHUDManager weatherHUDManager;

    public GeneralHUDController HUDctrl;
    public EndScreenController endScreenCtrl;  // Reference to the EndScreenController

    public GridController ctrl;

    private int combatTime = 3;
    private int dyingTime = 3;

    public bool disablePlayerInput = false;

    public enum WeatherState { Clear, Rain, Snow }

    public Queue<WeatherState> weatherQueue;

    void Start()
    {
        currentTurn = TurnState.PlayerTurn;  // Player starts first
        ctrl = FindObjectOfType<GridController>();
        cityHUDManager = FindObjectOfType<CityHUDManager>();
        unitHUDManager = FindObjectOfType<UnitHUDManager>();
        weatherHUDManager = FindObjectOfType<WeatherHUDManager>();

        HUDctrl = FindObjectOfType<GeneralHUDController>();


        // Load the unit lists from the GridController
        player = ctrl.player;
        enemy = ctrl.enemy;

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
                            HexagonGame selectedUnitHex = ctrl.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];
                            HexagonGame enemyUnitHex = ctrl.gameHexagons[clickedEnemyUnit.coordinates.x, clickedEnemyUnit.coordinates.y];

                            // Check if enemy is on a neighboring hex
                            if (ctrl.GetGameNeighbors(selectedUnitHex).Contains(enemyUnitHex) && !selectedUnit.hasAttacked)
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
                        GridController ctrl = FindObjectOfType<GridController>();
                        HexagonGame startHex = ctrl.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];

                        // Move the selected unit to the new terrain if it's a valid destination
                        if (targetHex != null && selectedUnit.movementExpended < selectedUnit.movementUnits && !selectedUnit.isMoving)
                        {
                            selectedUnit.SetDestination(ctrl.pathfinder.FindPath(startHex, targetHex));
                        }
                    }

                    // Detect a city click
                    if (hit.collider.CompareTag("PlayerCity") && !EventSystem.current.IsPointerOverGameObject())
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
                        HexagonGame selectedUnitHex = ctrl.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];
                        HexagonGame cityHex = hit.transform.GetComponent<HexagonGame>();

                        // Check if the city is on a neighboring hex
                        if (ctrl.GetGameNeighbors(selectedUnitHex).Contains(cityHex))
                        {
                            // Initiate combat with the city
                            StartCoroutine(AttackCity(selectedUnit, cityHex.tag));
                        }
                    }
                }
            }
        }
    }

    private IEnumerator AttackCity(Unit attacker, string tag) //TODO test
    {
        attacker.hasAttacked = true;
        attacker.isFighting = true;
        disablePlayerInput = true;
        if (tag == "EnemyCity")
        {
            Vector3 cityPos = enemy.city.transform.position;
            cityPos.y = 3f;
            StartCoroutine(RotateTowards(attacker, cityPos));
            Vector3 direction = (attacker.gameObject.transform.position - cityPos).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));
            Instantiate(ctrl.cityAttackParticle, enemy.city.gameObject.transform.position, lookRotation);
            
            yield return new WaitForSeconds(combatTime);

            // reduce city health by the attacking unit's attack value
            enemy.city.defenseHp -= attacker.atk;
            HUDctrl.Notify(attacker.owner.playerName + " unit attacks the enemy city doing " + attacker.atk + " damage");
            HUDctrl.Notify("Enemy city has " + enemy.city.defenseHp + " hp left");
            attacker.isFighting = false;
            // Check if the city has been destroyed
            if (enemy.city.defenseHp <= 0)
            {
                StartCoroutine(DestroyCity(enemy));
            }
            else
            {
                attacker.hp -= enemy.city.defenseAtk;

                if (attacker.hp <= 0)
                {
                    StartCoroutine(KillUnit(attacker, dyingTime));
                    yield return new WaitForSeconds(2);
                    Destroy(attacker.gameObject);  // Remove unit from the game
                    HUDctrl.Notify(attacker.owner.playerName + " unit has been defeated!");
                }
            }
        }
        else
        {
            Vector3 cityPos = player.city.transform.position;
            cityPos.y = 3f;
            StartCoroutine(RotateTowards(attacker, cityPos));
            Vector3 direction = (attacker.gameObject.transform.position - cityPos).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));
            Instantiate(ctrl.cityAttackParticle, player.city.gameObject.transform.position, lookRotation);
            StartCoroutine(RotateTowards(attacker, player.city.transform.position));

            // reduce city health by the attacking unit's attack value
            player.city.defenseHp -= attacker.atk;
            HUDctrl.Notify(attacker.owner.playerName + " unit attacks the player city doing " + attacker.atk + " damage");
            HUDctrl.Notify("Plyaer city has " + player.city.defenseHp + " hp left");
            attacker.isFighting = false;
            // Check if the city has been destroyed
            if (player.city.defenseHp <= 0)
            {
                StartCoroutine(DestroyCity(player));
            }
            else
            {
                attacker.hp -= player.city.defenseAtk;

                if (attacker.hp <= 0)
                {
                    StartCoroutine(KillUnit(attacker, dyingTime));
                    yield return new WaitForSeconds(2);
                    HUDctrl.Notify(attacker.owner.playerName + " unit has been defeated!");
                }
            }
        }
        disablePlayerInput = false;
    }

    public IEnumerator DestroyCity(Player loser)
    {
        if (loser == enemy)
        {
            Instantiate(ctrl.cityDestroyedParticle, loser.city.transform);
            yield return new WaitForSeconds(5);
            Destroy(loser.city.gameObject);
            int playerScore = player.GetScore();
            endScreenCtrl.ShowEndScreen(true, playerScore);  // Player wins
        }
        else if (loser == player)
        {
            Instantiate(ctrl.cityDestroyedParticle, loser.city.transform);
            yield return new WaitForSeconds(5);
            Destroy(loser.city.gameObject);
            int playerScore = player.GetScore();
            endScreenCtrl.ShowEndScreen(false, playerScore);  // Player loses
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

        HUDctrl.Notify("Combat initiated between " + attacker.owner.playerName + " soldier and " + defender.owner.playerName + " unit");
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
        if (defenderHex.hexType.Equals(ctrl.mountainHex))
        {
            attackModifier -= 0.2f;
            defenseModifier += 0.3f; // Mountain gives defender 30% defense boost
        }
        else if (defenderHex.hexType.Equals(ctrl.forestHex))
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
        HUDctrl.Notify(attacker.owner.playerName + " soldier deals " + finalAttackDamage + " damage to defender");

        yield return new WaitForSeconds(combatTime);

        attacker.isFighting = false;
        defender.isFighting = false;

        // Check if enemy unit is still alive
        if (defender.hp <= 0)
        {
            HUDctrl.Notify(defender.owner.playerName + " unit has been defeated!");
            defenderDead = true;
            StartCoroutine(KillUnit(defender, dyingTime));
            yield return new WaitForSeconds(dyingTime);
        }

        if (!defenderDead) // No retaliation if enemy is defeated
        {
            // Enemy unit retaliates
            attacker.hp -= finalDefenseValue;  // Apply defender's attack modified by the terrain/weather

            HUDctrl.Notify(defender.owner.playerName + " unit retaliates with " + finalDefenseValue + " damage to attacker");

            // Check if player unit is still alive
            if (attacker.hp <= 0)
            {
                HUDctrl.Notify(attacker.owner.playerName + " unit has been defeated!");
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
        ctrl.gameHexagons[unit.coordinates.x, unit.coordinates.y].tag = "MovableTerrain";
        yield return new WaitForSeconds(dieTime);
        Instantiate(ctrl.deathParticle, unit.gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
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
        turnCount++;
        HUDctrl.Notify("---------Turn " + turnCount + " starts---------");
        player.GenerateFundsPerTurn();
        ProgressWeather();
        HUDctrl.Notify(player.playerName + " turn starts");
    }

    IEnumerator StartEnemyTurn()
    {
        HUDctrl.Notify(enemy.playerName + " turn starts");

        // Economic actions (funds generation, upgrades, etc.)
        yield return StartCoroutine(HandleAIEconomics());

        // Move all units
        yield return StartCoroutine(MoveEnemyUnits());

        // Wait for all units to finish their actions (moving, fighting, etc.)
        yield return StartCoroutine(WaitForUnitsToFinishActions());

        // After moving, check for attacks
        yield return StartCoroutine(HandleUnitAttacks());

        // Final wait for all units to finish after potential attacks
        yield return StartCoroutine(WaitForUnitsToFinishActions());

        // End the enemy turn
        EndEnemyTurn();
    }

    IEnumerator HandleAIEconomics()
    {
        enemy.GenerateFundsPerTurn();

        int unitCount = enemy.units.Count;
        int cityLevel = enemy.city.level;
        int unitLevel = enemy.unitUpgradeLevel;

        // Priority 1: Train new units if few units exist //todo add variable for unit max enemy
        if (unitCount < 5 && enemy.GetFunds() >= ctrl.TRAIN_UNIT_COST && enemy.SpendFunds(ctrl.TRAIN_UNIT_COST))
        {
            if (enemy.city.TrainUnit())
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Priority 2: Upgrade units if the AI has many units
        else if (unitCount >= 5 && enemy.GetFunds() >= ctrl.UNIT_UPGRADE_COST && enemy.SpendFunds(ctrl.UNIT_UPGRADE_COST))
        {
            enemy.city.UpgradeUnits();
            yield return new WaitForSeconds(0.5f);
        }

        // Priority 3: Upgrade the city if unit level exceeds city level
        else if (unitLevel > cityLevel && enemy.GetFunds() >= ctrl.CITY_UPGRADE_COST && enemy.SpendFunds(ctrl.CITY_UPGRADE_COST))
        {
            enemy.city.UpgradeCity();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MoveEnemyUnits()
    {
        if (player.units.Count == 0)
        {
            // If no player units, AI should focus solely on attacking the city
            foreach (Unit enemyUnit in enemy.units)
            {
                if (enemyUnit.movementExpended >= enemyUnit.movementUnits)
                    continue;

                HexagonGame enemyUnitHex = ctrl.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];
                yield return StartCoroutine(MoveUnitTowardsCity(enemyUnit, enemyUnitHex));
            }
            yield break;
        }

        foreach (Unit enemyUnit in enemy.units)
        {
            if (enemyUnit.movementExpended >= enemyUnit.movementUnits)
                continue;

            Unit closestPlayerUnit = FindClosestPlayerUnit(enemyUnit);
            HexagonGame enemyUnitHex = ctrl.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];

            if (ShouldTargetCity(closestPlayerUnit, enemyUnit))
            {
                yield return StartCoroutine(MoveUnitTowardsCity(enemyUnit, enemyUnitHex));
            }
            else
            {
                yield return StartCoroutine(MoveUnitTowardsPlayer(enemyUnit, closestPlayerUnit));
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    Unit FindClosestPlayerUnit(Unit enemyUnit)
    {
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

        return closestPlayerUnit;
    }

    bool ShouldTargetCity(Unit closestPlayerUnit, Unit enemyUnit)
    {
        float distanceToPlayerCity = Vector3.Distance(enemyUnit.transform.position, player.city.transform.position);
        return closestPlayerUnit == null || distanceToPlayerCity < Vector3.Distance(enemyUnit.transform.position, closestPlayerUnit.transform.position);
    }

    IEnumerator MoveUnitTowardsCity(Unit enemyUnit, HexagonGame enemyUnitHex)
    {
        HexagonGame playerCityHex = ctrl.gameHexagons[player.city.gameHex.coordinates.x, player.city.gameHex.coordinates.y];

        if (!ctrl.GetGameNeighbors(enemyUnitHex).Contains(playerCityHex))
        {
            List<HexagonGame> pathToCity = ctrl.pathfinder.FindPath(enemyUnitHex, playerCityHex);
            if (pathToCity != null && pathToCity.Count > 0)
            {
                enemyUnit.SetDestination(pathToCity);
            }
        }
        else
        {
            yield return StartCoroutine(AttackCity(enemyUnit, playerCityHex.tag));
        }
    }

    IEnumerator MoveUnitTowardsPlayer(Unit enemyUnit, Unit closestPlayerUnit)
    {
        HexagonGame enemyUnitHex = ctrl.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];
        HexagonGame closestPlayerHex = ctrl.gameHexagons[closestPlayerUnit.coordinates.x, closestPlayerUnit.coordinates.y];

        if (!ctrl.GetGameNeighbors(enemyUnitHex).Contains(closestPlayerHex))
        {
            List<HexagonGame> pathToPlayer = ctrl.pathfinder.FindPath(enemyUnitHex, closestPlayerHex);
            if (pathToPlayer != null && pathToPlayer.Count > 0)
            {
                enemyUnit.SetDestination(pathToPlayer);
            }
        }
        yield return null;
    }

    IEnumerator WaitForUnitsToFinishActions()
    {
        bool somethingStillMoving;
        do
        {
            somethingStillMoving = CheckIfUnitsStillMoving(enemy.units) || CheckIfUnitsStillMoving(player.units);
            yield return new WaitForSeconds(1f);
        } while (somethingStillMoving);
    }

    bool CheckIfUnitsStillMoving(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            if (unit.isMoving || unit.isFighting || unit.isDying)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator HandleUnitAttacks()
    {
        if (player.units.Count == 0)
        {
            // If no player units, skip unit-to-unit combat and focus on city attacks
            foreach (Unit enemyUnit in enemy.units)
            {
                yield return StartCoroutine(HandleCityAttacks(enemyUnit));
            }
            yield break;
        }

        foreach (Unit enemyUnit in enemy.units)
        {
            if (!enemyUnit.hasAttacked)
            {
                Unit closestPlayerUnit = FindClosestPlayerUnit(enemyUnit);
                if (closestPlayerUnit != null)
                {
                    HexagonGame closestPlayerHex = ctrl.gameHexagons[closestPlayerUnit.coordinates.x, closestPlayerUnit.coordinates.y];
                    HexagonGame enemyUnitHex = ctrl.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];

                    if (ctrl.GetGameNeighbors(enemyUnitHex).Contains(closestPlayerHex))
                    {
                        InitiateCombat(enemyUnit, closestPlayerUnit, closestPlayerHex);
                    }
                }
            }

            yield return StartCoroutine(HandleCityAttacks(enemyUnit));
        }
    }


    IEnumerator HandleCityAttacks(Unit enemyUnit)
    {
        HexagonGame enemyUnitHex = ctrl.gameHexagons[enemyUnit.coordinates.x, enemyUnit.coordinates.y];

        foreach (HexagonGame hex in ctrl.GetGameNeighbors(enemyUnitHex))
        {
            if (hex.CompareTag("PlayerCity"))
            {
                yield return StartCoroutine(AttackCity(enemyUnit, hex.tag));
            }
        }
    }

    void ProgressWeather()
    {
        Array values = Enum.GetValues(typeof(WeatherState));
        Random random = new();
        WeatherState randomWeather = (WeatherState)values.GetValue(random.Next(values.Length));

        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
        weatherQueue.Enqueue((WeatherState)values.GetValue(random.Next(values.Length)));
        weatherQueue.Dequeue();
        weatherQueue.Enqueue(randomWeather);

        weatherHUDManager.UpdateQueue(weatherQueue);
    }
}
