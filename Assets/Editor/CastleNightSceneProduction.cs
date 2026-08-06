#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Audits and safely productionizes the converted castle-night scene.
/// The automatic fixes intentionally avoid renaming or reparenting objects,
/// because cutscenes in this scene contain many direct serialized references.
/// </summary>
public static class CastleNightSceneProduction
{
    public const string ScenePath =
        "Assets/Scenes/GameScene/Fan_Retro_Castle_Night_CONVERTED.unity";

    public const string SceneName =
        "Fan_Retro_Castle_Night_CONVERTED";

    const float MaximumRecommendedFarClip = 500f;
    const float ZeroScaleThreshold = 0.0001f;
    const int RecommendedMaximumRootCount = 30;

    static readonly string[] RemovableDebugRootNames =
    {
        "test",
        "Cutscene Test",
        "Cutscene Backup"
    };

    static readonly string[] WallVariantNames =
    {
        "0.1_mura_sxopen_dxclose",
        "0.1_mura_sxopen_dxopen",
        "0.1_mura_sxclose_dxopen",
        "0.1_mura_sxclose_dxclose"
    };

    /// <summary>
    /// Stores the outcome of a scene audit.
    /// </summary>
    public sealed class AuditResult
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Information = new List<string>();

        public bool IsProductionReady => Errors.Count == 0;

        public string BuildReport()
        {
            StringBuilder builder = new StringBuilder(2048);

            builder.AppendLine(
                IsProductionReady
                    ? "CASTLE NIGHT: PRODUCTION AUDIT PASSED"
                    : "CASTLE NIGHT: PRODUCTION AUDIT FAILED");

            AppendSection(builder, "ERRORS", Errors);
            AppendSection(builder, "WARNINGS", Warnings);
            AppendSection(builder, "INFORMATION", Information);

            return builder.ToString();
        }

