using System.Collections.Generic;
using UnityEngine;

public class City : MonoBehaviour
{
    public HexagonGame gameHex;
    public Player owner;

    public int defenseAtk;

    public int defenseHp;

    public void UpgradeCity()
    {
        // Logic for building a structure (e.g., adding defense or resource production)
        Debug.Log("Upgrading city!");
    }

    public void UpgradeUnits()
    {
        // Logic for upgrading units (e.g., increasing their power or range)
        Debug.Log("Upgrading units!");
    }

    public void TrainUnit()
    {
        // Logic for training units
        Debug.Log("Training units!");
    }
}