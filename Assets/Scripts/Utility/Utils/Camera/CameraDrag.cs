using System;
using UnityEngine;

class CameraDrag : MonoBehaviour
{
    private Vector3 dragOrigin;
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Camera.main.GetMouseWorldPosition();
            return;
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 move = Camera.main.GetMouseWorldPosition() - Camera.main.transform.position;
            Camera.main.transform.position = dragOrigin - move;
        }
    }

}