        static void AppendSection(
            StringBuilder builder,
            string title,
            IReadOnlyList<string> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(title);

            for (int i = 0; i < messages.Count; i++)
            {
                builder.Append("- ");
                builder.AppendLine(messages[i]);
            }
        }
    }

    /// <summary>
    /// Opens the target scene without altering the current scene setup.
    /// </summary>
    [MenuItem("Outcasts/Scene Production/Castle Night/Open Scene")]
    public static void OpenScene()
    {
        if (!EnsureSceneAssetExists())
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    /// <summary>
    /// Applies only fixes that do not change cutscene object identities,
    /// serialized references, hierarchy paths or gameplay coordinates.
    /// </summary>
    [MenuItem(
        "Outcasts/Scene Production/Castle Night/Apply Safe Fixes And Save")]
    public static void ApplySafeFixesAndSave()
    {
        if (!EnsureSceneAssetExists())
        {
            return;
        }

        using (SceneScope scope = SceneScope.Open(ScenePath))
        {
            Scene scene = scope.Scene;
            int changeCount = 0;

            changeCount += FixMainCamera(scene);
            changeCount += FixZeroScaleCanvases(scene);
            changeCount += DisableActiveDebugRoots(scene);
            changeCount += RemoveUnreferencedDebugRoots(scene);

            if (changeCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            AuditResult result = Audit(scene);
            LogAudit(result);

            Debug.Log(
                "CastleNightSceneProduction: applicate " +
                changeCount +
                " correzioni sicure e scena salvata.");
        }
    }

    /// <summary>
    /// Runs the complete production audit without changing the scene.
    /// </summary>
    [MenuItem("Outcasts/Scene Production/Castle Night/Validate Scene")]
    public static void ValidateScene()
    {
        if (!EnsureSceneAssetExists())
        {
            return;
        }

        using (SceneScope scope = SceneScope.Open(ScenePath))
        {
            LogAudit(Audit(scope.Scene));
        }
    }

    /// <summary>
    /// Returns a production audit for build validation and editor tooling.
    /// </summary>
    public static AuditResult AuditSceneAsset()
    {
        AuditResult result = new AuditResult();

        if (!EnsureSceneAssetExists(result))
        {
            return result;
        }

        using (SceneScope scope = SceneScope.Open(ScenePath))
        {
            return Audit(scope.Scene);
        }
    }

    // ---------------------------------------------------------------------
    // SAFE FIXES
    // ---------------------------------------------------------------------

    /// <summary>
    /// Clamps the real main camera to a depth range suitable for this 2D scene.
    /// </summary>
    static int FixMainCamera(Scene scene)
    {
        Camera mainCamera = FindMainCamera(scene);

        if (mainCamera == null ||
            mainCamera.farClipPlane <= MaximumRecommendedFarClip)
        {
            return 0;
        }

        Undo.RecordObject(
            mainCamera,
            "Clamp Castle Night Main Camera Far Clip");

        mainCamera.farClipPlane = MaximumRecommendedFarClip;
        EditorUtility.SetDirty(mainCamera);

        return 1;
    }

    /// <summary>
    /// Repairs active canvases whose scale is effectively zero.
    /// Inactive canvases may intentionally use scale for transition states.
    /// </summary>
    static int FixZeroScaleCanvases(Scene scene)
    {
        int changeCount = 0;
        Canvas[] canvases = FindComponentsInScene<Canvas>(scene, true);

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];

            if (!canvas.gameObject.activeInHierarchy ||
                !HasZeroAxis(canvas.transform.localScale))
            {
                continue;
            }

            Undo.RecordObject(
                canvas.transform,
                "Repair Castle Night Canvas Scale");

            canvas.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(canvas.transform);
            changeCount++;
        }

        return changeCount;
    }

    /// <summary>
    /// Ensures known test-only roots cannot accidentally become active in a build.
    /// </summary>
    static int DisableActiveDebugRoots(Scene scene)
    {
        int changeCount = 0;

        for (int i = 0; i < RemovableDebugRootNames.Length; i++)
        {
            GameObject[] matches =
                FindGameObjectsByName(scene, RemovableDebugRootNames[i]);

            for (int j = 0; j < matches.Length; j++)
            {
                GameObject candidate = matches[j];

                if (!candidate.activeSelf)
                {
                    continue;
                }

                Undo.RecordObject(
                    candidate,
                    "Disable Castle Night Debug Root");

                candidate.SetActive(false);
                EditorUtility.SetDirty(candidate);
                changeCount++;
            }
        }

        return changeCount;
    }

    /// <summary>
    /// Deletes inactive test roots only when no component outside their hierarchy
    /// contains a serialized reference to the root or one of its descendants.
    /// </summary>
    static int RemoveUnreferencedDebugRoots(Scene scene)
    {
        int changeCount = 0;

        for (int i = 0; i < RemovableDebugRootNames.Length; i++)
        {
            GameObject[] matches =
                FindGameObjectsByName(scene, RemovableDebugRootNames[i]);

            for (int j = 0; j < matches.Length; j++)
            {
                GameObject candidate = matches[j];

                if (candidate == null ||
                    candidate.activeSelf ||
                    IsReferencedOutsideHierarchy(scene, candidate))
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(candidate);
                changeCount++;
            }
        }

        return changeCount;
    }

    // ---------------------------------------------------------------------
    // AUDIT
    // ---------------------------------------------------------------------

    /// <summary>
    /// Performs scene-level checks that are safe in both the editor and builds.
    /// </summary>
    static AuditResult Audit(Scene scene)
    {
        AuditResult result = new AuditResult();

        if (!scene.IsValid() || !scene.isLoaded)
        {
            result.Errors.Add("La scena non è valida o non è caricata.");
            return result;
        }

        AuditMainCamera(scene, result);
        AuditCanvases(scene, result);
        AuditMissingScripts(scene, result);
        AuditAudioListeners(scene, result);
        AuditEventSystems(scene, result);
        AuditDebugObjects(scene, result);
        AuditWallVariants(scene, result);
        AuditHierarchy(scene, result);
        AuditDuplicateCutsceneTargets(scene, result);
        AuditPhysicsLayers(result);
        AuditNavigation(scene, result);
        AuditLighting(scene, result);
        AuditBuildSettings(result);

        return result;
    }

    /// <summary>
    /// Checks the main camera and its depth range.
    /// </summary>
    static void AuditMainCamera(Scene scene, AuditResult result)
    {
        Camera[] cameras = FindComponentsInScene<Camera>(scene, true);
        List<Camera> taggedMainCameras = cameras
            .Where(camera =>
                camera != null &&
                camera.gameObject.CompareTag("MainCamera"))
            .ToList();

        if (taggedMainCameras.Count == 0)
        {
            result.Errors.Add(
                "Nessuna Camera con tag MainCamera nella scena.");
            return;
        }

        if (taggedMainCameras.Count > 1)
        {
            result.Errors.Add(
                "Sono presenti " +
                taggedMainCameras.Count +
                " camere con tag MainCamera.");
        }

        Camera mainCamera = taggedMainCameras[0];

        if (!mainCamera.gameObject.activeInHierarchy ||
            !mainCamera.enabled)
        {
            result.Errors.Add("La MainCamera non è attiva e abilitata.");
        }

        if (mainCamera.farClipPlane > MaximumRecommendedFarClip)
        {
            result.Errors.Add(
                "La MainCamera ha far clip " +
                mainCamera.farClipPlane +
                "; il massimo previsto è " +
                MaximumRecommendedFarClip +
                ".");
        }

        if (mainCamera.nearClipPlane <= 0f ||
            mainCamera.nearClipPlane >= mainCamera.farClipPlane)
        {
            result.Errors.Add(
                "I clipping plane della MainCamera non sono validi.");
        }

        if (mainCamera.fieldOfView < 15f ||
            mainCamera.fieldOfView > 80f)
        {
            result.Warnings.Add(
                "FOV MainCamera fuori dall'intervallo consigliato 15-80.");
        }
    }

    /// <summary>
    /// Checks that active canvases remain visible and renderable.
    /// </summary>
    static void AuditCanvases(Scene scene, AuditResult result)
    {
        Canvas[] canvases = FindComponentsInScene<Canvas>(scene, true);

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];

            if (canvas.gameObject.activeInHierarchy &&
                HasZeroAxis(canvas.transform.localScale))
            {
                result.Errors.Add(
                    "Canvas attivo con scala zero: " +
                    GetHierarchyPath(canvas.transform) +
                    ".");
            }
        }
    }

    /// <summary>
    /// Missing scripts are build-blocking because their original behaviour
    /// cannot be reconstructed reliably at runtime.
    /// </summary>
    static void AuditMissingScripts(Scene scene, AuditResult result)
    {
        foreach (GameObject gameObject in EnumerateGameObjects(scene))
        {
            int missingCount =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    gameObject);

            if (missingCount <= 0)
            {
                continue;
            }

            result.Errors.Add(
                GetHierarchyPath(gameObject.transform) +
                " contiene " +
                missingCount +
                " script mancanti.");
        }
    }

    /// <summary>
    /// Ensures that only one enabled audio listener can receive audio.
    /// </summary>
    static void AuditAudioListeners(Scene scene, AuditResult result)
    {
        AudioListener[] listeners =
            FindComponentsInScene<AudioListener>(scene, true);

        int activeCount = listeners.Count(listener =>
            listener != null &&
            listener.enabled &&
            listener.gameObject.activeInHierarchy);

        if (activeCount == 0)
        {
            result.Warnings.Add(
                "Nessun AudioListener attivo nella scena.");
        }
        else if (activeCount > 1)
        {
            result.Errors.Add(
                "Sono presenti " +
                activeCount +
                " AudioListener attivi.");
        }
    }

    /// <summary>
    /// Multiple active EventSystems can duplicate UI input.
    /// </summary>
    static void AuditEventSystems(Scene scene, AuditResult result)
    {
        EventSystem[] systems =
            FindComponentsInScene<EventSystem>(scene, true);

        int activeCount = systems.Count(system =>
            system != null &&
            system.enabled &&
            system.gameObject.activeInHierarchy);

        if (activeCount > 1)
        {
            result.Errors.Add(
                "Sono presenti " +
                activeCount +
                " EventSystem attivi.");
        }
    }

    /// <summary>
    /// Active test roots must never ship in a production scene.
    /// </summary>
    static void AuditDebugObjects(Scene scene, AuditResult result)
    {
        for (int i = 0; i < RemovableDebugRootNames.Length; i++)
        {
            GameObject[] matches =
                FindGameObjectsByName(scene, RemovableDebugRootNames[i]);

            for (int j = 0; j < matches.Length; j++)
            {
                GameObject candidate = matches[j];

                if (candidate.activeSelf)
                {
                    result.Errors.Add(
                        "Oggetto di test attivo: " +
                        GetHierarchyPath(candidate.transform) +
                        ".");
                }
                else
                {
                    result.Warnings.Add(
                        "Oggetto di test ancora serializzato: " +
                        GetHierarchyPath(candidate.transform) +
                        ".");
                }
            }
        }
    }

    /// <summary>
    /// Exactly one castle-wall state must be enabled at scene start.
    /// </summary>
    static void AuditWallVariants(Scene scene, AuditResult result)
    {
        List<GameObject> variants = new List<GameObject>();

        for (int i = 0; i < WallVariantNames.Length; i++)
        {
            variants.AddRange(
                FindGameObjectsByName(scene, WallVariantNames[i]));
        }

        int activeCount =
            variants.Count(variant => variant.activeSelf);

        if (variants.Count == 0)
        {
            result.Warnings.Add(
                "Nessuna variante iniziale delle mura trovata.");
        }
        else if (activeCount != 1)
        {
            result.Errors.Add(
                "Dev'essere attiva una sola variante delle mura; attive: " +
                activeCount +
                ".");
        }
    }

    /// <summary>
    /// Reports hierarchy growth without reparenting referenced scene objects.
    /// </summary>
    static void AuditHierarchy(Scene scene, AuditResult result)
    {
        int rootCount = scene.GetRootGameObjects().Length;

        result.Information.Add(
            "Root GameObject nella scena: " + rootCount + ".");

        if (rootCount > RecommendedMaximumRootCount)
        {
            result.Warnings.Add(
                "La scena contiene " +
                rootCount +
                " root GameObject; obiettivo consigliato: massimo " +
                RecommendedMaximumRootCount +
                ".");
        }
    }

    /// <summary>
    /// Duplicate target names are legal but make cutscene maintenance fragile.
    /// They remain warnings because serialized references do not depend on names.
    /// </summary>
    static void AuditDuplicateCutsceneTargets(
        Scene scene,
        AuditResult result)
    {
        string[] duplicateNames = EnumerateGameObjects(scene)
            .Where(gameObject =>
                gameObject.name.EndsWith(
                    " Target",
                    StringComparison.Ordinal))
            .GroupBy(gameObject => gameObject.name)
            .Where(group => group.Count() > 1)
            .Select(group =>
                group.Key + " x" + group.Count())
            .OrderBy(value => value)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            result.Warnings.Add(
                "Target cutscene con nomi duplicati: " +
                string.Join(", ", duplicateNames) +
                ".");
        }
    }

    /// <summary>
    /// Checks project-level collision settings used by the scene.
    /// </summary>
    static void AuditPhysicsLayers(AuditResult result)
    {
        int obstaclesLayer = LayerMask.NameToLayer("Obstacles");

        if (obstaclesLayer < 0)
        {
            result.Errors.Add("Il layer Obstacles non esiste.");
            return;
        }

        if (!Physics2D.GetIgnoreLayerCollision(
                obstaclesLayer,
                obstaclesLayer))
        {
            result.Warnings.Add(
                "Obstacles collide con Obstacles nella matrice Physics2D.");
        }
    }

    /// <summary>
    /// Detects whether A* navigation is present locally or expected externally.
    /// </summary>
    static void AuditNavigation(Scene scene, AuditResult result)
    {
        Component[] components =
            FindComponentsInScene<Component>(scene, true);

        bool hasAstarPath = components.Any(component =>
            component != null &&
            string.Equals(
                component.GetType().FullName,
                "Pathfinding.AstarPath",
                StringComparison.Ordinal));

        bool hasAstarAgents = components.Any(component =>
            component != null &&
            component.GetType().Namespace == "Pathfinding");

        if (hasAstarAgents && !hasAstarPath)
        {
            result.Warnings.Add(
                "Sono presenti componenti Pathfinding ma nessun AstarPath " +
                "nella scena; verificare che venga caricato da una scena manager.");
        }
    }

    /// <summary>
    /// Reports the current lightmap state without forcing a rendering workflow.
    /// </summary>
    static void AuditLighting(Scene scene, AuditResult result)
    {
        Light[] lights = FindComponentsInScene<Light>(scene, true);
        int lightmapCount =
            LightmapSettings.lightmaps != null
                ? LightmapSettings.lightmaps.Length
                : 0;

        result.Information.Add(
            "Luci serializzate: " +
            lights.Length +
            "; lightmap caricate: " +
            lightmapCount +
            ".");

        if (lights.Length == 0 && lightmapCount > 0)
        {
            result.Warnings.Add(
                "Sono caricate lightmap ma non risultano luci nella scena.");
        }
    }

    /// <summary>
    /// Reports whether the scene is included in the player build.
    /// </summary>
    static void AuditBuildSettings(AuditResult result)
    {
        bool isEnabledInBuild = EditorBuildSettings.scenes.Any(scene =>
            scene.enabled &&
            string.Equals(
                scene.path,
                ScenePath,
                StringComparison.Ordinal));

        if (!isEnabledInBuild)
        {
            result.Warnings.Add(
                "La scena non è abilitata nei Build Settings; " +
                "ignorare solo se viene caricata tramite Addressables o bundle.");
        }
    }

    // ---------------------------------------------------------------------
    // REFERENCE SAFETY
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns true when an object outside the candidate hierarchy stores a
    /// serialized reference to the candidate or one of its descendants.
    /// </summary>
    static bool IsReferencedOutsideHierarchy(
        Scene scene,
        GameObject candidateRoot)
    {
        HashSet<Object> candidateObjects =
            BuildHierarchyObjectSet(candidateRoot);

        foreach (GameObject gameObject in EnumerateGameObjects(scene))
        {
            if (gameObject == null ||
                gameObject.transform.IsChildOf(candidateRoot.transform))
            {
                continue;
            }

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];

                if (component == null)
                {
                    continue;
                }

                try
                {
                    SerializedObject serializedObject =
                        new SerializedObject(component);

                    SerializedProperty property =
                        serializedObject.GetIterator();

                    while (property.Next(true))
                    {
                        if (property.propertyType !=
                            SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        Object referencedObject =
                            property.objectReferenceValue;

                        if (referencedObject != null &&
                            candidateObjects.Contains(referencedObject))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        "CastleNightSceneProduction: impossibile analizzare " +
                        component.GetType().FullName +
                        " su " +
                        GetHierarchyPath(gameObject.transform) +
                        ". " +
                        exception.Message);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Builds the complete set of Unity objects represented by a hierarchy.
    /// </summary>
    static HashSet<Object> BuildHierarchyObjectSet(GameObject root)
    {
        HashSet<Object> objects = new HashSet<Object>();
        Transform[] transforms =
            root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject gameObject = transforms[i].gameObject;
            objects.Add(gameObject);

            Component[] components = gameObject.GetComponents<Component>();

            for (int j = 0; j < components.Length; j++)
            {
                if (components[j] != null)
                {
                    objects.Add(components[j]);
                }
            }
        }

        return objects;
    }

    // ---------------------------------------------------------------------
    // SCENE HELPERS
    // ---------------------------------------------------------------------

    /// <summary>
    /// Finds the first tagged main camera belonging to the target scene.
    /// </summary>
    static Camera FindMainCamera(Scene scene)
    {
        Camera[] cameras = FindComponentsInScene<Camera>(scene, true);

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].gameObject.CompareTag("MainCamera"))
            {
                return cameras[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Finds all components of a type inside one specific scene.
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
    /// Enumerates every GameObject in the scene, including inactive objects.
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
    /// Finds scene objects by exact name.
    /// </summary>
    static GameObject[] FindGameObjectsByName(
        Scene scene,
        string objectName)
    {
        return EnumerateGameObjects(scene)
            .Where(gameObject =>
                string.Equals(
                    gameObject.name,
                    objectName,
                    StringComparison.Ordinal))
            .ToArray();
    }

    /// <summary>
    /// Returns a stable human-readable hierarchy path.
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

    /// <summary>
    /// Checks whether at least one scale axis is effectively zero.
    /// </summary>
    static bool HasZeroAxis(Vector3 scale)
    {
        return Mathf.Abs(scale.x) <= ZeroScaleThreshold ||
               Mathf.Abs(scale.y) <= ZeroScaleThreshold ||
               Mathf.Abs(scale.z) <= ZeroScaleThreshold;
    }

    /// <summary>
    /// Confirms that the target scene exists before opening it.
    /// </summary>
    static bool EnsureSceneAssetExists()
    {
        return EnsureSceneAssetExists(null);
    }

    /// <summary>
    /// Confirms that the target scene exists and optionally records an error.
    /// </summary>
    static bool EnsureSceneAssetExists(AuditResult result)
    {
        bool exists =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;

        if (exists)
        {
            return true;
        }

        const string message =
            "CastleNightSceneProduction: scena non trovata in " +
            ScenePath +
            ".";

        if (result != null)
        {
            result.Errors.Add(message);
        }

        Debug.LogError(message);
        return false;
    }

    /// <summary>
    /// Logs the audit with a severity matching the result.
    /// </summary>
    static void LogAudit(AuditResult result)
    {
        string report = result.BuildReport();

        if (result.Errors.Count > 0)
        {
            Debug.LogError(report);
        }
        else if (result.Warnings.Count > 0)
        {
            Debug.LogWarning(report);
        }
        else
        {
            Debug.Log(report);
        }
    }

    /// <summary>
    /// Opens a scene additively when needed and restores the previous setup
    /// by closing only the temporary scene after the operation.
    /// </summary>
    sealed class SceneScope : IDisposable
    {
        readonly bool closeWhenDisposed;

        public Scene Scene { get; }

        SceneScope(Scene scene, bool closeWhenDisposed)
        {
            Scene = scene;
            this.closeWhenDisposed = closeWhenDisposed;
        }

        public static SceneScope Open(string scenePath)
        {
            Scene loadedScene =
                SceneManager.GetSceneByPath(scenePath);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                return new SceneScope(loadedScene, false);
            }

            Scene openedScene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Additive);

            return new SceneScope(openedScene, true);
        }

        public void Dispose()
        {
            if (closeWhenDisposed &&
                Scene.IsValid() &&
                Scene.isLoaded)
            {
                EditorSceneManager.CloseScene(Scene, true);
            }
        }
    }
}

/// <summary>
/// Blocks player builds when the castle-night scene contains critical errors.
/// Warnings remain visible in the Console without preventing development builds.
/// </summary>
public sealed class CastleNightProductionBuildValidator :
    IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    /// <summary>
    /// Runs before every build and fails only on production-blocking errors.
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        CastleNightSceneProduction.AuditResult audit =
            CastleNightSceneProduction.AuditSceneAsset();

        if (audit.Errors.Count == 0)
        {
            if (audit.Warnings.Count > 0)
            {
                Debug.LogWarning(audit.BuildReport());
            }

            return;
        }

        throw new BuildFailedException(audit.BuildReport());
    }
}
#endif
