using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BattleSceneTestMenu
{
    const string BattleScenePath = "Assets/Scenes/Battle.unity";

    [MenuItem("RhythmCombat/Tests/Open Battle Scene")]
    public static void OpenBattleScene()
    {
        EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
        Debug.Log("BattleSceneTestMenu: scena Battle aperta.");
    }

    [MenuItem("RhythmCombat/Tests/Open And Play Battle Scene")]
    public static void OpenAndPlayBattleScene()
    {
        OpenBattleScene();

        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
                Debug.Log("BattleSceneTestMenu: Play Mode avviata sulla scena Battle.");
            }
        };
    }
}
