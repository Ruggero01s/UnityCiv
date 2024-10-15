using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro


public class CityHUDManager : MonoBehaviour
{

    public GeneralHUDController hudController;
    public GameObject cityHUDPanel;  // Reference to the City HUD panel
    private City currentCity;        // The city currently selected
    private int costToUpgradeCity = 100;
    private int costToUpgradeUnits = 100;
    private int costToTrainUnit = 100;
    public Button upgradeCityButton;
    public Button upgradeUnitsButton;
    public Button trainUnitButton;
    public TextMeshProUGUI hpText; // For TextMeshPro
    public TextMeshProUGUI atkText; // For TextMeshPro
    public TextMeshProUGUI unitLevelText; // For TextMeshPro



    void Start()
    {
        upgradeCityButton.onClick.AddListener(SpendFundsOnUpgradeCity);
        upgradeUnitsButton.onClick.AddListener(SpendFundsOnUpgradeUnits);
        trainUnitButton.onClick.AddListener(SpendFundsOnTrainUnit);
        cityHUDPanel.SetActive(false);  // Hide the HUD initially
    }

    void Update()
    {
        if (cityHUDPanel.activeSelf && currentCity != null)
        {
            hpText.text = "HP: " + currentCity.defenseHp;
            atkText.text = "DEF: " +currentCity.defenseAtk;
            unitLevelText.text = "Unit LV: " + currentCity.owner.unitUpgradeLevel; 
        }
    }

    public void OpenCityHUD(City city)
    {
        currentCity = city;
        cityHUDPanel.SetActive(true);  // Show the HUD
        // Optionally, update the UI elements to reflect city details or player funds
    }

    public void CloseCityHUD()
    {
        cityHUDPanel.SetActive(false);  // Hide the HUD
        currentCity = null;
    }

    public void SpendFundsOnUpgradeCity()
    {
        if (currentCity != null && currentCity.owner.SpendFunds(costToUpgradeCity))
        {
            currentCity.UpgradeCity();
            hudController.Notify("Defense increased!");
            CloseCityHUD();  // Close the HUD after spending
        }
        else
        {
            hudController.Notify("Not enough Funds! Need " + costToUpgradeCity);
        }
    }

    public void SpendFundsOnUpgradeUnits()
    {
        if (currentCity != null && currentCity.owner.SpendFunds(costToUpgradeUnits))
        {
            currentCity.UpgradeUnits();
            hudController.Notify("Units upgraded!");
            CloseCityHUD();  // Close the HUD after spending
        }
        else
        {
           hudController.Notify("Not enough Funds! Need " + costToUpgradeUnits);
        }
    }

    public void SpendFundsOnTrainUnit()
    {
        if (currentCity != null && currentCity.owner.SpendFunds(costToTrainUnit))
        {
            if (currentCity.TrainUnit())
            {
                hudController.Notify("Trained Unit!");
                CloseCityHUD();  // Close the HUD after spending
            }
            else
               hudController.Notify("No space to spawn new unit!");

        }
        else
        {
            hudController.Notify("Not enough Funds! Need " + costToTrainUnit);
        }
    }
}
