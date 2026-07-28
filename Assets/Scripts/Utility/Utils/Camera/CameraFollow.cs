using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    public bool is2d = false;

    float z;
    private void Start()
    {
        z = transform.position.z;
    }

    void FixedUpdate()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (is2d)
        {
            smoothedPosition.z = z;
        }
        else
        {
            transform.LookAt(target);
        }
        transform.position = smoothedPosition;
        if (clamped)
        {
            Clamp();
        }
    }


    [SerializeField] bool clamped;
    [SerializeField] Transform clampSouthWest;
    [SerializeField] Transform clampNorthEast;

    void Clamp()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, clampSouthWest.position.x, clampNorthEast.position.x);
        pos.y = Mathf.Clamp(pos.y, clampSouthWest.position.y, clampNorthEast.position.y);
        transform.position = pos;
    }

}
