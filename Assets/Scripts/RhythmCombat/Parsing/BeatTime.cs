using System.Linq;

public static class BeatTime
{ 
    public static double BeatToTime(ChartData chart, double beat)
    {
        var bpms = chart.BPMs;
        if (bpms.Count == 0)
            bpms.Add(new BPMChange { Beat = 0, BPM = 120 });

        double time = chart.OffsetSeconds;

        for (int i = 0; i < bpms.Count; i++)
        {
            var seg = bpms[i];
            
            double next = (i + 1 < bpms.Count) ? bpms[i + 1].Beat : double.PositiveInfinity;

            if (beat < seg.Beat)
                break;

            if (beat >= seg.Beat && beat < next)
            {
                double diff = beat - seg.Beat;
                return time + diff * (60.0 / seg.BPM);
            }

            time += (next - seg.Beat) * (60.0 / seg.BPM);
        }

        var last = bpms.Last();

        return time + (beat - last.Beat) * (60.0 / last.BPM);
    }
}