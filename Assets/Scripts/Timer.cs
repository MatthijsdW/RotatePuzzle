using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (!GameManager.hasWon)
        {
            timerText.text = $"{(int)Time.timeSinceLevelLoad / 60}:{((int)Time.timeSinceLevelLoad % 60).ToString().PadLeft(2, '0')}";
        }
    }
}
