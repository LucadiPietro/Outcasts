using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPointManager : MonoBehaviour
{
    DefaultInput input;
    Vector2 inputVector;



    private void Awake()
    {
        input = new DefaultInput();

        input.Player.Movement.performed += ctx => inputVector = ctx.ReadValue<Vector2>().normalized;

        input.Enable();
    }

    private void Update()
    {
        if (inputVector.magnitude > float.Epsilon)
        {
            float angle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0, 0, angle + 90);
        }
    }
}
