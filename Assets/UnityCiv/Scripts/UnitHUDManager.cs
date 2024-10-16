using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UnitHUDManager : MonoBehaviour
{
    public GeneralHUDController hudController;
    public GameObject unitHUDPanel;  // Reference to the City HUD panel
    private Unit currentUnit;        // The city currently selected

    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;


    // Start is called before the first frame update
    void Start()
    {
        unitHUDPanel.SetActive(false);
    }

    void Update()
    {
        if (unitHUDPanel.activeSelf && currentUnit != null)
        {
            hpText.text = "HP: " + currentUnit.hp + "/" + currentUnit.maxHp;
            atkText.text = "ATK: " +currentUnit.atk.ToString(); 
            defText.text = "DEF: " +currentUnit.def.ToString(); 
        }
        if(currentUnit == null || currentUnit.IsDestroyed())
            unitHUDPanel.SetActive(false);
    }

    public void OpenUnitHUD(Unit unit)
    {
        currentUnit = unit;
        hpText.text = "HP: " + unit.hp + "/" + unit.maxHp;
        atkText.text = "ATK: " +unit.atk.ToString(); 
        defText.text = "DEF: " +unit.def.ToString(); 
 
        unitHUDPanel.SetActive(true);  // Show the HUD
    }

    public void CloseUnitHUD()
    {
        unitHUDPanel.SetActive(false);  // Hide the HUD
        currentUnit = null;
    }


}
