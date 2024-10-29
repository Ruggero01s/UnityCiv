using UnityEngine;

public class City : MonoBehaviour
{

    public HexagonGame gameHex;
    public GridController ctrl;
    public Player owner;

    public int defenseAtk;

    public int defenseHp;
    public int fundsPerTurn;

    public int level = 0;

    public void Start()
    {
        defenseAtk = ctrl.STARTING_CITY_DEFENSE;
        defenseHp = ctrl.STARTING_CITY_HP;
        fundsPerTurn = ctrl.DEFAULT_FUNDS_PER_TURN;
    }

    public void UpgradeCity()
    {
        // Logic for building a structure
        ctrl.HUDctrl.Notify("Upgraded city! ");
        defenseAtk += ctrl.CITY_DEF_UPGRADE;
        defenseHp += ctrl.CITY_HP_UPGRADE;
        fundsPerTurn += ctrl.FUNDS_PER_TURN_UPGRADE;
        level++;
    }

    public void UpgradeUnits()
    {
        // Logic for upgrading units
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