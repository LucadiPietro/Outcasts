using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Vector2 startingPoint;

    public Animator animator;

    public float moveSpeed = 1;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Movement(Vector2 endPoint, bool run)
    {
        startingPoint = transform.position;

        var direction = endPoint - startingPoint;

        animator.SetBool("Movement", true);
        
        animator.SetBool("isRun", run);
        animator.SetFloat("x_Input", direction.x);
        animator.SetFloat("y_Input", direction.y);


        animator.SetFloat("idle_x_input", direction.x);
        animator.SetFloat("idle_y_input", direction.y);


        StartCoroutine(MoveRoutine(endPoint));
    }

    IEnumerator MoveRoutine(Vector2 endPoint)
    {
        while (Vector2.Distance(transform.position, endPoint) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPoint, 0.016f * moveSpeed);
            yield return null;
        }
        
        animator.SetBool("Movement", false);
        
        animator.SetBool("isRun", false);
    }
}