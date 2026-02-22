#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SMChartImporterWindow : EditorWindow
{
    private DefaultAsset smFile;
    private int chartIndex = 0;

    private string parsedTitle = "";
    private List<ChartData> parsedCharts;

    [MenuItem("RhythmCombat/SM Chart Importer")]
    public static void Open()
    {
        GetWindow<SMChartImporterWindow>("SM Chart Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("StepMania → ChartDataAsset", EditorStyles.boldLabel);

        smFile = (DefaultAsset)EditorGUILayout.ObjectField("SM File (.sm)", smFile, typeof(DefaultAsset), false);

        EditorGUILayout.Space(8);

        // 1) Parse
        GUI.enabled = smFile != null;
        if (GUILayout.Button("1) Parse .sm"))
        {
            ParseSelectedFile();
        }
        GUI.enabled = true;

        // Show parse result
        if (parsedCharts != null && parsedCharts.Count > 0)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Title:", parsedTitle);
            EditorGUILayout.LabelField("Charts found:", parsedCharts.Count.ToString());

            chartIndex = Mathf.Clamp(chartIndex, 0, parsedCharts.Count - 1);
            chartIndex = EditorGUILayout.IntSlider("Chart Index", chartIndex, 0, parsedCharts.Count - 1);

            var c = parsedCharts[chartIndex];
            EditorGUILayout.LabelField("GameType:", c.Meta.GameType);
            EditorGUILayout.LabelField("Difficulty:", c.Meta.DifficultyText);
            EditorGUILayout.LabelField("Meter:", c.Meta.Meter.ToString());
            EditorGUILayout.LabelField("Guessed Lanes:", c.NumLanes.ToString());

            EditorGUILayout.Space(8);

            // 2) Build Frames + Save asset
            if (GUILayout.Button("2) Generate Frames + Save ScriptableObject"))
            {
                GenerateAndSaveAsset(c);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select a .sm TextAsset, click 'Parse .sm'.", MessageType.Info);
        }
    }

    private void ParseSelectedFile()
    {
        parsedCharts = null;
        parsedTitle = "";
        chartIndex = 0;

        string path = AssetDatabase.GetAssetPath(smFile);
        string smText = File.ReadAllText(path);

        var (title, music, offset, bpms, charts) = SMParser.ParseSM(smText, smFile.name);
        parsedTitle = title;
        parsedCharts = charts;

        Debug.Log($"Parsed '{smFile.name}': title='{title}', charts={charts.Count}");
    }

    private void GenerateAndSaveAsset(ChartData chart)
    {
        // 1) Build pipeline base
        MeasureUtils.SplitMeasuresAndValidate(chart);

        // 2) Crea rows (grid 0/1/2/3 raggruppata a 6)
        var rows = RowGenerator.GenerateRows(chart);

        // 3) (Opzionale) Crea anche frames evento-based
        FrameGenerator.GenerateFrames(chart);

        // 4) Create ScriptableObject
        var asset = ScriptableObject.CreateInstance<ChartDataAsset>();

        asset.title = parsedTitle;
        asset.sourceFileName = smFile.name;

        asset.gameType = chart.Meta.GameType;
        asset.difficulty = chart.Meta.DifficultyText;
        asset.meter = chart.Meta.Meter;
        asset.numLanes = chart.NumLanes;

        asset.offsetSeconds = (float)chart.OffsetSeconds;
        asset.bpms = new List<BPMChange>(chart.BPMs);

        asset.rows = rows;
        asset.frames = new List<Frame>(chart.Frames); // (opzionale)

        // 5) Ensure folder exists
        string folder = "Assets/Charts";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        // 6) Safe file name
        string safeDiff = MakeSafeFileName(asset.difficulty);
        string safeGT = MakeSafeFileName(asset.gameType);
        string path = $"{folder}/{smFile.name}_{safeGT}_{safeDiff}.asset";

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;

        Debug.Log($"Saved ChartDataAsset → {path} | rows={asset.rows.Count} frames={asset.frames.Count}");
    }

    private static string MakeSafeFileName(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c.ToString(), "_");
        return s.Replace(" ", "_");
    }
}
#endif
