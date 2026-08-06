#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SphericalBattleSceneBuilder
{
    const string SourceScene = "Assets/Scenes/Battle.unity";
    const string DestinationScene = "Assets/Scenes/Battle_SphericalLanes_Standalone.unity";

    [MenuItem("Tools/Rhythm Combat/Create Spherical Battle Scene")]
    public static void CreateScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScene) == null)
        {
            EditorUtility.DisplayDialog(
                "Spherical Battle",
                "Non trovo Assets/Scenes/Battle.unity.",
                "OK");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DestinationScene) != null)
        {
            if (!EditorUtility.DisplayDialog(
                    "Spherical Battle",
                    "La scena standalone esiste già. Vuoi rigenerarla da Battle.unity?",
                    "Rigenera",
                    "Annulla"))
            {
                return;
            }

            AssetDatabase.DeleteAsset(DestinationScene);
        }

        if (!AssetDatabase.CopyAsset(SourceScene, DestinationScene))
        {
            EditorUtility.DisplayDialog("Spherical Battle", "Copia della scena fallita.", "OK");
            return;
        }

        AssetDatabase.Refresh();
        Scene scene = EditorSceneManager.OpenScene(DestinationScene, OpenSceneMode.Single);
        SphericalBattleLayout layout = SphericalBattleLayout.ApplyToCurrentBattle();

        if (layout == null)
        {
            EditorUtility.DisplayDialog("Spherical Battle", "Layout non applicato: Panel non trovato.", "OK");
            return;
        }

        EditorUtility.SetDirty(layout);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Spherical Battle",
            "Creata: " + DestinationScene,
            "OK");
    }
}
#endif
