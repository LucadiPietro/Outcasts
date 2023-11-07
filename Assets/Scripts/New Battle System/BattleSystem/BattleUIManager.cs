using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public TextMeshProUGUI counter;

    public void UpdateCounter(int count)
    {
        counter.text = count.ToString();
    }
}
