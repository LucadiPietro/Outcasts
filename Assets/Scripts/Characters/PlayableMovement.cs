using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableMovement : MonoBehaviour
{
    #region --------------------------------------------Configuration---------------------------------------------------

    [Header("Configuration")] public DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 idle_input_Movement;
    private Player player;
    public Animator animator;
    private bool isColliding = false;
    private Vector2 collisionNormal;

    [Space(10)]

    #endregion

    #region --------------------------------------------Movement Configuration------------------------------------------

    [Header("Movement")]
    public float moveSpeedHorizontal = 3f;

    public float moveSpeedVertical = 2f;
    public float runMultiplayer = 1f;
    private float runMulti = 1;
    [SerializeField] private float moveDuration = 1f;
    private float moveTimer = 0f;

    [Space(10)]

    #endregion
    
    #region --------------------------------------------CutScene Configuration------------------------------------------

    [Header("CutScene Movement")]
    
    public List<Transform> checkPoint;

    public float timeBeforeStartMovement = 1;
    public float restTime = 1;
    private Vector2 startingPoint;
    private bool movementBool;
    public float moveSpeed = 1;

    //[Space(10)]

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
        player = GetComponent<Player>();

        StartCoroutine(FirstMovemnt());

    }

    private void Update()
    {
        if(player.playerState == Player.PlayerState.Playable) MovePlayer();
    }

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

        float horizontalSpeed = moveSpeedHorizontal * Time.deltaTime;
        float verticalSpeed = moveSpeedVertical * Time.deltaTime;

        Vector3 move = new Vector3(moveDirection.x * horizontalSpeed, moveDirection.y * verticalSpeed, 0);
        transform.Translate(move * runMulti);

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
            idle_input_Movement.y = input_Movement.y;
        }
    }

    private void OnRunStarted()
    {
        animator.SetBool("isRun", true);
        runMulti = runMultiplayer;
    }

    private void OnRunCanceled()
    {
        animator.SetBool("isRun", false);
        runMulti = 1f;
    }
    
    #endregion
    
    #region --------------------------------------------CutScene Functions----------------------------------------------

    public IEnumerator FirstMovemnt()
    {
        yield return new WaitForSeconds(timeBeforeStartMovement);
        
        foreach (var t in checkPoint)
        {
            Movement(t.position, false);

            yield return new WaitUntil(() => !movementBool);
        }

        yield return new WaitForSeconds(restTime);
    }
    
    #endregion
}