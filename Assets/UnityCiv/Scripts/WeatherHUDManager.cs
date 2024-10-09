using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherHUDManager : MonoBehaviour
{
    public GeneralHUDController hudController;
    public List<Image> weatherImages;

    public Sprite clearSprite;
    public Sprite rainSprite;
    public Sprite snowSprite;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {

    }

    public void UpdateQueue(Queue<TurnManager.WeatherState> queue)
    {
        Queue<TurnManager.WeatherState> copy = new(queue);
        for (int i = 0; i<3;i++)
        {
            TurnManager.WeatherState next = copy.Dequeue();
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
