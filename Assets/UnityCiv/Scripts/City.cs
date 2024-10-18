using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

public class City : MonoBehaviour
{

    public HexagonGame gameHex;
    public GridController ctrl;
    public Player owner;

    public int defenseAtk = 5;

    public int defenseHp = 50;

    public int level = 1;


    public void UpgradeCity()
    {
        // Logic for building a structure (e.g., adding defense or resource production)
        ctrl.HUDctrl.Notify("Upgraded city!");
        defenseAtk += 5;
        defenseHp += 10;
        level++;
    }

    public void UpgradeUnits()
    {
        // Logic for upgrading units (e.g., increasing their power or range)
        ctrl.HUDctrl.Notify("Upgraded units!");

        foreach (Unit unit in owner.units)
        {
            unit.maxHp += ctrl.UNIT_HP_UPGRADE;
            unit.hp += ctrl.UNIT_HP_UPGRADE;
            unit.atk += ctrl.UNIT_ATK_UPGRADE;
            unit.def += ctrl.UNIT_DEF_UPGRADE;
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