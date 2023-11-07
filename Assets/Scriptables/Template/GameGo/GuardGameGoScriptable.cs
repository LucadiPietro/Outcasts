using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ViewCone
{
    public Guard.Position next;
    [Range(0.7f,10)]
    public float speed;
    [Range(0.7f,10)]
    public float delay;
}

[CreateAssetMenu(menuName = "GameGo/Guard Pattern", fileName = "Guard Pattern")]
[System.Serializable]
public class GuardGameGoScriptable : ScriptableObject
{
    [Header("List Of Pattern")]
    [Tooltip(
        "Enter the next position the time it takes to change position and the time it remains in that position.")]
    public List<ViewCone> pattern;

    [Header("LevelTimer")] public int timer = 10;
}