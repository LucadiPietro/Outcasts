using UnityEngine;

[CreateAssetMenu(fileName = "LaneMapping", menuName = "RhythmCombat/LaneMapping")]
public class LaneMappingAsset : ScriptableObject
{
    public string GameType;
    public ButtonId[] LaneToButton;
}