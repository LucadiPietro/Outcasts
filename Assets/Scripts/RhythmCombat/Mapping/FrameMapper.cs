using System.Collections.Generic;

public static class FrameMapper
{
    public static List<(double time, ButtonId button, LaneEventType type)>
        Map(ChartData chart, LaneMappingAsset mapping)
    {
        var list = new List<(double, ButtonId, LaneEventType)>();

        foreach (var frame in chart.Frames)
        {
            foreach (var ev in frame.Events)
            {
                var button = mapping.LaneToButton[ev.LaneIndex];
                list.Add((frame.Time, button, ev.Type));
            }
        }

        return list;
    }
}
