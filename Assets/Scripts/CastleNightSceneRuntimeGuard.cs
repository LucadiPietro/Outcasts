using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime safety net for the converted castle-night scene.
/// The editor production tool should serialize the fixes permanently; this
/// guard prevents a bad scene revision from shipping with invisible UI,
/// excessive camera depth or enabled test-only roots.
/// </summary>
public static class CastleNightSceneRuntimeGuard
{
    const string TargetSceneName =
        "Fan_Retro_Castle_Night_CONVERTED";

    const float MaximumFarClip = 500f;
    const float ZeroScaleThreshold = 0.0001f;

    static readonly string[] DebugRootNames =
    {
        "test",
        "Cutscene Test",
        "Cutscene Backup"
    };

    /// <summary>
    /// Registers the scene-loaded callback before gameplay scenes are opened.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    /// <summary>
    /// Also validates scenes that were already loaded before registration.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ValidateLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            ValidateScene(SceneManager.GetSceneAt(i));
        }
    }

    /// <summary>
    /// Validates the newly loaded scene.
    /// </summary>
    static void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadMode)
    {
        ValidateScene(scene);
    }

    /// <summary>
    /// Applies runtime-only safeguards to the castle-night scene.
    /// </summary>
    static void ValidateScene(Scene scene)
    {
        if (!scene.IsValid() ||
            !scene.isLoaded ||
            !string.Equals(
                scene.name,
                TargetSceneName,
                StringComparison.Ordinal))
        {
            return;
        }

        RepairMainCamera(scene);
        RepairActiveCanvases(scene);
        DisableDebugRoots(scene);
        ReportMissingScripts(scene);
        ReportDuplicateAudioListeners(scene);
    }

    /// <summary>
    /// Restricts depth precision to the range used by this 2D scene.
    /// </summary>
    static void RepairMainCamera(Scene scene)
    {
        Camera[] cameras =
            FindComponentsInScene<Camera>(scene, true);

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];

            if (camera == null ||
                !camera.gameObject.CompareTag("MainCamera"))
            {
                continue;
            }

            if (camera.farClipPlane > MaximumFarClip)
            {
                camera.farClipPlane = MaximumFarClip;

                Debug.LogWarning(
                    "CastleNightSceneRuntimeGuard: far clip MainCamera " +
                    "ripristinato a " +
                    MaximumFarClip +
                    ".");
            }

            return;
        }

        Debug.LogError(
            "CastleNightSceneRuntimeGuard: MainCamera non trovata.");
    }

    /// <summary>
    /// Repairs active canvases that would otherwise be completely invisible.
    /// </summary>
    static void RepairActiveCanvases(Scene scene)
    {
        Canvas[] canvases =
            FindComponentsInScene<Canvas>(scene, true);

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];

            if (canvas == null ||
                !canvas.gameObject.activeInHierarchy ||
                !HasZeroAxis(canvas.transform.localScale))
            {
                continue;
            }

            canvas.transform.localScale = Vector3.one;

            Debug.LogWarning(
                "CastleNightSceneRuntimeGuard: scala Canvas ripristinata su " +
                GetHierarchyPath(canvas.transform) +
                ".");
        }
    }

    /// <summary>
    /// Prevents known test-only roots from being enabled in a player build.
    /// </summary>
    static void DisableDebugRoots(Scene scene)
    {
        foreach (GameObject gameObject in EnumerateGameObjects(scene))
        {
            if (gameObject == null ||
                !gameObject.activeSelf ||
                Array.IndexOf(DebugRootNames, gameObject.name) < 0)
            {
                continue;
            }

            gameObject.SetActive(false);

            Debug.LogWarning(
                "CastleNightSceneRuntimeGuard: oggetto di test disattivato: " +
                GetHierarchyPath(gameObject.transform) +
                ".");
        }
    }

    /// <summary>
    /// Reports missing MonoBehaviours, which cannot be repaired at runtime.
    /// </summary>
    static void ReportMissingScripts(Scene scene)
    {
        foreach (GameObject gameObject in EnumerateGameObjects(scene))
        {
            Component[] components =
                gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                {
                    continue;
                }

                Debug.LogError(
                    "CastleNightSceneRuntimeGuard: script mancante su " +
                    GetHierarchyPath(gameObject.transform) +
                    ".");
            }
        }
    }

    /// <summary>
    /// Reports multiple enabled listeners before Unity starts spamming warnings.
    /// </summary>
    static void ReportDuplicateAudioListeners(Scene scene)
    {
        AudioListener[] listeners =
            FindComponentsInScene<AudioListener>(scene, true);

        int activeCount = 0;

        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];

            if (listener != null &&
                listener.enabled &&
                listener.gameObject.activeInHierarchy)
            {
                activeCount++;
            }
        }

        if (activeCount > 1)
        {
            Debug.LogError(
                "CastleNightSceneRuntimeGuard: trovati " +
                activeCount +
                " AudioListener attivi.");
        }
    }

    /// <summary>
    /// Finds components belonging only to the requested scene.
    /// </summary>
    static T[] FindComponentsInScene<T>(
        Scene scene,
        bool includeInactive)
        where T : Component
    {
        List<T> results = new List<T>();
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            results.AddRange(
                roots[i].GetComponentsInChildren<T>(includeInactive));
        }

        return results.ToArray();
    }

    /// <summary>
    /// Enumerates all scene objects, including inactive hierarchies.
    /// </summary>
    static IEnumerable<GameObject> EnumerateGameObjects(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms =
                roots[i].GetComponentsInChildren<Transform>(true);

            for (int j = 0; j < transforms.Length; j++)
            {
                yield return transforms[j].gameObject;
            }
        }
    }

    /// <summary>
    /// Checks whether a scale contains an effectively zero axis.
    /// </summary>
    static bool HasZeroAxis(Vector3 scale)
    {
        return Mathf.Abs(scale.x) <= ZeroScaleThreshold ||
               Mathf.Abs(scale.y) <= ZeroScaleThreshold ||
               Mathf.Abs(scale.z) <= ZeroScaleThreshold;
    }

    /// <summary>
    /// Builds a readable hierarchy path for diagnostics.
    /// </summary>
    static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "<null>";
        }

        Stack<string> names = new Stack<string>();
        Transform current = transform;

        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names.ToArray());
    }
}
