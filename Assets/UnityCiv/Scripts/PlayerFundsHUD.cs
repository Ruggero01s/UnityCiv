using TMPro;
using UnityEngine;

public class PlayerFundsHUD : MonoBehaviour
{
    public Player player;
    public TextMeshProUGUI fundsText;

    public TextMeshProUGUI scoreText;


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
        UpdateFunds(player.funds);
        UpdateScore(player.score);
    }
}
