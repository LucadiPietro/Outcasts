using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class BattleSceneRuntimeBootstrap : MonoBehaviour
{
    const string TargetBattleScenePath = "Assets/Scenes/Battle.unity";
    const string TimelineResourcePath = "BattleSystem/Beat GuardiansOfTheLight";

    static readonly string[] CellNames =
    {
        "Cell1",
        "Cell2",
        "Cell3",
        "Cell4",
        "Cell5",
        "Cell6"
    };

    [SerializeField] bool logSetup = true;
    [SerializeField] float laneColumnSpacing = 520f;
    [SerializeField] float runtimeBarHeight = 64f;
    [SerializeField] float exitOffset = 100f;
    [SerializeField] float timeBeforeStart = 1.5f;
    [SerializeField] float timeToReachBar = 2f;
    [SerializeField] bool useChartDataAsset = true;
    [SerializeField] bool playTimelineFallback;
    [SerializeField] bool enableSpatialMotion = true;
    [SerializeField] int maxChartNotesToSpawn = 128;
    [SerializeField] float spatialToleranceRadius = 1f;
    [SerializeField] float spatialPerfectPercent = 0.1f;
    [SerializeField] float spatialGoodPercent = 0.25f;
    [SerializeField] float spatialBadPercent = 0.5f;
    [SerializeField] float spatialDespawnAfterHitSeconds = 1f;

    Canvas canvas;
    RectTransform canvasRect;
    RectTransform runtimeRoot;

    static void InstallForInitialScene()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    static void TryInstall(Scene scene)
    {
        if (scene.path != TargetBattleScenePath)
        {
            return;
        }

        if (FindObjectOfType<BattleSceneRuntimeBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("Battle Scene Runtime Bootstrap");
        bootstrapObject.AddComponent<BattleSceneRuntimeBootstrap>();
    }

    void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: nessun Canvas trovato nella scena Battle.");
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();
        runtimeRoot = EnsureRuntimeRoot(canvasRect);

        TextMeshProUGUI counter = EnsureCounter(runtimeRoot);
        TextMeshProUGUI feedback = EnsureFeedback(runtimeRoot);
        PlayableDirector director = EnsurePlayableDirector();
        List<GameObject> cells = EnsureCells(runtimeRoot);
        List<GameObject> bars = EnsureBars(runtimeRoot);
        BattleButton buttonTemplate = EnsureButtonTemplate(runtimeRoot);

        BattleInputManager inputManager = GetOrAdd<BattleInputManager>(gameObject);
        BattleUIManager uiManager = GetOrAdd<BattleUIManager>(gameObject);
        BattleManager battleManager = GetOrAdd<BattleManager>(gameObject);
        BMBattleManager beatMapManager = GetOrAdd<BMBattleManager>(gameObject);
        CellDivisor cellDivisor = GetOrAdd<CellDivisor>(gameObject);
        BattleRhythmChartRunner chartRunner = GetOrAdd<BattleRhythmChartRunner>(gameObject);

        uiManager.counter = counter;
        uiManager.feedback = feedback;

        battleManager.timeBeforeStart = timeBeforeStart;
        battleManager.audioController = director.gameObject;
        battleManager.playTimelineOnStart = !useChartDataAsset || playTimelineFallback;
        battleManager.players = EnsurePlayers();
        battleManager.enemies = EnsureEnemies();
        battleManager.ConfigureSpatialJudgment(
            timeToReachBar,
            spatialToleranceRadius,
            spatialPerfectPercent,
            spatialGoodPercent,
            spatialBadPercent,
            spatialDespawnAfterHitSeconds);

        beatMapManager.cells = BuildCellDictionary(cells);
        beatMapManager.bars = bars;
        beatMapManager.buttonPrefab = buttonTemplate;
        beatMapManager.timeToReachBar = timeToReachBar;
        beatMapManager.enableSpatialMotion = enableSpatialMotion;

        cellDivisor.playableDirector = director;
        if (director.playableAsset is TimelineAsset)
        {
            cellDivisor.SetParent();
        }

        chartRunner.Configure(
            null,
            beatMapManager,
            timeBeforeStart,
            timeToReachBar,
            maxChartNotesToSpawn,
            useChartDataAsset);

        if (logSetup)
        {
            Debug.Log("BattleSceneRuntimeBootstrap: Battle scene collegata a BattleManager, BMBattleManager, celle, barre e timeline.");
        }

        _ = inputManager;
    }

    RectTransform EnsureRuntimeRoot(RectTransform parent)
    {
        Transform existing = parent.Find("BattleRuntime");
        if (existing != null)
        {
            return (RectTransform)existing;
        }

        GameObject rootObject = new GameObject("BattleRuntime", typeof(RectTransform));
        RectTransform root = rootObject.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = parent.rect.size;
        return root;
    }

    TextMeshProUGUI EnsureCounter(RectTransform parent)
    {
        Transform existing = parent.Find("RuntimeCounter");
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        GameObject counterObject = new GameObject("RuntimeCounter", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = counterObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -32f);
        rect.sizeDelta = new Vector2(180f, 64f);

        TextMeshProUGUI text = counterObject.GetComponent<TextMeshProUGUI>();
        text.text = "0";
        text.fontSize = 42f;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    TextMeshProUGUI EnsureFeedback(RectTransform parent)
    {
        Transform existing = parent.Find("RuntimeFeedback");
        if (existing != null)
        {
            return existing.GetComponent<TextMeshProUGUI>();
        }

        GameObject feedbackObject = new GameObject("RuntimeFeedback", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = feedbackObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -94f);
        rect.sizeDelta = new Vector2(420f, 60f);

        TextMeshProUGUI text = feedbackObject.GetComponent<TextMeshProUGUI>();
        text.text = "WAIT";
        text.fontSize = 30f;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.color = Color.white;
        return text;
    }

    PlayableDirector EnsurePlayableDirector()
    {
        GameObject audioController = GameObject.Find("AudioController");
        if (audioController == null)
        {
            audioController = new GameObject("AudioController");
        }

        PlayableDirector director = GetOrAdd<PlayableDirector>(audioController);
        AudioSource audioSource = GetOrAdd<AudioSource>(audioController);

        director.playOnAwake = false;
        director.playableAsset = Resources.Load<PlayableAsset>(TimelineResourcePath);
        director.extrapolationMode = DirectorWrapMode.None;

        BindTimelineAudio(director, audioSource);
        return director;
    }

    void BindTimelineAudio(PlayableDirector director, AudioSource audioSource)
    {
        TimelineAsset timeline = director.playableAsset as TimelineAsset;
        if (timeline == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: timeline BattleSystem/Beat GuardiansOfTheLight non trovata.");
            return;
        }

        foreach (TrackAsset track in timeline.GetOutputTracks())
        {
            if (track is AudioTrack)
            {
                director.SetGenericBinding(track, audioSource);
            }
        }
    }

    List<GameObject> EnsureCells(RectTransform parent)
    {
        var cells = new List<GameObject>();
        float spacing = Mathf.Min(laneColumnSpacing, Mathf.Max(300f, canvasRect.rect.width * 0.28f));
        float[] columns = { -spacing, 0f, spacing };

        for (int i = 0; i < CellNames.Length; i++)
        {
            string cellName = CellNames[i];
            Transform existing = parent.Find(cellName);
            GameObject cellObject = existing != null ? existing.gameObject : CreateCell(parent, cellName);

            RectTransform rect = cellObject.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(columns[i % 3], 0f);
            rect.sizeDelta = new Vector2(120f, 120f);
            TrySetTag(cellObject, cellName);

            cells.Add(cellObject);
        }

        return cells;
    }

    GameObject CreateCell(RectTransform parent, string cellName)
    {
        GameObject cellObject = new GameObject(cellName, typeof(RectTransform));
        RectTransform rect = cellObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        return cellObject;
    }

    List<GameObject> EnsureBars(RectTransform parent)
    {
        RectTransform lineUp = FindRectTransform("LineUp");
        RectTransform lineDown = FindRectTransform("LineDown");

        float topY = lineUp != null ? lineUp.anchoredPosition.y : 460f;
        float bottomY = lineDown != null ? lineDown.anchoredPosition.y : -460f;
        float width = canvasRect.rect.width > 0f ? canvasRect.rect.width : 1920f;

        GameObject upHit = EnsureTriggerBar(parent, "UpBar", topY, width, "Bar", false);
        GameObject downHit = EnsureTriggerBar(parent, "DownBar", bottomY, width, "Bar", false);

        EnsureTriggerBar(parent, "UpBarUnsub", topY + exitOffset, width, "Untagged", true);
        EnsureTriggerBar(parent, "DownBarUnsub", bottomY - exitOffset, width, "Untagged", true);

        return new List<GameObject> { upHit, downHit };
    }

    GameObject EnsureTriggerBar(RectTransform parent, string objectName, float y, float width, string tagName, bool unsubscribe)
    {
        Transform existing = parent.Find(objectName);
        GameObject barObject = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(BoxCollider2D));
        RectTransform rect = barObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(width, runtimeBarHeight);

        Image image = barObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;
        }

        BoxCollider2D collider = GetOrAdd<BoxCollider2D>(barObject);
        collider.isTrigger = true;
        collider.size = rect.sizeDelta;

        TrySetTag(barObject, tagName);

        if (unsubscribe)
        {
            GetOrAdd<BattleBarToUnsubcribe>(barObject);
        }

        return barObject;
    }

    BattleButton EnsureButtonTemplate(RectTransform parent)
    {
        Transform existing = parent.Find("RuntimeBattleButtonTemplate");
        if (existing != null)
        {
            return existing.GetComponent<BattleButton>();
        }

        GameObject buttonObject = new GameObject(
            "RuntimeBattleButtonTemplate",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(BattleButton),
            typeof(BoxCollider2D),
            typeof(Rigidbody2D));

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(50f, 50f);

        Image image = buttonObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;

        BoxCollider2D collider = buttonObject.GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = rect.sizeDelta;

        Rigidbody2D body = buttonObject.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        BattleButton button = buttonObject.GetComponent<BattleButton>();
        button.image = image;
        button.timeToReachBar = timeToReachBar;

        buttonObject.SetActive(false);
        return button;
    }

    List<Player> EnsurePlayers()
    {
        var players = new List<Player>(FindObjectsOfType<Player>(true));
        if (players.Count > 0)
        {
            return players;
        }

        BMButtonPrefab.Cell[] cells =
        {
            BMButtonPrefab.Cell.Cell4,
            BMButtonPrefab.Cell.Cell5,
            BMButtonPrefab.Cell.Cell6
        };

        for (int i = 0; i < cells.Length; i++)
        {
            GameObject playerObject = new GameObject("Runtime Player " + (i + 1));
            playerObject.transform.SetParent(transform, false);
            Player player = playerObject.AddComponent<Player>();
            player.cell = cells[i];
            player.startingHealth = 100;
            player.actualHealth = 100;
            player.attack = 10f;
            player.defence = 4f;
            player.damageConstant = 1f;
            player.positionMultiplayer = 1f;
            players.Add(player);
        }

        return players;
    }

    List<Enemy> EnsureEnemies()
    {
        var enemies = new List<Enemy>(FindObjectsOfType<Enemy>(true));
        if (enemies.Count > 0)
        {
            return enemies;
        }

        BMButtonPrefab.Cell[] cells =
        {
            BMButtonPrefab.Cell.Cell1,
            BMButtonPrefab.Cell.Cell2,
            BMButtonPrefab.Cell.Cell3
        };

        for (int i = 0; i < cells.Length; i++)
        {
            GameObject enemyObject = new GameObject("Runtime Enemy " + (i + 1));
            enemyObject.transform.SetParent(transform, false);
            Enemy enemy = enemyObject.AddComponent<Enemy>();
            enemy.cell = cells[i];
            enemy.startingHealth = 100;
            enemy.actualHealth = 100;
            enemy.attack = 8f;
            enemy.defence = 3f;
            enemy.damageConstant = 1f;
            enemies.Add(enemy);
        }

        return enemies;
    }

    SDictionary<string, GameObject> BuildCellDictionary(List<GameObject> cells)
    {
        var dictionary = new SDictionary<string, GameObject>();
        for (int i = 0; i < cells.Count; i++)
        {
            dictionary[CellNames[i]] = cells[i];
        }

        return dictionary;
    }

    RectTransform FindRectTransform(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<RectTransform>() : null;
    }

    T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return target.AddComponent<T>();
    }

    void TrySetTag(GameObject target, string tagName)
    {
        try
        {
            target.tag = tagName;
        }
        catch (UnityException)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: tag non configurato: " + tagName);
        }
    }
}
