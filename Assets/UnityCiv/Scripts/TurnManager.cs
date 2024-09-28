using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (Physics.Raycast(ray, out hit))
            {
                // Select a player unit
                if (hit.collider.CompareTag("PlayerUnit")  && !EventSystem.current.IsPointerOverGameObject())
                {
                    selectedUnit = hit.transform.GetComponent<Unit>();
                }

                // If a unit is selected, move it to the selected terrain
                if (selectedUnit != null && selectedUnit.movementExpended < selectedUnit.movementUnits)
                {
                    if (hit.collider.CompareTag("MovableTerrain")  && !EventSystem.current.IsPointerOverGameObject())
                    {
                        HexagonGame targetHex = hit.transform.GetComponent<HexagonGame>();
                        HexagonGame startHex = gridController.gameHexagons[selectedUnit.coordinates.x, selectedUnit.coordinates.y];
                        selectedUnit.SetDestination(gridController.pathfinder.FindPath(startHex, targetHex));
                    }
                }

                // Detect a city click
                if (hit.collider.CompareTag("City") && !EventSystem.current.IsPointerOverGameObject())
                {
                    selectedCity = hit.collider.GetComponent<City>();
                    cityHUDManager.OpenCityHUD(selectedCity); // Open HUD for the city
                }
            }
        }
    }
}

    private void OpenCityHUD(City city)
    {
        throw new NotImplementedException();
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
