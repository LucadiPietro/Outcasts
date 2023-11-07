using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;

public class ShieldCircularPuzzleController : Singleton<ShieldCircularPuzzleController>
{

    [Header("Settings")]
    [SerializeField] int rotationSteps = 8;
    [SerializeField][Range(0f, 2f)] float rotSpeed = 1f;
    float stepAngle;

    [Header("Components")]
    [SerializeField] ShieldSectionUi outerEdgeRt;
    [SerializeField] ShieldSectionUi innerEdgeRt;
    [SerializeField] ShieldSectionUi centerRt;
    [SerializeField] ShieldSectionUi center2Rt;
    ShieldSectionUi[] sections;

    protected override void OnAwake()
    {
        if (rotationSteps < 3)
        {
            Debug.LogError("Increase ShieldCircularPuzzleController.rotationSteps to at least 3.");
            gameObject.SetActive(false);
            return;
        }
        stepAngle = 360f / rotationSteps;
        sections = new ShieldSectionUi[] { outerEdgeRt, innerEdgeRt, centerRt, center2Rt };
        Setup();
        SetupInput();
    }

    #region Player Input

    void SetupInput()
    {
        InputManager.Instance().SetAction(ActionKey.UP, SelectUp);
        InputManager.Instance().SetAction(ActionKey.DOWN, SelectDown);
        InputManager.Instance().SetAction(ActionKey.RIGHT, RotateRight);
        InputManager.Instance().SetAction(ActionKey.LEFT, RotateLeft);
        //InputManager.Instance().SetAction(ActionKey.)
    }

    void Restart()
    {

    }

    #endregion

    #region Logic
    void Setup()
    {
        List<int> possibleStartingSteps = Enumerable.Range(1, rotationSteps - 1).ToList();
        foreach(ShieldSectionUi section in sections)
        {
            int startingStep = possibleStartingSteps.PopRandom();
            section.Setup(rotationSteps, startingStep, stepAngle, rotSpeed);
        }
        sections[selectedElementIdx].Select();
    }

    public void CheckCorrectness()
    {
        if(sections.All(sec => sec.IsCorrect()))
        {
            Debug.Log("GAME WON!!");
        }
    }

    #endregion

    #region Selection

    int selectedElementIdx = 0;

    void SelectUp(InputAction.CallbackContext ctx)
    {
        selectedElementIdx = Mathf.Clamp(selectedElementIdx - 1, 0, sections.Length - 1);
        sections[selectedElementIdx].Select();
    }

    void SelectDown(InputAction.CallbackContext ctx)
    {
        selectedElementIdx = Mathf.Clamp(selectedElementIdx + 1, 0, sections.Length - 1);
        sections[selectedElementIdx].Select();
    }

    void RotateRight(InputAction.CallbackContext ctx)
    {
        ShieldSectionUi.selected.RotateRight();
    }

    void RotateLeft(InputAction.CallbackContext ctx)
    {
        ShieldSectionUi.selected.RotateLeft();
    }

    #endregion

}
