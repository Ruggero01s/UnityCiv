using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WeatherHUDManager : MonoBehaviour
{
    public GeneralHUDController hudController;
    public List<Image> weatherImages;

    public Sprite clearSprite;
    public Sprite rainSprite;
    public Sprite snowSprite;

    //Updates the weather HUD with the current weather queue
    public void UpdateQueue(Queue<TurnManager.WeatherState> queue)
    {
        for (int i = 0; i<3;i++)
        {
            TurnManager.WeatherState next = queue.ElementAt(i);
            switch (next)
            {
                case TurnManager.WeatherState.Clear:
                    weatherImages[i].sprite = clearSprite;
                    break;
                case TurnManager.WeatherState.Rain:
                    weatherImages[i].sprite = rainSprite;
                    break;
                case TurnManager.WeatherState.Snow:
                    weatherImages[i].sprite = snowSprite;
                    break;
            }
        }
    }
}
