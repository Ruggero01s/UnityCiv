using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//TODO change all debug.log with notifications

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PlayerTurn, EnemyTurn }
    public TurnState currentTurn;
    public Player enemy;
    public Player player;

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    private Unit selectedUnit;
    private City selectedCity;

    private CityHUDManager cityHUDManager;
    private GridController gridController;

    private int combatTime = 3;
    private int dyingTime = 3;


    void Start()
    {
        currentTurn = TurnState.PlayerTurn;  // Player starts first
        gridController = FindObjectOfType<GridController>();
        cityHUDManager = FindObjectOfType<CityHUDManager>();


        // Load the unit lists from the GridController
        playerUnits = gridController.playerUnits;
        enemyUnits = gridController.enemyUnits;
        player = gridController.player;
        enemy = gridController.enemy;
    }

    void Update()
    {

        HandleInput();  // Manage player input for selecting units and issuing commands
    }

    void HandleInput()
    {
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
                            selectedUnit = clickedUnit;
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
                            if (gridController.GetGameNeighbors(selectedUnitHex).Contains(enemyUnitHex))
                            {
                                // Initiate combat between selected unit and enemy unit
                                InitiateCombat(selectedUnit, clickedEnemyUnit, enemyUnitHex);
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
                        if (targetHex != null && selectedUnit.movementExpended < selectedUnit.movementUnits)
                        {
                            selectedUnit.SetDestination(gridController.pathfinder.FindPath(startHex, targetHex));
                        }
                    }

                    // Detect a city click
                    if (hit.collider.CompareTag("City") && !EventSystem.current.IsPointerOverGameObject())
                    {
                        selectedCity = hit.collider.GetComponent<City>();
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

    private void AttackCity(Unit attackingUnit, string tag) //TODO test
    {
        if (tag == "EnemyCity")
        {
            // Example logic: reduce city health by the attacking unit's attack value
            gridController.enemyCity.defenseHp -= attackingUnit.atk;

            // Check if the city has been destroyed
            if (gridController.enemyCity.defenseHp <= 0)
            {
                //TODO implement win 
            }
            else
            {
                attackingUnit.hp -= gridController.enemyCity.defenseAtk;

                if (attackingUnit.hp <= 0)
                {
                    Destroy(attackingUnit.gameObject);  // Remove player unit from the game
                    Debug.Log(attackingUnit.name + " has been defeated!");
                }
            }
        }
        else
        {
            // Example logic: reduce city health by the attacking unit's attack value
            gridController.playerCity.defenseHp -= attackingUnit.atk;

            // Check if the city has been destroyed
            if (gridController.playerCity.defenseHp <= 0)
            {
                //TODO implement win 
            }
            else
            {
                attackingUnit.hp -= gridController.playerCity.defenseAtk;

                if (attackingUnit.hp <= 0)
                {
                    Destroy(attackingUnit.gameObject);  // Remove player unit from the game
                    Debug.Log(attackingUnit.name + " has been defeated!");
                }
            }
        }
    }

    void InitiateCombat(Unit attacker, Unit defender, HexagonGame defenderHex) //TODO terrain/weather modifiers
    {
        // Start coroutines to rotate both units towards each other
        StartCoroutine(RotateTowards(attacker, defender.transform.position));
        StartCoroutine(RotateTowards(defender, attacker.transform.position));

        // Delay combat slightly to allow the units to rotate before attacking
        StartCoroutine(DelayedCombat(attacker, defender, 0.5f));  // 0.5 second delay for example
    }

    IEnumerator DelayedCombat(Unit attacker, Unit defender, float delay)
    {
        bool defenderDead = false;

        attacker.isFighting = true;
        defender.isFighting = true;
        yield return new WaitForSeconds(delay);

        Debug.Log("Combat initiated between " + attacker.name + " and " + defender.name);

        // Player unit attacks first
        defender.hp -= attacker.atk;
        Debug.Log(attacker.name + " deals " + attacker.atk + " damage to " + defender.name);


        yield return new WaitForSeconds(combatTime);

        attacker.isFighting = false;
        defender.isFighting = false;

        // Check if enemy unit is still alive
        if (defender.hp <= 0)
        {
            defenderDead = true;
            defender.isDying = true;  // No retaliation if enemy is defeated
            yield return new WaitForSeconds(dyingTime);
            Destroy(defender.gameObject);  // Remove enemy unit from the game
            Debug.Log(defender.name + " has been defeated!");
        }

        if (!defenderDead)
        {
            // Enemy unit retaliates
            attacker.hp -= defender.def;
            Debug.Log(defender.name + " retaliates with " + defender.def + " damage to " + attacker.name);

            // Check if player unit is still alive
            if (attacker.hp <= 0)
            {
                attacker.isDying = true;  // No retaliation if enemy is defeated
                yield return new WaitForSeconds(dyingTime);
                Destroy(attacker.gameObject);  // Remove player unit from the game
                Debug.Log(attacker.name + " has been defeated!");
            }
        }
    }


    IEnumerator RotateTowards(Unit unit, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - unit.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, direction.z));

        float rotationSpeed = 5f;  // Rotation speed can be adjusted to be faster or slower
        float rotationProgress = 0f;

        while (rotationProgress < 1f)
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
        foreach (var unit in playerUnits)
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
        foreach (var unit in enemyUnits)
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
        foreach (var unit in playerUnits)
        {
            unit.movementExpended = 0;  // Reset movement for all player units
        }

        currentTurn = TurnState.EnemyTurn;
        StartEnemyTurn();  // Start the enemy turn
    }

    public void EndEnemyTurn()
    {
        foreach (var unit in enemyUnits)
        {
            unit.movementExpended = 0;  // Reset movement for all enemy units
        }

        currentTurn = TurnState.PlayerTurn;
        StartPlayerTurn();  // Start the next player turn
    }

    void StartPlayerTurn()
    {
        player.GenerateFundsPerTurn();
        // Any logic to prepare the player’s turn, like refreshing UI
        Debug.Log("Player's turn starts");
    }

    void StartEnemyTurn()
    {
        // Any AI logic for enemy turn
        Debug.Log("Enemy's turn starts");
        // Implement AI behavior here
        EndEnemyTurn();
    }
}