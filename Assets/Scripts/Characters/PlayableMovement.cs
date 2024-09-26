using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using Pathfinding;

public class PlayableMovement : MonoBehaviour
{
    #region --------------------------------------------Configuration---------------------------------------------------

    [Header("Configuration")]
    DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 idle_input_Movement;
    public Animator animator;
    private bool isColliding = false;
    private Vector2 collisionNormal;
    public AIPath aiPath;

    Rigidbody2D rb;
    bool m_IsMoving;
    public bool IsMoving => m_IsMoving;

    [Space(10)]

    #endregion

    #region --------------------------------------------Movement Configuration------------------------------------------

    [Header("Movement")]
    public float moveSpeedHorizontal = 3f;

    public float moveSpeedVertical = 2f;
    public float runMultiplayer = 1f;
    private float runMulti = 1;

    public MoveType moveType = MoveType.Walk;

    #endregion

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Player.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Player.Run.started += e => OnRunStarted();
        defaultInput.Player.Run.canceled += e => OnRunCanceled();

        defaultInput.Enable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        MovePlayer();
    }

    #region --------------------------------------------Input Functions-------------------------------------------------

    private void MovePlayer()
    {
        Vector2 moveInput = new Vector2(input_Movement.x, input_Movement.y);
        Vector2 moveDirection = moveInput.normalized;

        if (isColliding)
        {
            float dotProduct = Vector2.Dot(moveDirection, collisionNormal);

            if (dotProduct > 0)
            {
                return;
            }
        }

        aiPath.enabled = false;

        float horizontalSpeed = moveSpeedHorizontal * Time.deltaTime;
        float verticalSpeed = moveSpeedVertical * Time.deltaTime;

        Vector3 move = new Vector3(moveDirection.x * horizontalSpeed, moveDirection.y * verticalSpeed, 0);
        //transform.Translate(move * runMulti);
        rb.velocity = new Vector2(moveDirection.x * moveSpeedHorizontal, moveDirection.y * moveSpeedVertical) * runMulti;


        m_IsMoving = move.sqrMagnitude > float.Epsilon;

        if (moveInput.magnitude > 0)
        {
            animator.SetBool("Movement", true);
            animator.SetFloat("x_Input", input_Movement.x);
            animator.SetFloat("y_Input", input_Movement.y);
            idle_input_Movement.x = input_Movement.x;
            idle_input_Movement.y = input_Movement.y;
        }
        else
        {
            animator.SetBool("Movement", false);
            animator.SetFloat("idle_x_input", idle_input_Movement.x);
            animator.SetFloat("idle_y_input", idle_input_Movement.y);
        }
    }

    private void OnRunStarted()
    {
        animator.SetBool("isRun", true);
        runMulti = runMultiplayer;

        moveType = MoveType.Run;
    }

    private void OnRunCanceled()
    {
        animator.SetBool("isRun", false);
        runMulti = 1f;

        moveType = MoveType.Walk;
    }

    #endregion
}
