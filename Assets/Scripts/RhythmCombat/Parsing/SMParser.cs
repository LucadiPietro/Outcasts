using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class SMParser
{
    public static (
        string title, 
        string musicFile,
        double offset,
        List<BPMChange> bpms,
        List<ChartData> charts) 
        
        ParseSM(string smText, string sourceFileName = "")
    {
        var lines = 
            smText.Replace("\r\n", "\n")
                  .Replace("\r", "\n")
                  .Split('\n')
                  .Select(l => l.Trim())
                  .ToList();

        string title = "";
        string music = "";
        double offset = 0;
        var bpms = new List<BPMChange>();
        var charts = new List<ChartData>();

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.StartsWith("#TITLE:", StringComparison.OrdinalIgnoreCase))
                title = ExtractValue(line);

            else if (line.StartsWith("#MUSIC:", StringComparison.OrdinalIgnoreCase))
                music = ExtractValue(line);

            else if (line.StartsWith("#OFFSET:", StringComparison.OrdinalIgnoreCase))
                double.TryParse(ExtractValue(line), NumberStyles.Float, CultureInfo.InvariantCulture, out offset);

            else if (line.StartsWith("#BPMS:", StringComparison.OrdinalIgnoreCase))
                bpms = ParseBPMs(ExtractValue(line));

            else if (line.StartsWith("#NOTES", StringComparison.OrdinalIgnoreCase))
            {
                var chart = ParseNotesBlock(lines, ref i, offset, bpms, sourceFileName);
                charts.Add(chart);
            }
        }

        return (title, music, offset, bpms, charts);
    }

    #region Parse methods
    private static ChartData ParseNotesBlock(List<string> lines, ref int i, double offset, List<BPMChange> bpms, string source)
    {
        int cursor = i + 1;

        string gameType = lines[cursor++].TrimEnd(':');
        string desc = lines[cursor++].TrimEnd(':');
        string diff = lines[cursor++].TrimEnd(':');
        string meterStr = lines[cursor++].TrimEnd(':');
        cursor++; // skip radar

        int.TryParse(meterStr, out int meter);

        var chart = new ChartData();
        chart.Meta.GameType = gameType;
        chart.Meta.Description = desc;
        chart.Meta.DifficultyText = diff;
        chart.Meta.Meter = meter;
        chart.OffsetSeconds = offset;
        chart.BPMs = bpms.Select(b => new BPMChange { Beat = b.Beat, BPM = b.BPM }).ToList();
        chart.SourceFileName = source;
        chart.NumLanes = GuessNumLanesFromGameType(gameType);

        // Read note data until ';'
        var body = new List<string>();
        while (cursor < lines.Count && !lines[cursor].StartsWith(";"))
        {
            if (!string.IsNullOrEmpty(lines[cursor]))
                body.Add(lines[cursor]);
            cursor++;
        }

        chart.RawNoteData = body;

        i = cursor; // update index
        return chart;
    }

    private static string ExtractValue(string line)
    {
        int colon = line.IndexOf(':');
        int semi = line.LastIndexOf(';');
        if (colon >= 0 && semi > colon)
            return line.Substring(colon + 1, semi - colon - 1).Trim();
        return "";
    }

    private static List<BPMChange> ParseBPMs(string raw)
    {
        var res = new List<BPMChange>();
        if (string.IsNullOrWhiteSpace(raw)) return res;

        foreach (var part in raw.Split(','))
        {
            var kv = part.Split('=');
            if (kv.Length != 2) continue;

            if (double.TryParse(kv[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double beat) &&
                double.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double bpm))
            {
                res.Add(new BPMChange { Beat = beat, BPM = bpm });
            }
        }
        return res.OrderBy(b => b.Beat).ToList();
    }

    private static int GuessNumLanesFromGameType(string g)
    {
        string s = g.ToLower();
        if (s.Contains("pump-halfdouble")) return 6;
        if (s.Contains("pump-single")) return 5;
        if (s.Contains("dance-single")) return 4;
        return 4;
    }
    #endregion

}