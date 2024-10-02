using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class City : MonoBehaviour
{

    public HexagonGame gameHex;
    public GridController ctrl;
    public Player owner;

    public int defenseAtk;

    public int defenseHp;

    public void UpgradeCity()
    {
        // Logic for building a structure (e.g., adding defense or resource production)
        StartCoroutine(ctrl.HUDctrl.Notify("Upgraded city!"));
        defenseAtk += 5;
        defenseHp += 10;
    }

    public void UpgradeUnits()
    {
        // Logic for upgrading units (e.g., increasing their power or range)
        StartCoroutine(ctrl.HUDctrl.Notify("Upgraded units!"));

        foreach (Unit unit in owner.units)
        {
            unit.hp += 5;
            unit.atk += 3;
            unit.def += 2;
            unit.movementUnits += 1;
        }

        owner.unitUpgradeLevel += 1;
    }

    public bool TrainUnit()
    {
        // Logic for training units
        if (ctrl.SpawnUnit(owner))
            return true;
        else
            return false;
    }
}