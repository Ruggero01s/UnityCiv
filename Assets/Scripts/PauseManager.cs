using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseUI;
    public TurnManager turnManager;
    public CameraController ctrlCamera;

    public Button restartButton;
    public Button menuButton;

    public bool paused;

    void Start()
    {
        menuButton.onClick.AddListener(Menu);
    }

    void Update()
    {
        if (!turnManager.gameEnded)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                PauseToggle();
        }
    }

    void PauseToggle()
    {
        paused = !paused;
        turnManager.disablePlayerInput = paused;
        ctrlCamera.disableMovement = paused;
        pauseUI.SetActive(paused);
        if (paused)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    public void Menu()
    {
        PauseToggle();
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("GameScene");
    }
}