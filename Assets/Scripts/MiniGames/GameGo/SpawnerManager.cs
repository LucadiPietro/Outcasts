using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    #region Configuration

    [Header("Configuration Panel")] [Space(10)] [Header("Time To Reach Circle")]
    public float timeToReach = 0;

    public float timeToReachFirstButton = 5;

    [Header("Time To Disappear Button")] public float timeToDisappear = 0.5f;

    #endregion

    #region DevConfiguration

    [Header("Dev Panel")] [Space(10)] public GGButton prefab;
    public Transform target;

    public List<GGButton> list;

    public SDictionary<string, Sprite> listOfSprite;

    private Vector3 initPosition;

    #endregion

    private void Start()
    {
        initPosition = transform.position;
    }

    public void GameInit(ButtonsGameGoScriptable pattern)
    {
        transform.position = initPosition;
        foreach (var ele in list)
        {
            Destroy(ele.gameObject);
        }
        list.Clear();
        foreach (var button in pattern.listOfButton)
        {
            var newButton = Instantiate(prefab, transform);
            newButton.transform.localPosition += new Vector3(0, list.Count, 0);
            switch (button)
            {
                case ButtonToPress.A:
                    newButton.keyToPass = KeyCode.A;
                    newButton.GetComponent<SpriteRenderer>().sprite = listOfSprite["A"];
                    break;
                case ButtonToPress.D:
                    newButton.keyToPass = KeyCode.D;
                    newButton.GetComponent<SpriteRenderer>().sprite = listOfSprite["D"];
                    break;
                case ButtonToPress.S:
                    newButton.keyToPass = KeyCode.D;
                    newButton.GetComponent<SpriteRenderer>().sprite = listOfSprite["D"];
                    break;
                case ButtonToPress.W:
                    newButton.keyToPass = KeyCode.W;
                    newButton.GetComponent<SpriteRenderer>().sprite = listOfSprite["W"];
                    break;
            }

            list.Add(newButton);
        }

        timeToReach = pattern.timeToReachTarget;
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        bool reachPosition = false;
        int i = 0;
        while (i < list.Count)
        {
            var positionToReach = target.position - new Vector3(0, 4 * i, 0);
            transform.DOMove(positionToReach, i == 0 ? timeToReachFirstButton : timeToReach)
                .OnComplete(() =>
                {
                    reachPosition = true;
                    list[i].Enter();
                });

            yield return new WaitUntil(() => reachPosition);

            reachPosition = false;

            yield return new WaitUntil(() => !GameGoLevelManager.Instance.newButton);

            list[i].GetComponent<SpriteRenderer>().DOFade(0, timeToDisappear)
                .OnComplete(() => { reachPosition = true; });

            yield return new WaitUntil(() => reachPosition);

            reachPosition = false;

            i++;
        }

        GameGoLevelManager.Instance.gameState = GameGoLevelManager.GameState.Stop;
        GameGoLevelManager.Instance.NextPattern();
    }
}