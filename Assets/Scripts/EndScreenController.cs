using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EndScreenController : MonoBehaviour
{
    public GameObject endScreenPanel;     
    public TextMeshProUGUI outcomeText;      // Text for displaying "You win" or "You lost"
    public TextMeshProUGUI scoreText;
    public Button exitButton;    

    public float decayRate = 0.02f;

    private void Start()
    {
        endScreenPanel.SetActive(false);     // Initially hide the end screen
        exitButton.onClick.AddListener(ExitGame);  // Assign the button listener
    }

    // Call this function when the game ends
    public void ShowEndScreen(bool playerWon, int playerScore, int turnsTaken)
    {
        endScreenPanel.SetActive(true);      // Display the end screen

        // Set outcome message based on the game result
        if (playerWon)
        {
            outcomeText.text = "You won!";
        }
        else
        {
            outcomeText.text = "You lost!";
        }
        double multiplier;
        if (playerWon)
            multiplier = 1.2 + Math.Truncate(Math.Exp(-decayRate*turnsTaken)*100)/100;
        else
            multiplier = 1 + Math.Truncate(Math.Exp(-decayRate*turnsTaken)*100)/100;

        double score = Math.Floor(playerScore * multiplier);

        // Display the player's score
        scoreText.text = "Score: " + playerScore + " x " + multiplier + " = " + score;
    }

    // Function to handle the exit button
    private void ExitGame()
    {
        Application.Quit();  // Quits the application
    }
}