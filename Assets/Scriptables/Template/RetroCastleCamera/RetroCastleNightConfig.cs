using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RetroCastleNight Camera Configuration/Camera Configuration", fileName = "Camera Configuration")]
[System.Serializable]
public class RetroCastleNightConfig : ScriptableObject
{
    public Vector3 position;
    public float FOV;
    public float restTime;
    public float duration = 6;
}
