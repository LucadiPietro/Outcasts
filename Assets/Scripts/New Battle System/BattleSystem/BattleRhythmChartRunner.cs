using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BattleRhythmChartRunner : MonoBehaviour
{
    const string DefaultChartPath = "Assets/RhythmCombat/Generated/Charts/Tutorial Battaglia   Epic Metal Feels_pump-halfdouble_Beginner.asset";
    const string DefaultMusicPath = "Assets/Scriptables/Resources/Battle/Charts/Tutorial/Tutorial Battaglia   Epic Metal Feels.mp3";
    const string DefaultMusicResourcePath = "Battle/Charts/Tutorial/Tutorial Battaglia   Epic Metal Feels";
    const string DefaultMusicProjectRelativePath = "Scriptables/Resources/Battle/Charts/Tutorial/Tutorial Battaglia   Epic Metal Feels.mp3";
    const string DefaultMusicAssetGuid = "9770822bd7e92344db8574c32fe148e2";

    [SerializeField] ChartDataAsset chart;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip musicClip;
    [SerializeField] bool useAudioSourceClock = true;
    [SerializeField] bool playMusicOnStart = true;
    [SerializeField] bool scheduleMusicWithDspClock = true;
    [SerializeField] bool playOnStart = true;
    [SerializeField] int maxNotesToSchedule = 128;
    [SerializeField] float startDelaySeconds = 1.5f;
    [SerializeField] float audioScheduleLeadSeconds = 0.1f;
    [SerializeField] float manualChartSyncOffsetSeconds = 0f;
    [SerializeField] float approachDurationSeconds = 2f;
    [SerializeField] Color attackColor = new Color(0.95f, 0.25f, 0.2f, 1f);
    [SerializeField] Color defenseColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] Color holdColor = new Color(0.9f, 0.75f, 0.2f, 1f);

    readonly List<ScheduledButton> scheduledButtons = new List<ScheduledButton>();

    BMBattleManager battleManager;
    double elapsedSeconds;
    double fallbackClockStartTime;
    double scheduledDspStartTime;
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
        musicClip = ResolveMusicClip(musicClip);
        if (musicClip == null)
        {
            yield return LoadDefaultMusicClipFromFile();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BMBattleManager>();
        }

        if (musicSource == null)
        {
            musicSource = FindSceneAudioSource();
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

        elapsedSeconds = GetChartClockSeconds();

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
        fallbackClockStartTime = Time.timeAsDouble;
        scheduledDspStartTime = 0d;
        nextButtonIndex = 0;

        PrepareAndPlayMusic();
        isPlaying = true;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetChartTimeSeconds(elapsedSeconds);
        }

        Debug.Log("BattleRhythmChartRunner: avvio chart '" + chart.name + "' con " + scheduledButtons.Count + " note schedulate.");
    }

    void PrepareAndPlayMusic()
    {
        if (musicSource == null)
        {
            Debug.LogWarning("BattleRhythmChartRunner: nessun AudioSource in scena, uso clock fallback senza musica.");
            fallbackClockStartTime = Time.timeAsDouble;
            return;
        }

        musicClip = ResolveMusicClip(musicClip != null ? musicClip : musicSource.clip);
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
        }

        musicSource.playOnAwake = false;
        musicSource.time = 0f;

        if (!playMusicOnStart || musicSource.clip == null)
        {
            Debug.LogWarning(
                "BattleRhythmChartRunner: musica non avviata. playMusicOnStart=" + playMusicOnStart +
                ", hasClip=" + (musicSource.clip != null) +
                ", defaultResourcePath='" + DefaultMusicResourcePath + "'" +
                ". Uso clock fallback.");
            fallbackClockStartTime = Time.timeAsDouble;
            return;
        }

        if (musicSource.clip.loadState != AudioDataLoadState.Loaded)
        {
            musicSource.clip.LoadAudioData();
        }

        if (scheduleMusicWithDspClock)
        {
            scheduledDspStartTime = AudioSettings.dspTime + Mathf.Max(0f, audioScheduleLeadSeconds);
            musicSource.PlayScheduled(scheduledDspStartTime);
            Debug.Log(
                "BattleRhythmChartRunner: musica schedulata su AudioSource esistente, dspStart=" +
                scheduledDspStartTime.ToString("0.000") +
                ", clip='" + musicSource.clip.name + "'.");
        }
        else
        {
            musicSource.Play();
            scheduledDspStartTime = AudioSettings.dspTime;
            Debug.Log("BattleRhythmChartRunner: musica avviata su AudioSource esistente, clip='" + musicSource.clip.name + "'.");
        }
    }

    double GetChartClockSeconds()
    {
        double chartTime;

        if (useAudioSourceClock && musicSource != null && playMusicOnStart && musicSource.clip != null)
        {
            if (scheduleMusicWithDspClock && scheduledDspStartTime > 0d)
            {
                chartTime = AudioSettings.dspTime - scheduledDspStartTime;
            }
            else
            {
                chartTime = musicSource.time;
            }
        }
        else
        {
            chartTime = Time.timeAsDouble - fallbackClockStartTime;
        }

        chartTime += manualChartSyncOffsetSeconds;
        return chartTime < 0d ? 0d : chartTime;
    }

    AudioSource FindSceneAudioSource()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>(true);
        if (sources == null || sources.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null && musicClip != null && sources[i].clip == musicClip)
            {
                return sources[i];
            }
        }

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null && sources[i].clip != null)
            {
                return sources[i];
            }
        }

        return sources[0];
    }

    AudioClip ResolveMusicClip(AudioClip currentClip)
    {
        if (currentClip != null)
        {
            return currentClip;
        }

        AudioClip resourceClip = Resources.Load<AudioClip>(DefaultMusicResourcePath);
        if (resourceClip != null)
        {
            Debug.Log("BattleRhythmChartRunner: clip musica caricata da Resources '" + DefaultMusicResourcePath + "'.");
            return resourceClip;
        }

#if UNITY_EDITOR
        AudioClip clipFromPath = AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultMusicPath);
        if (clipFromPath != null)
        {
            Debug.Log("BattleRhythmChartRunner: clip musica caricata da AssetDatabase path '" + DefaultMusicPath + "'.");
            return clipFromPath;
        }

        string guidPath = AssetDatabase.GUIDToAssetPath(DefaultMusicAssetGuid);
        if (!string.IsNullOrEmpty(guidPath))
        {
            AudioClip clipFromGuid = AssetDatabase.LoadAssetAtPath<AudioClip>(guidPath);
            if (clipFromGuid != null)
            {
                Debug.Log("BattleRhythmChartRunner: clip musica caricata da GUID '" + DefaultMusicAssetGuid + "'.");
                return clipFromGuid;
            }
        }

        string[] audioGuids = AssetDatabase.FindAssets("Tutorial Battaglia Epic Metal Feels t:AudioClip");
        for (int i = 0; i < audioGuids.Length; i++)
        {
            string audioPath = AssetDatabase.GUIDToAssetPath(audioGuids[i]);
            AudioClip foundClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
            if (foundClip != null && foundClip.name == "Tutorial Battaglia   Epic Metal Feels")
            {
                Debug.Log("BattleRhythmChartRunner: clip musica caricata tramite ricerca AssetDatabase '" + audioPath + "'.");
                return foundClip;
            }
        }

        Debug.LogWarning(
            "BattleRhythmChartRunner: impossibile risolvere AudioClip. resourcePath='" +
            DefaultMusicResourcePath + "', assetPath='" + DefaultMusicPath + "', guidPath='" + guidPath + "'.");
        return null;
