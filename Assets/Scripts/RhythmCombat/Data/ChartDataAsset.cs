using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChartData", menuName = "RhythmCombat/Chart Data")]
public class ChartDataAsset : ScriptableObject
{
    [Header("Identity")]
    public string title;
    public string sourceFileName;

    [Header("Chart Meta")]
    public string gameType;
    public string difficulty;
    public int meter;
    public int numLanes;

    [Header("Timing")]
    public float offsetSeconds;
    public List<BPMChange> bpms = new();

    [Header("Result")]
    public List<Frame> frames = new();
}
