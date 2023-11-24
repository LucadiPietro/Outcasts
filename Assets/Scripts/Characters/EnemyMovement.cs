using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMovement : MonoBehaviour
{
    #region --------------------------------------------Configuration---------------------------------------------------

    [Header("Configuration")] public Animator animator;
    private Enemy enemy;

    [Space(10)]

    #endregion

    #region --------------------------------------------Movement Configuration------------------------------------------

    [Header("Movement")]
    private Vector2 startingPoint;

    public float moveSpeed = 1;
    public float runMultiplayer = 1.5f;
    private float runMulti = 1;

    private bool movementBool;

    [Space(10)]

    #endregion

    #region --------------------------------------------Patrol Configuration--------------------------------------------

    [Header("Patrol")]
    public List<Transform> checkPoint;

    public float restTime = 1;

    //[Space(10)]

    #endregion


    private void Start()
    {
        enemy = GetComponent<Enemy>();

        OnStateTrantion();
    }

    #region --------------------------------------------Trantition Functions--------------------------------------------

    private void OnStateTrantion()
    {
        switch (enemy.enemyState)
        {
            case Enemy.EnemyState.Patrol:
                StartCoroutine(Patrol());
                break;
            default:
                break;
        }
    }

    #endregion

    #region --------------------------------------------Movement Functions----------------------------------------------

    public void Movement(Vector2 endPoint, bool run)
    {
        startingPoint = transform.position;
        movementBool = true;
        var direction = (endPoint - startingPoint).normalized;

        animator.SetBool("isRun", run);
        animator.SetFloat("x_Input", direction.x);
        animator.SetFloat("y_Input", direction.y);


        animator.SetFloat("idle_x_input", direction.x);
        animator.SetFloat("idle_y_input", direction.y);


        StartCoroutine(MoveRoutine(endPoint, run));
    }

    IEnumerator MoveRoutine(Vector2 endPoint, bool run)
    {
        runMulti = run ? runMultiplayer : 1;

        animator.SetBool("Movement", true);

        while (Vector2.Distance(transform.position, endPoint) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPoint, 0.016f * moveSpeed * runMulti);
            yield return null;
        }

        movementBool = false;
        animator.SetBool("Movement", false);

        animator.SetBool("isRun", false);
    }

    #endregion

    #region --------------------------------------------Movement Functions----------------------------------------------
    
    public IEnumerator Patrol()
    {
        int index = 0;
        while (enemy.enemyState == Enemy.EnemyState.Patrol)
        {
            
            Movement(checkPoint[index].position, false);

            yield return new WaitUntil(() => !movementBool);
            float randomRest = Random.Range(0, 1);
            yield return new WaitForSeconds(restTime + randomRest);
            index++;
            if (index == checkPoint.Count) index = 0;
        }
    }
    
    #endregion
}