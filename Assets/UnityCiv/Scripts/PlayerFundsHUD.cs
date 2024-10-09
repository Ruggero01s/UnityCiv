using TMPro; // If using TextMeshPro
using UnityEngine;
using UnityEngine.UI;

public class PlayerFundsHUD : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI fundsText; // For TextMeshPro

    public TextMeshProUGUI scoreText; // For TextMeshPro


    private bool firstUpdate = true;

    private GridController gridController;

    // Update the funds value 
    public void UpdateFunds(int newFunds)
    {
        fundsText.text = "Funds: " + newFunds;
    }

    public void UpdateScore(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    void Start()
    {
        gridController = FindObjectOfType<GridController>();
    }

    void Update()
    {
        if(firstUpdate)
        {
            player = gridController.player;
            firstUpdate = false;
        }
        UpdateFunds(player.GetFunds()); // Initialize with current funds
        UpdateScore(player.GetScore());
    }
}
