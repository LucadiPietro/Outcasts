using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    private static InputManager instance = null;

    private InputManager()
    {
        InputMapping mapping = new InputMapping();
        mapping.Enable();
        actionMap = mapping.ActionMap;
        actionToDelegateMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>();
    }

    public static InputManager Instance()
    {
        return instance ??= new InputManager();
    }


    static InputMapping.ActionMapActions actionMap;
    static Dictionary<InputAction, Action<InputAction.CallbackContext>> actionToDelegateMap;

    public void SetAction(ActionKey key, Action<InputAction.CallbackContext> _delegate)
    {
        InputAction action = KeyToAction(key);
        CheckAlreadyMappedAction(action);
        Associate(action, _delegate);
    }

    void Associate(InputAction action, Action<InputAction.CallbackContext> _delegate)
    {
        actionToDelegateMap[action] = _delegate;
        action.started += _delegate;
    }

    void CheckAlreadyMappedAction(InputAction action)
    {
        if (actionToDelegateMap.ContainsKey(action))
        {
            Debug.LogWarning(
                $"CAREFUL! You are overriding an already mapped action {action.name}. Ensure this was made on purpose.");
            action.started -= actionToDelegateMap[action];
        }
    }

    public InputAction KeyToAction(ActionKey key)
    {
        return key switch
        {
            ActionKey.UP => actionMap.Up,
            ActionKey.DOWN => actionMap.Down,
            ActionKey.LEFT => actionMap.Left,
            ActionKey.RIGHT => actionMap.Right,
            ActionKey.NORTH => actionMap.North,
            ActionKey.SOUTH => actionMap.South,
            ActionKey.EAST => actionMap.East,
            ActionKey.WEST => actionMap.West,
            ActionKey.START => actionMap.Start,
            ActionKey.SELECT => actionMap.Select,
            ActionKey.LT => actionMap.LT,
            ActionKey.RT => actionMap.RT,
            _ => actionMap.Up
        };
    }
}

public enum ActionKey
{
    UP,
    DOWN,
    RIGHT,
    LEFT,
    NORTH,
    SOUTH,
    EAST,
    WEST,
    START,
    SELECT,
    LT,
    RT
}