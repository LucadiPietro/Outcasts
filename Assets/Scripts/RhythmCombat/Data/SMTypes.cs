using System;
using System.Collections.Generic;

[Serializable]
public enum LaneEventType { Tap, HoldStart, HoldEnd }

[Serializable]
public class LaneEvent
{
    public int LaneIndex;  // 0-based
    public LaneEventType Type;
}

[Serializable]
public class Frame
{
    public int MeasureIndex;
    public int RowIndexInMeasure;
    public double Time; // seconds
    public List<LaneEvent> Events = new List<LaneEvent>();
}

[Serializable]
public class ChartMeta
{
    public string GameType;
    public string Description;
    public string DifficultyText;
    public int Meter;
}

[Serializable]
public class BPMChange
{
    public double Beat;
    public double BPM;
}

[Serializable]
public class ChartData
{
    public ChartMeta Meta = new ChartMeta();
    public List<string> RawNoteData = new List<string>();
    public List<List<string>> Measures = new List<List<string>>();
    public int NumLanes;
    public double OffsetSeconds;
    public List<BPMChange> BPMs = new List<BPMChange>();
    public List<Frame> Frames = new List<Frame>();
    public string SourceFileName;
}