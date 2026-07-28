using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Camera Configuration/Camera Config", fileName = "CameraConfig")]
[System.Serializable]
public class CameraConfig : ScriptableObject
{
    public Vector3 position;
    public float FOV;
    public float restTime;
    public float duration = 6;
}
