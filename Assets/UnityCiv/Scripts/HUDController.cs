using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class HUDController : MonoBehaviour
{
    public Button endTurnButton;  // Reference to the End Turn Button
    public TurnManager turnManager;  // Reference to your GridController script

    public GameObject notifyPanel;
    public TextMeshProUGUI textNotify;

    private int notifyTime = 2;

    public CityHUDManager cityHUDManager;
    public PlayerFundsHUD playerFundsHUD;

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

    public IEnumerator NotifyText(string newText){
        textNotify.text = newText;
        notifyPanel.SetActive(true);
        yield return new WaitForSeconds(notifyTime);
        notifyPanel.SetActive(false);
    }
}
