using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Mantiene coerenti i riferimenti Player/Enemy usati dal BattleManager.
///
/// Il sistema di combattimento si aspetta:
/// - Enemy sulle celle Cell1, Cell2 e Cell3;
/// - Player sulle celle Cell4, Cell5 e Cell6.
///
/// In Editor il componente conserva soltanto il riferimento al BattleManager:
/// non forza il binding mentre l'installer sta ricostruendo la scena e non
/// sovrascrive liste valide con liste vuote. In Play Mode riprova per un breve
/// intervallo, cosi' supporta anche personaggi istanziati da altri sistemi.
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
[AddComponentMenu("Rhythm Combat/Battle Character Auto Binder")]
public sealed class BattleCharacterAutoBinder : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;

    [Tooltip("Assegna automaticamente Cell1-3 agli Enemy e Cell4-6 ai Player quando una cella richiesta e' mancante.")]
    [SerializeField] private bool assignMissingExpectedCells = true;

    [Tooltip("Numero massimo di tentativi eseguiti in Play Mode per attendere personaggi istanziati a runtime.")]
    [SerializeField, Min(1)] private int runtimeBindAttempts = 20;

    [Tooltip("Intervallo reale tra i tentativi di binding in Play Mode.")]
    [SerializeField, Min(0.01f)] private float runtimeBindRetrySeconds = 0.15f;

    [Tooltip("Scrive un log anche quando tutti i riferimenti sono stati configurati correttamente.")]
    [SerializeField] private bool logSuccessfulBinding;

    private bool hasLoggedIncompleteSetup;
    private Coroutine runtimeBindingRoutine;

    private static readonly BMButtonPrefab.Cell[] ExpectedEnemyCells =
    {
        BMButtonPrefab.Cell.Cell1,
        BMButtonPrefab.Cell.Cell2,
        BMButtonPrefab.Cell.Cell3
    };

    private static readonly BMButtonPrefab.Cell[] ExpectedPlayerCells =
    {
        BMButtonPrefab.Cell.Cell4,
        BMButtonPrefab.Cell.Cell5,
        BMButtonPrefab.Cell.Cell6
    };

    private void Reset()
    {
        battleManager = GetComponent<BattleManager>();
    }

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        runtimeBindingRoutine = StartCoroutine(BindWhenCharactersAreReady());
    }

    private void OnDisable()
    {
        if (runtimeBindingRoutine == null)
        {
            return;
        }

        StopCoroutine(runtimeBindingRoutine);
        runtimeBindingRoutine = null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// L'installer usa questo metodo soltanto per salvare il riferimento.
    /// Il binding vero viene eseguito in Play Mode, quando i personaggi
    /// eventualmente istanziati da bootstrap o spawner sono disponibili.
    /// </summary>
    public void ConfigureEditor(BattleManager manager)
    {
        battleManager = manager;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private IEnumerator BindWhenCharactersAreReady()
    {
        int attempts = Mathf.Max(1, runtimeBindAttempts);

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (BindCharacters(false))
            {
                runtimeBindingRoutine = null;
                yield break;
            }

            yield return new WaitForSecondsRealtime(
                Mathf.Max(0.01f, runtimeBindRetrySeconds));
        }

        BindCharacters(true);
        runtimeBindingRoutine = null;
    }

    /// <summary>
    /// Cerca i personaggi caricati, ripara le celle mancanti e aggiorna le
    /// liste del BattleManager. I riferimenti gia' presenti nel manager vengono
    /// sempre mantenuti come candidati e una ricerca vuota non azzera le liste.
    /// </summary>
    public bool BindCharacters(bool logWarnings)
    {
        if (battleManager == null)
        {
            battleManager = GetComponent<BattleManager>();
        }

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>(true);
        }

        if (battleManager == null)
        {
            if (logWarnings && Application.isPlaying)
            {
                Debug.LogError(
                    "BattleCharacterAutoBinder: BattleManager non trovato.",
                    this);
            }

            return false;
        }

        List<Player> scenePlayers = MergeCandidates(
            battleManager.players,
            FindObjectsOfType<Player>(true));

        List<Enemy> sceneEnemies = MergeCandidates(
            battleManager.enemies,
            FindObjectsOfType<Enemy>(true));

        if (scenePlayers.Count > 0)
        {
            battleManager.players = BuildPlayerList(scenePlayers);
        }

        if (sceneEnemies.Count > 0)
        {
            battleManager.enemies = BuildEnemyList(sceneEnemies);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(battleManager);
        }
#endif

        List<Player> currentPlayers = battleManager.players ?? new List<Player>();
        List<Enemy> currentEnemies = battleManager.enemies ?? new List<Enemy>();

        bool complete =
            HasAllPlayerCells(currentPlayers) &&
            HasAllEnemyCells(currentEnemies);

        if (!complete)
        {
            if (logWarnings && Application.isPlaying && !hasLoggedIncompleteSetup)
            {
                hasLoggedIncompleteSetup = true;

                Debug.LogError(
                    "BattleCharacterAutoBinder: configurazione personaggi incompleta dopo " +
                    "l'attesa runtime. Servono Player su Cell4/Cell5/Cell6 ed Enemy su " +
                    "Cell1/Cell2/Cell3. Trovati Player=" + CountValid(currentPlayers) +
                    ", Enemy=" + CountValid(currentEnemies) + ". " +
                    "Le lane senza una coppia valida non possono applicare danno.",
                    battleManager);
            }

            return false;
        }

        hasLoggedIncompleteSetup = false;

        if (logSuccessfulBinding)
        {
            Debug.Log(
                "BattleCharacterAutoBinder: riferimenti configurati. " +
                "Player Cell4-6 ed Enemy Cell1-3 disponibili.",
                battleManager);
        }

        return true;
    }

    private static List<T> MergeCandidates<T>(
        IEnumerable<T> existing,
        IEnumerable<T> discovered)
        where T : Component
    {
        var result = new List<T>();
        var used = new HashSet<T>();

        AddCandidates(existing, result, used);
        AddCandidates(discovered, result, used);

        return result
            .Where(IsSceneComponent)
            .OrderBy(component => GetHierarchyPath(component.transform), StringComparer.Ordinal)
            .ToList();
    }

    private static void AddCandidates<T>(
        IEnumerable<T> source,
        ICollection<T> destination,
        ISet<T> used)
        where T : Component
    {
        if (source == null)
        {
            return;
        }

        foreach (T component in source)
        {
            if (component == null || used.Contains(component))
            {
                continue;
            }

            used.Add(component);
            destination.Add(component);
        }
    }

    private List<Player> BuildPlayerList(List<Player> candidates)
    {
        var result = new List<Player>(candidates.Count);
        var used = new HashSet<Player>();

        for (int i = 0; i < ExpectedPlayerCells.Length; i++)
        {
            BMButtonPrefab.Cell expectedCell = ExpectedPlayerCells[i];
            Player selected = candidates.FirstOrDefault(
                player => !used.Contains(player) && player.cell == expectedCell);

            if (selected == null)
            {
                selected = candidates.FirstOrDefault(player => !used.Contains(player));

                if (selected != null && assignMissingExpectedCells)
                {
                    selected.cell = expectedCell;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(selected);
                    }
#endif
                }
            }

            if (selected != null)
            {
                used.Add(selected);
                result.Add(selected);
            }
        }

        result.AddRange(candidates.Where(player => !used.Contains(player)));
        return result;
    }

    private List<Enemy> BuildEnemyList(List<Enemy> candidates)
    {
        var result = new List<Enemy>(candidates.Count);
        var used = new HashSet<Enemy>();

        for (int i = 0; i < ExpectedEnemyCells.Length; i++)
        {
            BMButtonPrefab.Cell expectedCell = ExpectedEnemyCells[i];
            Enemy selected = candidates.FirstOrDefault(
                enemy => !used.Contains(enemy) && enemy.cell == expectedCell);

            if (selected == null)
            {
                selected = candidates.FirstOrDefault(enemy => !used.Contains(enemy));

                if (selected != null && assignMissingExpectedCells)
                {
                    selected.cell = expectedCell;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(selected);
                    }
#endif
                }
            }

            if (selected != null)
            {
                used.Add(selected);
                result.Add(selected);
            }
        }

        result.AddRange(candidates.Where(enemy => !used.Contains(enemy)));
        return result;
    }

    private static bool HasAllPlayerCells(IEnumerable<Player> players)
    {
        return players != null && ExpectedPlayerCells.All(
            expected => players.Any(player => player != null && player.cell == expected));
    }

    private static bool HasAllEnemyCells(IEnumerable<Enemy> enemies)
    {
        return enemies != null && ExpectedEnemyCells.All(
            expected => enemies.Any(enemy => enemy != null && enemy.cell == expected));
    }

    private static int CountValid<T>(IEnumerable<T> source)
        where T : UnityEngine.Object
    {
        return source == null ? 0 : source.Count(item => item != null);
    }

    private static bool IsSceneComponent(Component component)
    {
        return component != null &&
               component.gameObject != null &&
               component.gameObject.scene.IsValid();
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        string path = transform.name;
        Transform parent = transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
