using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
public class BattleSceneRuntimeBootstrap : MonoBehaviour
{
    const string TargetBattleScenePath = "Assets/Scenes/Battle.unity";

    static readonly string[] CellNames =
    {
        "Cell1",
        "Cell2",
        "Cell3",
        "Cell4",
        "Cell5",
        "Cell6"
    };

    [SerializeField] bool autoWireExistingReferences = true;
    [SerializeField] bool logSetup = true;

    void Awake()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != TargetBattleScenePath)
        {
            return;
        }

        if (!autoWireExistingReferences)
        {
            LogSceneStatus();
            return;
        }

        WireExistingSceneReferences();
    }

    void WireExistingSceneReferences()
    {
        BattleManager battleManager = FindFirst<BattleManager>();
        BattleUIManager uiManager = FindFirst<BattleUIManager>();
        BMBattleManager beatMapManager = FindFirst<BMBattleManager>();
        BattleRhythmChartRunner chartRunner = FindFirst<BattleRhythmChartRunner>();
        PlayableDirector director = FindFirst<PlayableDirector>();
        AudioSource audioSource = FindFirst<AudioSource>();

        if (battleManager != null)
        {
            if (battleManager.battleUIManager == null)
            {
                battleManager.battleUIManager = uiManager;
            }

            if (battleManager.audioController == null)
            {
                if (director != null)
                {
                    battleManager.audioController = director.gameObject;
                }
                else if (audioSource != null)
                {
                    battleManager.audioController = audioSource.gameObject;
                }
            }

            if (battleManager.players == null || battleManager.players.Count == 0)
            {
                battleManager.players = FindScenePlayers();
            }

            if (battleManager.enemies == null || battleManager.enemies.Count == 0)
            {
                battleManager.enemies = FindSceneEnemies();
            }
        }

        if (beatMapManager != null)
        {
            if (beatMapManager.cells == null || beatMapManager.cells.Count == 0)
            {
                beatMapManager.cells = FindExistingCells();
            }

            if (beatMapManager.bars == null || beatMapManager.bars.Count == 0)
            {
                beatMapManager.bars = FindExistingBars();
            }
        }

        if (logSetup)
        {
            Debug.Log(
                "BattleSceneRuntimeBootstrap: riferimenti esistenti verificati. " +
                "Non vengono creati manager, Canvas, AudioSource, celle, barre, player o enemy a runtime.");
        }

        LogMissingReferences(battleManager, uiManager, beatMapManager, chartRunner, audioSource);
    }

    void LogSceneStatus()
    {
        if (!logSetup)
        {
            return;
        }

        Debug.Log("BattleSceneRuntimeBootstrap: auto wiring disattivato. La scena Battle deve avere tutti i riferimenti configurati in Inspector.");
    }

    void LogMissingReferences(
        BattleManager battleManager,
        BattleUIManager uiManager,
        BMBattleManager beatMapManager,
        BattleRhythmChartRunner chartRunner,
        AudioSource audioSource)
    {
        if (!logSetup)
        {
            return;
        }

        if (battleManager == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: BattleManager mancante in scena.");
        }

        if (uiManager == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: BattleUIManager mancante in scena.");
        }

        if (beatMapManager == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: BMBattleManager mancante in scena.");
        }

        if (chartRunner == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: BattleRhythmChartRunner mancante in scena.");
        }

        if (audioSource == null)
        {
            Debug.LogWarning("BattleSceneRuntimeBootstrap: AudioSource mancante in scena.");
        }
    }

    SDictionary<string, GameObject> FindExistingCells()
    {
        var dictionary = new SDictionary<string, GameObject>();
        for (int i = 0; i < CellNames.Length; i++)
        {
            GameObject cell = GameObject.Find(CellNames[i]);
            if (cell != null)
            {
                dictionary[CellNames[i]] = cell;
            }
        }

        return dictionary;
    }

    List<GameObject> FindExistingBars()
    {
        var bars = new List<GameObject>();
        AddIfFound(bars, "UpBar");
        AddIfFound(bars, "DownBar");

        if (bars.Count == 0)
        {
            AddIfFound(bars, "LineUp");
            AddIfFound(bars, "LineDown");
        }

        return bars;
    }

    List<Player> FindScenePlayers()
    {
        Player[] foundPlayers = FindObjectsOfType<Player>(true);
        var result = new List<Player>(foundPlayers);
        result.Sort((left, right) => ((int)left.cell).CompareTo((int)right.cell));
        return result;
    }

    List<Enemy> FindSceneEnemies()
    {
        Enemy[] foundEnemies = FindObjectsOfType<Enemy>(true);
        var result = new List<Enemy>(foundEnemies);
        result.Sort((left, right) => ((int)left.cell).CompareTo((int)right.cell));
        return result;
    }

    void AddIfFound(List<GameObject> objects, string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        if (found != null)
        {
            objects.Add(found);
        }
    }

    T FindFirst<T>() where T : Object
    {
        T[] objects = FindObjectsOfType<T>(true);
        return objects != null && objects.Length > 0 ? objects[0] : null;
    }
}
