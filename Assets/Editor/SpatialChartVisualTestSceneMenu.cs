#if UNITY_EDITOR
using RhythmCombat.Unity;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmCombat.EditorTools
{
    public static class SpatialChartVisualTestSceneMenu
    {
        const string kScenePath = "Assets/Scenes/SpatialChartVisualTest.unity";
        const string kChartPath = "Assets/RhythmCombat/Generated/Charts/Tutorial Battaglia   Epic Metal Feels_pump-halfdouble_Beginner.asset";

        [MenuItem("RhythmCombat/Tests/Open Spatial Chart Visual Test")]
        public static void OpenScene()
        {
            if (!File.Exists(kScenePath))
                CreateScene();
            else
                EditorSceneManager.OpenScene(kScenePath, OpenSceneMode.Single);
        }

        [MenuItem("RhythmCombat/Tests/Open And Play Spatial Chart Visual Test")]
        public static void OpenAndPlay()
        {
            OpenScene();
            EditorApplication.delayCall += StartPlayMode;
        }

        [MenuItem("RhythmCombat/Tests/Create Spatial Chart Visual Test Scene")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var runnerObject = new GameObject("Spatial Chart Visual Test Runner");
            var runner = runnerObject.AddComponent<SpatialChartVisualTestRunner>();
            var chart = AssetDatabase.LoadAssetAtPath<global::ChartDataAsset>(kChartPath);

            var serializedRunner = new SerializedObject(runner);
            serializedRunner.FindProperty("m_Chart").objectReferenceValue = chart;
            serializedRunner.FindProperty("m_MaxNotesToSpawn").intValue = 48;
            serializedRunner.FindProperty("m_PlayOnStart").boolValue = true;
            serializedRunner.ApplyModifiedPropertiesWithoutUndo();

            var sceneDirectory = Path.GetDirectoryName(kScenePath);
            if (!Directory.Exists(sceneDirectory))
                Directory.CreateDirectory(sceneDirectory);

            EditorSceneManager.SaveScene(scene, kScenePath);
            EditorSceneManager.OpenScene(kScenePath, OpenSceneMode.Single);
            Selection.activeObject = runnerObject;

            Debug.Log("Spatial chart visual test scene created at " + kScenePath + ". Press Play to run it.", runnerObject);
        }

        static void StartPlayMode()
        {
            EditorApplication.delayCall -= StartPlayMode;

            if (!EditorApplication.isPlaying)
                EditorApplication.isPlaying = true;
        }
    }
}
#endif
