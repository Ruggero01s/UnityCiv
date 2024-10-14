using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class GeneralHUDController : MonoBehaviour
{
    public Button endTurnButton;  // Reference to the End Turn Button
    public TurnManager turnManager;  // Reference to your GridController script

    public GameObject notifyPanel;
    public TextMeshProUGUI textNotify;

    private float notifyTime = 3f;

    void Start()
    {
        // Assign the Button's OnClick listener
        endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
        notifyPanel.SetActive(false);
    }

    void OnEndTurnButtonClick()
    {
        // Call the EndTurn function from GridController
        turnManager.EndPlayerTurn();
    }

    public IEnumerator Notify(string newText)
    {
        textNotify.text = newText;
        notifyPanel.SetActive(true);
        yield return new WaitForSeconds(notifyTime);
        notifyPanel.SetActive(false);
    }

    public void UpdateEndTurnButtonState(bool isEnabled)
    {
        endTurnButton.interactable = isEnabled;
    }
}