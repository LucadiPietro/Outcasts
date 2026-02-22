using System;
using System.Collections.Generic;

[Serializable]
public class ChartRow
{
    public int measureIndex;
    public int rowIndexInMeasure;
    public double time;

    // 6 valori per pump-halfdouble (o numLines in generale)
    // 0,1,2,3 come StepMania
    public byte[] lanes;

    // opzionale: eventi già calcolati della riga
    public List<LaneEvent> events = new();
}
