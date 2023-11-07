using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameGoUIManager : MonoBehaviour
{
    public enum TargetState
    {
        right,
        wrong
    }

    public TextMeshPro timer;

    public GameObject textGO;

    public SpriteRenderer targetGO;

    public Color ready = Color.black;
    public Color rightAnswer = Color.green;
    public Color wrongAnswer = Color.red;

    public float transitionTime = 0.5f;

    private void Start()
    {
        targetGO.color = ready;
    }

    // Update is called once per frame
    void Update()
    {
        textGO.SetActive(GameGoLevelManager.Instance.gameState != GameGoLevelManager.GameState.Begin);
        timer.text = Mathf.RoundToInt(GameGoLevelManager.Instance.timer).ToString();
    }

    public void TargetFunction(TargetState state)
    {
        StartCoroutine(TargetRoutine(state));
    }

    IEnumerator TargetRoutine(TargetState state)
    {
        targetGO.DOColor(state == TargetState.right ? rightAnswer : wrongAnswer, 0.3f);
        yield return new WaitForSeconds(transitionTime);
        targetGO.DOColor(ready, 0.3f);
    }
}