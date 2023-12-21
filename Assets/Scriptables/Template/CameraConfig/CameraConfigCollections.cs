using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Camera Configuration/Camera Config Collections", fileName = "CameraConfigCollections")]
[System.Serializable]
public class CameraConfigCollections : ScriptableObject
{
    public List<CameraConfig> cameraConfigs;
}