#else
        return null;
#endif
    }

    IEnumerator LoadDefaultMusicClipFromFile()
    {
        string absolutePath = Path.Combine(Application.dataPath, DefaultMusicProjectRelativePath);
        if (!File.Exists(absolutePath))
        {
            Debug.LogWarning("BattleRhythmChartRunner: file musica non trovato su disco '" + absolutePath + "'.");
            yield break;
        }

        string fileUri = new Uri(absolutePath).AbsoluteUri;
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "BattleRhythmChartRunner: caricamento mp3 da file fallito. uri='" +
                    fileUri + "', error='" + request.error + "'.");
                yield break;
            }

            musicClip = DownloadHandlerAudioClip.GetContent(request);
            if (musicClip != null)
            {
                musicClip.name = "Tutorial Battaglia   Epic Metal Feels";
                Debug.Log("BattleRhythmChartRunner: clip musica caricata da file '" + absolutePath + "'.");
            }
        }
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

            Keys buttonKey = BattleManager.Instance != null
                ? BattleManager.Instance.GetLaneButtonAction(scheduled.LaneIndex)
                : scheduled.Key;

            battleManager.CreateButton(
                scheduled.Cell,
                buttonKey,
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

    void TryLoadDefaultMusic()
    {
        musicClip = ResolveMusicClip(musicClip);
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
