using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    public Color ownedColor; // Color assigned to the player's controlled hexes
	public List<Unit> units = new List<Unit>();

    private List<HexagonGame> controlledHexes;
    private int funds = 0;

    private int score = 0;

    public int unitUpgradeLevel;


    public Player(string name, Color color)
    {
        playerName = name;
        ownedColor = color;
        controlledHexes = new List<HexagonGame>();
        funds = 0;
        score = 0;
    }

    // Adds a hex to the player's controlled list
    public void AddControlledHex(HexagonGame hex)
    {
        if (!controlledHexes.Contains(hex))
        {
            controlledHexes.Add(hex);
        }
    }

    // Removes a hex from the player's control
    public void RemoveControlledHex(HexagonGame hex)
    {
        if (controlledHexes.Contains(hex))
        {
            controlledHexes.Remove(hex);
        }
    }

    // Each turn, this function generates funds based on controlled hexes
    public void GenerateFundsPerTurn()
    {
        // Example: 10 units of funds per hex
        int fundsToGenerate = controlledHexes.Count * 10;
        funds += fundsToGenerate;
        score += fundsToGenerate;
    }

    // Function to spend funds (e.g., on units or buildings)
    public bool SpendFunds(int amount)
    {
        if (funds >= amount)
        {
            funds -= amount;
            return true;
        }
        return false;
    }

    public int GetFunds()
    {
        return funds;
    }

    public int GetScore()
    {
        return score;
    }
}
