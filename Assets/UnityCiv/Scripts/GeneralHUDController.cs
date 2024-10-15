using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralHUDController : MonoBehaviour
{
    public Button endTurnButton;  // Reference to the End Turn Button
    public TurnManager turnManager;  // Reference to your GridController script

    public GameObject notifyPanel;  // Panel that holds the log and scroll view
    public TextMeshProUGUI textPrefab;  // Prefab for creating new messages in the log
    public Transform logContentTransform;  // The parent Transform inside the ScrollRect where messages will go
    public ScrollRect logScrollRect;  // The ScrollRect for the notification log
    private List<string> messageLog = new List<string>();  // Store all messages in a list

    void Start()
    {
        // Assign the Button's OnClick listener
        endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
        notifyPanel.SetActive(true);  // Panel is always active now since it's a log
    }

    void OnEndTurnButtonClick()
    {
        // Call the EndTurn function from GridController
        turnManager.EndPlayerTurn();
    }

    // Function to add a message to the notification log
    public void Notify(string newText)
    {
        // Add the new message to the log
        messageLog.Add(newText);

        // Instantiate a new TextMeshProUGUI message element from the prefab
        TextMeshProUGUI newMessage = Instantiate(textPrefab, logContentTransform);
        newMessage.text = newText;

        // Scroll the log to the bottom
        StartCoroutine(ScrollToBottom());

        // Optionally, limit the number of messages shown in the log (e.g., to last 50 messages)
        if (logContentTransform.childCount > 50)
        {
            Destroy(logContentTransform.GetChild(0).gameObject);  // Remove the oldest message
        }
    }

    // Coroutine to scroll the log to the bottom whenever a new message is added
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();  // Wait until the next frame to ensure the content is updated
        logScrollRect.verticalNormalizedPosition = 0;  // Scroll to the bottom
    }

    public void UpdateEndTurnButtonState(bool isEnabled)
    {
        endTurnButton.interactable = isEnabled;
    }
}
