using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public int targetPoint;
    public float speed;

    public Transform player;
    public Transform Sight;

    public Animator animator;
    public bool isWaiting;

    void Start()
    {
        targetPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaiting == true)
        {

        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        isWaiting = false;
        if (transform.position == patrolPoints[targetPoint].position)
        {
            increaseTargetInt();
        }

        transform.position = Vector2.MoveTowards(transform.position, patrolPoints[targetPoint].position, speed * Time.deltaTime);

        //animations sprite
        animator.SetFloat("Horizontal", transform.position.x);
        animator.SetFloat("Vertical", transform.position.y);
        animator.SetFloat("Speed", speed);
    }

    void increaseTargetInt()
    {
            if (isWaiting == false)
            {
                targetPoint++;
            }

        if (targetPoint >= patrolPoints.Length)
        {
            targetPoint = 0;
        }

        switch (targetPoint)
        {
            case 6:
                isWaiting = true;
                Sight.Rotate(new Vector3(0, 0, 90));
                Invoke("Move", 2f);

                break;
            case 5:
                Sight.Rotate(new Vector3(0, 0, 90));
                break;
            case 4:
                isWaiting = true;
                Sight.Rotate(new Vector3(0, 0, -90));
                StartCoroutine(RotateAfterDelay(3f, 90));
                StartCoroutine(RotateAfterDelay(4f, 90));
                Invoke("Move", 7f);
                break;
            case 3:
                isWaiting = true;
                Sight.Rotate(new Vector3(0, 0, -180));
                //First rotation left
                StartCoroutine(RotateAfterDelay(2f, -90));
                StartCoroutine(RotateAfterDelay(3f, 180));
                Invoke("Move", 6f);
                break;
            case 2:
                Sight.Rotate(new Vector3(0, 0, 90));
                break;
            case 1:
                Sight.Rotate(new Vector3(0, 0, 0));
                StartCoroutine(RotateAfterDelay(0f, 90));
                break;
        }
    }

    IEnumerator RotateAfterDelay(float delay, int angle)
    {
        yield return new WaitForSeconds(delay);
        Sight.Rotate(0, 0, angle);
    }
}