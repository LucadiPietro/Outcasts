public static class FrameGenerator
{
    public static void GenerateFrames(ChartData chart)
    {
        chart.Frames.Clear();

        for (int m = 0; m < chart.Measures.Count; m++)
        {
            var measure = chart.Measures[m];
            int rows = measure.Count;

            for (int r = 0; r < rows; r++)
            {
                string row = measure[r];
                double beat = m * 4.0 + (4.0 * r) / rows;
                double time = BeatTime.BeatToTime(chart, beat);

                var frame = new Frame { MeasureIndex = m, RowIndexInMeasure = r, Time = time };

                for (int lane = 0; lane < row.Length; lane++)
                {
                    char c = row[lane];
                    if (c == '1')
                        frame.Events.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.Tap });
                    else if (c == '2')
                        frame.Events.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.HoldStart });
                    else if (c == '3')
                        frame.Events.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.HoldEnd });
                }

                chart.Frames.Add(frame);
            }
        }
    }
}