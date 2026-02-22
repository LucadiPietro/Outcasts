using System.Collections.Generic;

public class RowGenerator
{
    public static List<ChartRow> GenerateRows(ChartData chart)
    {
        var rowsOut = new List<ChartRow>();

        for (int m = 0; m < chart.Measures.Count; m++)
        {
            var measure = chart.Measures[m];
            int rowsInMeasure = measure.Count;
            if (rowsInMeasure == 0) continue;

            for (int r = 0; r < rowsInMeasure; r++)
            {
                string row = measure[r].Trim();

                double beat = m * 4.0 + (4.0 * r) / rowsInMeasure;
                double time = BeatTime.BeatToTime(chart, beat);

                var lanes = new byte[chart.NumLanes]; // ✅ sempre 6 per pump-halfdouble
                var eventsList = new List<LaneEvent>();

                int fillCount = row.Length < chart.NumLanes ? row.Length : chart.NumLanes;

                for (int lane = 0; lane < fillCount; lane++)
                {
                    char ch = row[lane];
                    byte v = (ch >= '0' && ch <= '9') ? (byte)(ch - '0') : (byte)0;
                    lanes[lane] = v;

                    if (v == 1) eventsList.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.Tap });
                    else if (v == 2) eventsList.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.HoldStart });
                    else if (v == 3) eventsList.Add(new LaneEvent { LaneIndex = lane, Type = LaneEventType.HoldEnd });
                }

                rowsOut.Add(new ChartRow
                {
                    measureIndex = m,
                    rowIndexInMeasure = r,
                    time = time,
                    lanes = lanes,
                    events = eventsList
                });
            }
        }

        return rowsOut;
    }

}
