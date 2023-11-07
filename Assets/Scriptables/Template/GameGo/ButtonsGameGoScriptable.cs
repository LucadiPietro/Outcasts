using System.Collections.Generic;
using UnityEngine;


public enum ButtonToPress
{
    W,
    A,
    S,
    D
}


[CreateAssetMenu(menuName = "GameGo/Buttons Pattern", fileName = "Buttons Pattern")]
[System.Serializable]
public class ButtonsGameGoScriptable : ScriptableObject
{
    public List<ButtonToPress> listOfButton;

    public float timeToReachTarget = 1;
}
