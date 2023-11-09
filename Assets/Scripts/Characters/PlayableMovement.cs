using UnityEngine;

public class PlayableMovement : MonoBehaviour
{
    public DefaultInput defaultInput;
    public Vector2 input_Movement;
    public Vector2 idle_input_Movement;
    public float moveSpeedHorizontal = 3f;
    public float moveSpeedVertical = 2f;
    public float runMultiplayer = 1f;
    private float runMulti = 1;
    public Animator animator;

    [SerializeField] private float moveDuration = 1f;
    private float moveTimer = 0f;

    private bool isColliding = false;
    private Vector2 collisionNormal;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Player.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Player.Run.started += e => OnRunStarted();
        defaultInput.Player.Run.canceled += e => OnRunCanceled();

        defaultInput.Enable();
    }

    private void Update()
    {
        MovePlayer();
    }

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
        runMulti = runMultiplayer;
    }

    private void OnRunCanceled()
    {
        runMulti = 1f;
    }
}
