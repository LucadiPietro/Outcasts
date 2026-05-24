using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BattleRhythmChartRunner : MonoBehaviour
{
    const string DefaultChartPath = "Assets/RhythmCombat/Generated/Charts/Tutorial Battaglia   Epic Metal Feels_pump-halfdouble_Beginner.asset";

    [SerializeField] ChartDataAsset chart;
    [SerializeField] bool playOnStart = true;
    [SerializeField] int maxNotesToSchedule = 128;
    [SerializeField] float startDelaySeconds = 1.5f;
    [SerializeField] float approachDurationSeconds = 2f;
    [SerializeField] Color attackColor = new Color(0.95f, 0.25f, 0.2f, 1f);
    [SerializeField] Color defenseColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] Color holdColor = new Color(0.9f, 0.75f, 0.2f, 1f);

    readonly List<ScheduledButton> scheduledButtons = new List<ScheduledButton>();

    BMBattleManager battleManager;
    double elapsedSeconds;
    int nextButtonIndex;
    bool isPlaying;

    public void Configure(
        ChartDataAsset chartData,
        BMBattleManager manager,
        float startDelay,
        float approachDuration,
        int maxNotes,
        bool autoPlay)
    {
        chart = chartData;
        battleManager = manager;
        startDelaySeconds = startDelay;
        approachDurationSeconds = approachDuration;
        maxNotesToSchedule = maxNotes;
        playOnStart = autoPlay;
    }

    IEnumerator Start()
    {
        TryLoadDefaultChart();

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BMBattleManager>();
        }

        BuildSchedule();

        if (!playOnStart)
        {
            yield break;
        }

        yield return new WaitForSeconds(startDelaySeconds);
        Play();
    }

    void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetChartTimeSeconds(elapsedSeconds);
        }

        SpawnDueButtons();
    }

    public void Play()
    {
        if (chart == null || battleManager == null)
        {
            Debug.LogWarning("BattleRhythmChartRunner: impossibile avviare, chart o BMBattleManager mancanti.");
            return;
        }

        elapsedSeconds = 0d;
        nextButtonIndex = 0;
        isPlaying = true;
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetChartTimeSeconds(elapsedSeconds);
        }

        Debug.Log("BattleRhythmChartRunner: avvio chart '" + chart.name + "' con " + scheduledButtons.Count + " note schedulate.");
    }

    void BuildSchedule()
    {
        scheduledButtons.Clear();
        nextButtonIndex = 0;

        if (chart == null)
        {
            Debug.LogWarning("BattleRhythmChartRunner: nessun ChartDataAsset assegnato.");
            return;
        }

        if (battleManager == null)
        {
            Debug.LogWarning("BattleRhythmChartRunner: nessun BMBattleManager disponibile.");
            return;
        }

        int scheduledCount = 0;
        for (int rowIndex = 0; rowIndex < chart.rows.Count && scheduledCount < maxNotesToSchedule; rowIndex++)
        {
            ChartRow row = chart.rows[rowIndex];
            if (row == null || row.lanes == null)
            {
                continue;
            }

            for (int laneIndex = 0; laneIndex < row.lanes.Length && laneIndex < 6 && scheduledCount < maxNotesToSchedule; laneIndex++)
            {
                byte laneValue = row.lanes[laneIndex];
                if (laneValue == 0)
                {
                    continue;
                }

                scheduledButtons.Add(CreateScheduledButton(rowIndex, laneIndex, laneValue, row.time));
                scheduledCount++;
            }
        }

        scheduledButtons.Sort((a, b) => a.SpawnTimeSeconds.CompareTo(b.SpawnTimeSeconds));
        Debug.Log("BattleRhythmChartRunner: lette " + scheduledButtons.Count + " note da ChartDataAsset '" + chart.name + "'.");
    }

    ScheduledButton CreateScheduledButton(int rowIndex, int laneIndex, byte laneValue, double hitTimeSeconds)
    {
        double spawnTime = hitTimeSeconds - approachDurationSeconds - 0.1d;
        if (spawnTime < 0d)
        {
            spawnTime = 0d;
        }

        return new ScheduledButton(
            rowIndex,
            laneIndex,
            laneValue,
            spawnTime,
            hitTimeSeconds,
            ToCell(laneIndex),
            ToKey(laneIndex),
            ToColor(laneIndex, laneValue));
    }

    void SpawnDueButtons()
    {
        while (nextButtonIndex < scheduledButtons.Count && scheduledButtons[nextButtonIndex].SpawnTimeSeconds <= elapsedSeconds)
        {
            ScheduledButton scheduled = scheduledButtons[nextButtonIndex];
            if (battleManager == null)
            {
                return;
            }

            battleManager.CreateButton(
                scheduled.Cell,
                scheduled.Key,
                null,
                scheduled.Color,
                scheduled.LaneIndex,
                scheduled.HitTimeSeconds);
            Debug.Log(
                "BattleRhythmChartRunner: spawn row " + scheduled.RowIndex +
                " lane " + scheduled.LaneIndex +
                " value " + scheduled.LaneValue +
                " -> " + scheduled.Cell);
            nextButtonIndex++;
        }
    }

    BMButtonPrefab.Cell ToCell(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return BMButtonPrefab.Cell.Cell1;
            case 1:
                return BMButtonPrefab.Cell.Cell2;
            case 2:
                return BMButtonPrefab.Cell.Cell3;
            case 3:
                return BMButtonPrefab.Cell.Cell4;
            case 4:
                return BMButtonPrefab.Cell.Cell5;
            case 5:
                return BMButtonPrefab.Cell.Cell6;
            default:
                return BMButtonPrefab.Cell.Cell1;
        }
    }

    Keys ToKey(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return Keys.Y;
            case 1:
                return Keys.X;
            case 2:
                return Keys.B;
            case 3:
                return Keys.A;
            case 4:
                return Keys.LT;
            case 5:
                return Keys.RT;
            default:
                return Keys.NONE;
        }
    }

    Color ToColor(int laneIndex, byte laneValue)
    {
        if (laneValue == 2 || laneValue == 3)
        {
            return holdColor;
        }

        return laneIndex < 3 ? attackColor : defenseColor;
    }

    void TryLoadDefaultChart()
    {
        if (chart != null)
        {
            return;
        }

#if UNITY_EDITOR
        chart = AssetDatabase.LoadAssetAtPath<ChartDataAsset>(DefaultChartPath);
#endif
    }

    struct ScheduledButton
    {
        public ScheduledButton(
            int rowIndex,
            int laneIndex,
            byte laneValue,
            double spawnTimeSeconds,
            double hitTimeSeconds,
            BMButtonPrefab.Cell cell,
            Keys key,
            Color color)
        {
            RowIndex = rowIndex;
            LaneIndex = laneIndex;
            LaneValue = laneValue;
            SpawnTimeSeconds = spawnTimeSeconds;
            HitTimeSeconds = hitTimeSeconds;
            Cell = cell;
            Key = key;
            Color = color;
        }

        public int RowIndex;
        public int LaneIndex;
        public byte LaneValue;
        public double SpawnTimeSeconds;
        public double HitTimeSeconds;
        public BMButtonPrefab.Cell Cell;
        public Keys Key;
        public Color Color;
    }
}
