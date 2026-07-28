using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraClamp : MonoBehaviour
{

    [SerializeField] Camera cam;
    [SerializeField] Transform topright;
    [SerializeField] Transform botleft;

    private void Start()
    {
        minX = botleft.position.x;
        maxX = topright.position.x;
        minY = botleft.position.y;
        maxY = topright.position.y;
    }

    float minX;
    float maxX;
    float minY;
    float maxY;



    private void LateUpdate()
    {
        Vector3 pos = cam.transform.localPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.localPosition = pos;
    }

}
