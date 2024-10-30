using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName; // Name of the player
    public Color ownedColor; // Color assigned to represent the player's controlled hexes visually
	public List<Unit> units = new(); // List of units under the player's control
    public City city; // Reference to the player's main city
    public GridController ctrl; // Reference to the grid controller for managing game-wide settings
    private List<HexagonGame> controlledHexes; // List of hexes currently under the player's control
    public int funds = 0; // Current funds available to the player
    public int score = 0; // Player's current score
    public int unitUpgradeLevel = 0; // Level representing player's upgrades for their units

    // Constructor to initialize player name, color, and controlled hex list
    public Player(string name, Color color)
    {
        playerName = name;
        ownedColor = color;
        controlledHexes = new List<HexagonGame>(); // Initializes controlled hexes list for tracking owned territory
        funds = 0; // Starting funds
        score = 0; // Starting score
    }

    // Adds a hexagon tile to the player's controlled list if not already controlled
    public void AddControlledHex(HexagonGame hex)
    {
        if (!controlledHexes.Contains(hex))
        {
            controlledHexes.Add(hex);
        }
    }

    // Removes a hexagon tile from the player's control if it exists in the list
    public void RemoveControlledHex(HexagonGame hex)
    {
        if (controlledHexes.Contains(hex))
        {
            controlledHexes.Remove(hex);
        }
    }

    // Generates funds each turn based on the number of controlled hexes, with a base amount
    public void GenerateFundsPerTurn()
    {
        int fertileCount = 0;
        foreach (HexagonGame hex in controlledHexes)
        {
            if (hex.hexType.Equals(ctrl.fertilePlainHex))
                fertileCount++;
        }
        int fundsToGenerate =   controlledHexes.Count * ctrl.FUNDS_PER_HEX + ctrl.FUNDS_PER_HEX_UPGRADE * city.level + 
                                fertileCount * ctrl.FERTILE_BONUS_FUNDS + city.fundsPerTurn;
        funds += fundsToGenerate; // Adds to current funds
        score += fundsToGenerate; // Updates score to match total funds earned
    }

    // Attempts to spend a specified amount of funds; returns true if successful
    public bool SpendFunds(int amount)
    {
        if (funds >= amount)
        {
            funds -= amount; // Deducts the amount from funds
            return true;
        }
        return false; // Returns false if insufficient funds
    }
}
