using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenController : MonoBehaviour
{
    public GameObject endScreenPanel;     
    public TextMeshProUGUI outcomeText;      // Text for displaying "You win" or "You lost"
    public TextMeshProUGUI scoreText;
    public Button exitButton;                

    private void Start()
    {
        endScreenPanel.SetActive(false);     // Initially hide the end screen
        exitButton.onClick.AddListener(ExitGame);  // Assign the button listener
    }

    // Call this function when the game ends
    public void ShowEndScreen(bool playerWon, int playerScore)
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

        // Display the player's score
        scoreText.text = "Score: " + playerScore.ToString();
    }

    // Function to handle the exit button
    private void ExitGame()
    {
        Application.Quit();  // Quits the application
    }
}