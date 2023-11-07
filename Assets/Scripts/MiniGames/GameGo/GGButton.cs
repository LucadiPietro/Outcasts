using System;
using UnityEngine;

public class GGButton : MonoBehaviour
{
    public KeyCode keyToPass;

    public void Enter()
    {
        GameGoLevelManager.Instance.SetUpButton(keyToPass);
    }
}
