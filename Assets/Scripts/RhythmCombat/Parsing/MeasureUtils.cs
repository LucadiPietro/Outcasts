public static class MeasureUtils
{
    public static void SplitMeasuresAndValidate(ChartData chart)
    {
        chart.Measures.Clear();

        var current = new System.Collections.Generic.List<string>();
        foreach (var raw in chart.RawNoteData)
        {
            if (raw == ",")
            {
                if (current.Count > 0)
                {
                    chart.Measures.Add(new System.Collections.Generic.List<string>(current));
                    current.Clear();
                }
            }
            else current.Add(raw);
        }
        if (current.Count > 0)
            chart.Measures.Add(current);
    }
}