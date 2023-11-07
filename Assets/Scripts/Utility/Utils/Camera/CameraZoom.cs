using System;
using UnityEngine;

class CameraZoom : MonoBehaviour
{

    [SerializeField] float minSize = 5f;
    [SerializeField] float maxSize = 10f;
    [SerializeField] float sensitivity = 10f;

    private void Update()
    {
        float fov = Camera.main.orthographicSize;
        fov -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
        fov = Mathf.Clamp(fov, minSize, maxSize);
        Camera.main.orthographicSize = fov;
    }

}
