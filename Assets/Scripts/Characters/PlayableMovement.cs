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
    [SerializeField] CharacterView m_View;
    bool m_IsMoving;
    bool m_IsCrouching = false;
    bool m_IsRunning = false;
    public bool IsMoving => m_IsMoving;
    public bool IsCrouching => m_IsCrouching;
    public bool IsRunning => m_IsRunning;

    [Space(10)]

    #endregion

    #region --------------------------------------------Movement Configuration------------------------------------------

    [Header("Movement")]
    public float moveSpeedHorizontal = 3f;

    public float moveSpeedVertical = 2f;
    public float runMultiplier = 1f;
    public float crouchMultiplier = .6f;
    private float speedMulti = 1;

    public MoveType moveType = MoveType.Walk;

    #endregion
    
    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Player.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Player.Run.started += e => OnRunStarted();
        defaultInput.Player.Run.canceled += e => OnRunCanceled();
        defaultInput.Player.Crouch.started += e => ToggleCrouch();
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
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeedHorizontal, moveDirection.y * moveSpeedVertical) * speedMulti;


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

        Debug.DrawRay(transform.position, idle_input_Movement, Color.yellow, .1f);

        ///TODO: Impostare che quando si esce da una cutscene si mantiene la direzione
        ///      che il character aveva nella cutscene, attualmente viene sovrascritta
        ///      da idle_input_movement perch� mantiene l'ultima direzione data in input
    }
    


    private void OnRunStarted()
    {
        if(m_IsCrouching) 
            return;

        animator.SetBool("isRun", true);
        speedMulti = runMultiplier;

        moveType = MoveType.Run;
        m_IsRunning = true;
    }

    private void OnRunCanceled()
    {
        if(m_IsCrouching)
            return;

        animator.SetBool("isRun", false);
        speedMulti = 1f;

        moveType = MoveType.Walk;
        m_IsRunning = false;
    }

    void ToggleCrouch()
    {
        ///TODO: Controlla conflitti con isMoving e isRunning

        if(m_IsRunning)
            return;

        m_IsCrouching = !m_IsCrouching;

        m_View.IsCrouching = m_IsCrouching;

        if (m_IsCrouching) 
        {
            speedMulti = crouchMultiplier;
            moveType = MoveType.Crouch;
        }
        else
        {
            speedMulti = 1;
            moveType = MoveType.Walk;
        }

        

    }

    #endregion

    private void OnEnable()
    {
        defaultInput.Enable();
    }

    private void OnDisable()
    {
        defaultInput.Disable();
    }
}
