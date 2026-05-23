using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public TextMeshProUGUI counter;
    public TextMeshProUGUI feedback;

    public void UpdateCounter(int count)
    {
        if (counter == null)
        {
            return;
        }

        counter.text = count.ToString();
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedback == null)
        {
            return;
        }

        feedback.text = message;
        feedback.color = color;
    }
}
