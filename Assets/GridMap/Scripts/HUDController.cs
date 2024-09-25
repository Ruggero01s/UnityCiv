using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Button endTurnButton;  // Reference to the End Turn Button
    public TurnManager turnManager;  // Reference to your GridController script

    void Start()
    {
        // Assign the Button's OnClick listener
        endTurnButton.onClick.AddListener(OnEndTurnButtonClick);
    }

    void OnEndTurnButtonClick()
    {
        // Call the EndTurn function from GridController
        turnManager.EndPlayerTurn();
    }
}
