using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Guard : MonoBehaviour
{
    public enum Position
    {
        Left,
        Top
    }

    public enum State
    {
        Patrol,
        Rest
    }

    public Position position;

    public State state;

    public GameObject cone;

    [HideInInspector] public GuardGameGoScriptable pattern;
    private bool endRotation;


    // Start is called before the first frame update
    public void StartGuard(GuardGameGoScriptable guardPattern)
    {
        StopAllCoroutines();
        pattern = guardPattern;
        state = State.Patrol;
        StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        var viewCones = pattern.pattern.Select(ele => ele).ToList();
        
        int i = 0;
        while (state == State.Patrol)
        {
            Vector3 rotation = Vector3.zero;
            endRotation = false;
            if (i >= viewCones.Count()) i = 0;

            position = viewCones[i].next;
            yield return null;
            rotation = position switch
            {
                Position.Left => new Vector3(0, 0, 90),
                Position.Top => new Vector3(0, 0, 0),
            };

            cone.transform.DOLocalRotate(rotation, viewCones[i].speed)
                .OnComplete(() =>
                {
                    endRotation = true;
                    
                });

            yield return new WaitUntil(() => endRotation);
            
            print(viewCones[i].delay);

            yield return new WaitForSeconds(viewCones[i].delay);

            i++;
        }
    }

    private void Update()
    {
        GameGoLevelManager.Instance.inCheck = (endRotation && position == Position.Left);
    }

    public void StopGuard()
    {
        state = State.Rest;
    }
}