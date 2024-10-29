using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityHUDManager : MonoBehaviour
{
    public GeneralHUDController hudController; // HUD controller for notifications
    public GameObject cityHUDPanel;  // City HUD panel
    private City currentCity;        // Selected city
    public GridController ctrl;      // Grid controller reference
    public Button upgradeCityButton; // Upgrade city button
    public Button upgradeUnitsButton; // Upgrade units button
    public Button trainUnitButton;   // Train unit button
    public TextMeshProUGUI hpText;   // City HP display
    public TextMeshProUGUI atkText;  // City attack display
    public TextMeshProUGUI unitLevelText; // Unit level display

    void Start()
    {
        // Attach button click events
        upgradeCityButton.onClick.AddListener(SpendFundsOnUpgradeCity);
        upgradeUnitsButton.onClick.AddListener(SpendFundsOnUpgradeUnits);
        trainUnitButton.onClick.AddListener(SpendFundsOnTrainUnit);
        
        cityHUDPanel.SetActive(false);  // Hide HUD initially
    }

    void Update()
    {
        // Update HUD text if active and city selected
        if (cityHUDPanel.activeSelf && currentCity != null)
        {
            hpText.text = "HP: " + currentCity.defenseHp;
            atkText.text = "DEF: " + currentCity.defenseAtk;
            unitLevelText.text = "Unit LV: " + currentCity.owner.unitUpgradeLevel; 
        }
    }

    public void OpenCityHUD(City city)
    {
        currentCity = city; 
        cityHUDPanel.SetActive(true);  // Show HUD
    }

    public void CloseCityHUD()
    {
        cityHUDPanel.SetActive(false);  // Hide HUD
        currentCity = null; 
    }

    public void SpendFundsOnUpgradeCity()
    {
        // Upgrade city if funds are sufficient
        if (currentCity != null && currentCity.owner.SpendFunds(ctrl.CITY_UPGRADE_COST))
        {
            currentCity.UpgradeCity();
            hudController.Notify("Defense increased!");
        }
        else
        {
            hudController.Notify("Not enough Funds! Need " + ctrl.CITY_UPGRADE_COST);
        }
    }

    public void SpendFundsOnUpgradeUnits()
    {
        // Upgrade units if funds are sufficient
        if (currentCity != null && currentCity.owner.SpendFunds(ctrl.UNIT_UPGRADE_COST))
        {
            currentCity.UpgradeUnits();
            hudController.Notify("Units upgraded!");
        }
        else
        {
            hudController.Notify("Not enough Funds! Need " + ctrl.UNIT_UPGRADE_COST);
        }
    }

    public void SpendFundsOnTrainUnit()
    {
        // Train unit if funds are sufficient and space available
        if (currentCity != null && currentCity.owner.SpendFunds(ctrl.TRAIN_UNIT_COST))
        {
            if (currentCity.TrainUnit())
            {
                hudController.Notify("Trained Unit!");
            }
            else
            {
                hudController.Notify("No space to spawn new unit!");
            }
        }
        else
        {
            hudController.Notify("Not enough Funds! Need " + ctrl.TRAIN_UNIT_COST);
        }
    }
}
