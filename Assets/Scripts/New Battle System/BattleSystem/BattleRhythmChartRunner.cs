using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BattleRhythmChartRunner : MonoBehaviour
{
    const string DefaultChartPath =
        "Assets/RhythmCombat/Generated/Charts/" +
        "Tutorial Battaglia   Epic Metal Feels_" +
        "pump-halfdouble_Beginner.asset";

    const string DefaultMusicPath =
        "Assets/Scriptables/Resources/Battle/Charts/Tutorial/" +
        "Tutorial Battaglia   Epic Metal Feels.mp3";

    const string DefaultMusicResourcePath =
        "Battle/Charts/Tutorial/" +
        "Tutorial Battaglia   Epic Metal Feels";

    const string DefaultMusicProjectRelativePath =
        "Scriptables/Resources/Battle/Charts/Tutorial/" +
        "Tutorial Battaglia   Epic Metal Feels.mp3";

    const string DefaultMusicAssetGuid =
        "9770822bd7e92344db8574c32fe148e2";

    [Header("Chart")]
    [SerializeField] ChartDataAsset chart;
    [SerializeField] int maxNotesToSchedule = 128;
    [SerializeField] float approachDurationSeconds = 2f;
    [SerializeField] float simultaneousToleranceSeconds = 0.0005f;

    [Header("Audio")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioClip musicClip;
    [SerializeField] bool useAudioSourceClock = true;
    [SerializeField] bool playMusicOnStart = true;
    [SerializeField] bool scheduleMusicWithDspClock = true;
    [SerializeField] float audioScheduleLeadSeconds = 0.1f;
    [SerializeField] float manualChartSyncOffsetSeconds = 0f;

    [Header("Playback")]
    [SerializeField] bool playOnStart = true;
    [SerializeField] float startDelaySeconds = 1.5f;

    [Header("Colors")]
    [SerializeField] Color attackColor =
        new Color(0.95f, 0.25f, 0.2f, 1f);

    [SerializeField] Color defenseColor =
        new Color(0.2f, 0.55f, 1f, 1f);

    [SerializeField] Color holdColor =
        new Color(0.9f, 0.75f, 0.2f, 1f);

    [Header("Scene References")]
    [SerializeField] BMBattleManager battleManager;
    [SerializeField] BattleChordFlashController chordFlashController;

    readonly List<ScheduledButton> scheduledButtons =
        new List<ScheduledButton>(256);

    double elapsedSeconds;
    double fallbackClockStartTime;
    double scheduledDspStartTime;
    int nextButtonIndex;
    bool isPlaying;
    bool isPaused;
    bool musicWasPlayingBeforePause;
    double pauseStartedRealtime;
    double pauseStartedDspTime;

    public double CurrentChartTimeSeconds =>
        elapsedSeconds;

    public bool IsPaused => isPaused;

#if UNITY_EDITOR
    public void ConfigureEditorReferences(
        BMBattleManager manager,
        BattleChordFlashController chordController)
    {
        battleManager = manager;
        chordFlashController = chordController;
    }
#endif

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
            battleManager =
                FindObjectOfType<BMBattleManager>();
        }

        if (chordFlashController == null)
        {
            chordFlashController =
                FindObjectOfType<BattleChordFlashController>();
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
        if (!isPlaying || isPaused)
        {
            return;
        }

        elapsedSeconds = GetChartClockSeconds();

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetChartTimeSeconds(
                elapsedSeconds);
        }

        SpawnDueButtons();
    }

    public void SetPaused(bool paused)
    {
        if (isPaused == paused)
        {
            return;
        }

        isPaused = paused;

        if (isPaused)
        {
            pauseStartedRealtime = Time.unscaledTimeAsDouble;
            pauseStartedDspTime = AudioSettings.dspTime;
            musicWasPlayingBeforePause =
                musicSource != null && musicSource.isPlaying;

            if (musicWasPlayingBeforePause)
            {
                musicSource.Pause();
            }

            return;
        }

        double realtimePauseDuration =
            Time.unscaledTimeAsDouble - pauseStartedRealtime;

        double dspPauseDuration =
            AudioSettings.dspTime - pauseStartedDspTime;

        fallbackClockStartTime +=
            Math.Max(0d, realtimePauseDuration);

        if (scheduledDspStartTime > 0d)
        {
            scheduledDspStartTime +=
                Math.Max(0d, dspPauseDuration);
        }

        if (musicWasPlayingBeforePause && musicSource != null)
        {
            musicSource.UnPause();
        }

        musicWasPlayingBeforePause = false;
    }

    public void Play()
    {
        if (chart == null || battleManager == null)
        {
            Debug.LogWarning(
                "BattleRhythmChartRunner: chart o " +
                "BMBattleManager mancanti.");

            return;
        }

        elapsedSeconds = 0d;
        isPaused = false;
        musicWasPlayingBeforePause = false;
        fallbackClockStartTime = Time.unscaledTimeAsDouble;
        scheduledDspStartTime = 0d;
        nextButtonIndex = 0;

        PrepareAndPlayMusic();
        isPlaying = true;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.SetChartTimeSeconds(
                elapsedSeconds);
        }
    }

    void PrepareAndPlayMusic()
    {
        if (musicSource == null)
        {
            fallbackClockStartTime =
                Time.unscaledTimeAsDouble;

            return;
        }

        musicClip = ResolveMusicClip(
            musicClip != null
                ? musicClip
                : musicSource.clip);

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
        }

        musicSource.playOnAwake = false;
        musicSource.time = 0f;

        if (!playMusicOnStart ||
            musicSource.clip == null)
        {
            fallbackClockStartTime =
                Time.unscaledTimeAsDouble;

            return;
        }

        if (musicSource.clip.loadState !=
            AudioDataLoadState.Loaded)
        {
            musicSource.clip.LoadAudioData();
        }

        if (scheduleMusicWithDspClock)
        {
            scheduledDspStartTime =
                AudioSettings.dspTime +
                Mathf.Max(
                    0f,
                    audioScheduleLeadSeconds);

            musicSource.PlayScheduled(
                scheduledDspStartTime);
        }
        else
        {
            musicSource.Play();
            scheduledDspStartTime =
                AudioSettings.dspTime;
        }
    }

    double GetChartClockSeconds()
    {
        double chartTime;

        if (useAudioSourceClock &&
            musicSource != null &&
            playMusicOnStart &&
            musicSource.clip != null)
        {
            chartTime =
                scheduleMusicWithDspClock &&
                scheduledDspStartTime > 0d
                    ? AudioSettings.dspTime -
                      scheduledDspStartTime
                    : musicSource.time;
        }
        else
        {
            chartTime =
                Time.unscaledTimeAsDouble -
                fallbackClockStartTime;
        }

        chartTime += manualChartSyncOffsetSeconds;

        return chartTime < 0d
            ? 0d
            : chartTime;
    }

    void BuildSchedule()
    {
        scheduledButtons.Clear();
        nextButtonIndex = 0;

        if (chart == null)
        {
            Debug.LogWarning(
                "BattleRhythmChartRunner: chart mancante.");

            return;
        }

        PendingHold[] pendingHolds =
            new PendingHold[6];

        int playableCount = 0;

        for (int rowIndex = 0;
             rowIndex < chart.rows.Count;
             rowIndex++)
        {
            ChartRow row = chart.rows[rowIndex];

            if (row == null || row.lanes == null)
            {
                continue;
            }

            int laneCount =
                Mathf.Min(6, row.lanes.Length);

            for (int laneIndex = 0;
                 laneIndex < laneCount;
                 laneIndex++)
            {
                byte laneValue =
                    row.lanes[laneIndex];

                if (laneValue == 0)
                {
                    continue;
                }

                if (laneValue == 2)
                {
                    pendingHolds[laneIndex] =
                        new PendingHold(
                            rowIndex,
                            row.time);

                    continue;
                }

                if (laneValue == 3)
                {
                    PendingHold pending =
                        pendingHolds[laneIndex];

                    if (!pending.IsActive)
                    {
                        Debug.LogWarning(
                            "BattleRhythmChartRunner: " +
                            "hold end senza start, lane " +
                            laneIndex + ".");

                        continue;
                    }

                    if (playableCount <
                        maxNotesToSchedule)
                    {
                        scheduledButtons.Add(
                            CreateScheduledButton(
                                pending.RowIndex,
                                laneIndex,
                                2,
                                pending.HitTimeSeconds,
                                true,
                                row.time));

                        playableCount++;
                    }

                    pendingHolds[laneIndex] =
                        default;

                    continue;
                }

                if (playableCount >=
                    maxNotesToSchedule)
                {
                    continue;
                }

                scheduledButtons.Add(
                    CreateScheduledButton(
                        rowIndex,
                        laneIndex,
                        laneValue,
                        row.time,
                        false,
                        0d));

                playableCount++;
            }
        }

        for (int laneIndex = 0;
             laneIndex < pendingHolds.Length &&
             playableCount < maxNotesToSchedule;
             laneIndex++)
        {
            PendingHold pending =
                pendingHolds[laneIndex];

            if (!pending.IsActive)
            {
                continue;
            }

            Debug.LogWarning(
                "BattleRhythmChartRunner: " +
                "hold start senza end, lane " +
                laneIndex +
                ". Convertito in tap.");

            scheduledButtons.Add(
                CreateScheduledButton(
                    pending.RowIndex,
                    laneIndex,
                    1,
                    pending.HitTimeSeconds,
                    false,
                    0d));

            playableCount++;
        }

        AssignSimultaneousGroups();

        scheduledButtons.Sort(
            CompareBySpawnTime);
    }

    ScheduledButton CreateScheduledButton(
        int rowIndex,
        int laneIndex,
        byte laneValue,
        double hitTimeSeconds,
        bool isHold,
        double holdEndTimeSeconds)
    {
        double spawnTime =
            hitTimeSeconds -
            approachDurationSeconds -
            0.1d;

        if (spawnTime < 0d)
        {
            spawnTime = 0d;
        }

        return new ScheduledButton
        {
            RowIndex = rowIndex,
            LaneIndex = laneIndex,
            LaneValue = laneValue,
            SpawnTimeSeconds = spawnTime,
            HitTimeSeconds = hitTimeSeconds,
            IsHold = isHold,
            HoldEndTimeSeconds = holdEndTimeSeconds,
            Cell = ToCell(laneIndex),
            Color = ToColor(laneIndex, laneValue),
            ChordGroupId = -1,
            ChordSize = 1
        };
    }

    void AssignSimultaneousGroups()
    {
        scheduledButtons.Sort(
            CompareByHitTime);

        int nextGroupId = 0;
        int startIndex = 0;

        while (startIndex <
               scheduledButtons.Count)
        {
            double referenceTime =
                scheduledButtons[startIndex]
                    .HitTimeSeconds;

            int endIndex =
                startIndex + 1;

            while (endIndex <
                   scheduledButtons.Count &&
                   Math.Abs(
                       scheduledButtons[endIndex]
                           .HitTimeSeconds -
                       referenceTime) <=
                   simultaneousToleranceSeconds)
            {
                endIndex++;
            }

            int count =
                endIndex - startIndex;

            if (count >= 2)
            {
                for (int i = startIndex;
                     i < endIndex;
                     i++)
                {
                    ScheduledButton scheduled =
                        scheduledButtons[i];

                    scheduled.ChordGroupId =
                        nextGroupId;

                    scheduled.ChordSize =
                        count;

                    scheduledButtons[i] =
                        scheduled;
                }

                nextGroupId++;
            }

            startIndex = endIndex;
        }
    }

    void SpawnDueButtons()
    {
        while (nextButtonIndex <
                   scheduledButtons.Count &&
               scheduledButtons[nextButtonIndex]
                   .SpawnTimeSeconds <=
               elapsedSeconds)
        {
            ScheduledButton scheduled =
                scheduledButtons[nextButtonIndex];

            if (battleManager == null)
            {
                return;
            }

            BattleButton button =
                battleManager.CreateButton(
                    scheduled.Cell,
                    Keys.NONE,
                    null,
                    scheduled.Color,
                    scheduled.LaneIndex,
                    scheduled.HitTimeSeconds);

            if (button != null &&
                scheduled.IsHold)
            {
                button.ConfigureHold(
                    scheduled.HoldEndTimeSeconds,
                    holdColor,
                    approachDurationSeconds);
            }

            if (button != null &&
                scheduled.ChordGroupId >= 0)
            {
                chordFlashController?.RegisterSpawn(
                    scheduled.ChordGroupId,
                    scheduled.ChordSize,
                    button);
            }

            nextButtonIndex++;
        }
    }

    static int CompareByHitTime(
        ScheduledButton left,
        ScheduledButton right)
    {
        int timeComparison =
            left.HitTimeSeconds.CompareTo(
                right.HitTimeSeconds);

        return timeComparison != 0
            ? timeComparison
            : left.LaneIndex.CompareTo(
                right.LaneIndex);
    }

    static int CompareBySpawnTime(
        ScheduledButton left,
        ScheduledButton right)
    {
        int timeComparison =
            left.SpawnTimeSeconds.CompareTo(
                right.SpawnTimeSeconds);

        return timeComparison != 0
            ? timeComparison
            : left.LaneIndex.CompareTo(
                right.LaneIndex);
    }

    static BMButtonPrefab.Cell ToCell(
        int laneIndex)
    {
        if (laneIndex < 0 || laneIndex > 5)
        {
            return BMButtonPrefab.Cell.Cell1;
        }

        return (BMButtonPrefab.Cell)laneIndex;
    }

    Color ToColor(
        int laneIndex,
        byte laneValue)
    {
        if (laneValue == 2 ||
            laneValue == 3)
        {
            return holdColor;
        }

        return laneIndex < 3
            ? attackColor
            : defenseColor;
    }

    AudioSource FindSceneAudioSource()
    {
        AudioSource[] sources =
            FindObjectsOfType<AudioSource>(true);

        if (sources == null ||
            sources.Length == 0)
        {
            return null;
        }

        for (int i = 0;
             i < sources.Length;
             i++)
        {
            if (sources[i] != null &&
                musicClip != null &&
                sources[i].clip == musicClip)
            {
                return sources[i];
            }
        }

        for (int i = 0;
             i < sources.Length;
             i++)
        {
            if (sources[i] != null &&
                sources[i].clip != null)
            {
                return sources[i];
            }
        }

        return sources[0];
    }

    AudioClip ResolveMusicClip(
        AudioClip currentClip)
    {
        if (currentClip != null)
        {
            return currentClip;
        }

        AudioClip resourceClip =
            Resources.Load<AudioClip>(
                DefaultMusicResourcePath);

        if (resourceClip != null)
        {
            return resourceClip;
        }

#if UNITY_EDITOR
        AudioClip clipFromPath =
            AssetDatabase.LoadAssetAtPath<AudioClip>(
                DefaultMusicPath);

        if (clipFromPath != null)
        {
            return clipFromPath;
        }

        string guidPath =
            AssetDatabase.GUIDToAssetPath(
                DefaultMusicAssetGuid);

        if (!string.IsNullOrEmpty(guidPath))
        {
            AudioClip clipFromGuid =
                AssetDatabase.LoadAssetAtPath<AudioClip>(
                    guidPath);

            if (clipFromGuid != null)
            {
                return clipFromGuid;
            }
        }
#endif

        return null;
    }

    IEnumerator LoadDefaultMusicClipFromFile()
    {
        string absolutePath =
            Path.Combine(
                Application.dataPath,
                DefaultMusicProjectRelativePath);

        if (!File.Exists(absolutePath))
        {
            yield break;
        }

        string fileUri =
            new Uri(absolutePath).AbsoluteUri;

        using (UnityWebRequest request =
               UnityWebRequestMultimedia.GetAudioClip(
                   fileUri,
                   AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                yield break;
            }

            musicClip =
                DownloadHandlerAudioClip.GetContent(
                    request);

            if (musicClip != null)
            {
                musicClip.name =
                    "Tutorial Battaglia   " +
                    "Epic Metal Feels";
            }
        }
    }

    void TryLoadDefaultChart()
    {
        if (chart != null)
        {
            return;
        }

#if UNITY_EDITOR
        chart =
            AssetDatabase.LoadAssetAtPath<ChartDataAsset>(
                DefaultChartPath);
#endif
    }

    struct PendingHold
    {
        public PendingHold(
            int rowIndex,
            double hitTimeSeconds)
        {
            RowIndex = rowIndex;
            HitTimeSeconds = hitTimeSeconds;
            IsActive = true;
        }

        public int RowIndex;
        public double HitTimeSeconds;
        public bool IsActive;
    }

    struct ScheduledButton
    {
        public int RowIndex;
        public int LaneIndex;
        public byte LaneValue;
        public double SpawnTimeSeconds;
        public double HitTimeSeconds;
        public bool IsHold;
        public double HoldEndTimeSeconds;
        public BMButtonPrefab.Cell Cell;
        public Color Color;
        public int ChordGroupId;
        public int ChordSize;
    }
}
