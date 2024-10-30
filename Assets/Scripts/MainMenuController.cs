using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public Button playButton;
    public Button quitButton;

    void Start()
    {
        // Add listeners to the buttons
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); // Replace with the name of your main game scene
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game"); // Useful for testing in the editor
        Application.Quit();
    }
}
