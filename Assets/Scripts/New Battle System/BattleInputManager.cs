using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum Keys
{
    A,
    B,
    X,
    Y,
    LB,
    RB,
    LT,
    RT,
    //START,
    NONE
}

public static class KeysUtil
{
    public static Keys ByString(string s)
    {
        return (Keys)Enum.Parse(typeof(Keys), s);
    }
}

public class BattleInputManager : MonoBehaviour
{
    public static BattleInputManager Instance { get; private set; }
    Controls controls;
    public Dictionary<Keys, InputAction> keyMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        controls = new Controls();
        controls.Enable();
        keyMap = new Dictionary<Keys, InputAction>();
        keyMap.Add(Keys.A, controls.Keys.A);
        keyMap.Add(Keys.B, controls.Keys.B);
        keyMap.Add(Keys.X, controls.Keys.X);
        keyMap.Add(Keys.Y, controls.Keys.Y);
        keyMap.Add(Keys.LB, controls.Keys.LB);
        keyMap.Add(Keys.RB, controls.Keys.RB);
        keyMap.Add(Keys.LT, controls.Keys.LT);
        keyMap.Add(Keys.RT, controls.Keys.RT);

        //keyMap.Add(Keys.START, controls.Keys.StartPause);
    }
}