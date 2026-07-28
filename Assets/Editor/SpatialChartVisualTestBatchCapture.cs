#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RhythmCombat.EditorTools
{
    public static class SpatialChartVisualTestBatchCapture
    {
        const string kOutputDirectory = "Logs/SpatialChartVisualTestFrames";
        static readonly double[] s_CaptureTimes = { 0.5d, 2.5d, 3.85d, 4.7d };

        static int s_CaptureIndex;
        static double s_PlayStartTime;
        static bool s_QuitAfterCapture;

        [MenuItem("RhythmCombat/Tests/Capture Spatial Chart Visual Test Frames")]
        public static void CaptureFrames()
        {
            StartCapture(false);
        }

        public static void CaptureFramesAndQuit()
        {
            StartCapture(true);
        }

        static void StartCapture(bool quitAfterCapture)
        {
            SpatialChartVisualTestSceneMenu.OpenScene();

            Directory.CreateDirectory(kOutputDirectory);
            s_CaptureIndex = 0;
            s_PlayStartTime = 0d;
            s_QuitAfterCapture = quitAfterCapture;

            EditorApplication.update -= CaptureUpdate;
            EditorApplication.update += CaptureUpdate;
            EditorApplication.delayCall += StartPlayMode;
        }

        static void StartPlayMode()
        {
            EditorApplication.delayCall -= StartPlayMode;

            if (!EditorApplication.isPlaying)
                EditorApplication.isPlaying = true;
        }

        static void CaptureUpdate()
        {
            if (!EditorApplication.isPlaying)
                return;

            if (s_PlayStartTime <= 0d)
            {
                s_PlayStartTime = EditorApplication.timeSinceStartup;
                return;
            }

            var elapsedSeconds = EditorApplication.timeSinceStartup - s_PlayStartTime;

            if (s_CaptureIndex < s_CaptureTimes.Length && elapsedSeconds >= s_CaptureTimes[s_CaptureIndex])
            {
                var fileName = "spatial_chart_t" + s_CaptureTimes[s_CaptureIndex].ToString("0.00").Replace(",", ".") + ".png";
                var path = Path.Combine(kOutputDirectory, fileName);
                CaptureCamera(path);
                s_CaptureIndex++;
            }

            if (s_CaptureIndex >= s_CaptureTimes.Length && elapsedSeconds >= s_CaptureTimes[s_CaptureTimes.Length - 1] + 0.5d)
                FinishCapture();
        }

        static void FinishCapture()
        {
            EditorApplication.update -= CaptureUpdate;

            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;

            AssetDatabase.Refresh();
            Debug.Log("Spatial chart visual capture completed in " + kOutputDirectory + ".");

            if (s_QuitAfterCapture)
                EditorApplication.delayCall += QuitEditor;
        }

        static void QuitEditor()
        {
            EditorApplication.delayCall -= QuitEditor;
            EditorApplication.Exit(0);
        }

        static void CaptureCamera(string relativePath)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("Cannot capture spatial chart visual test: no Main Camera found.");
                return;
            }

            const int width = 1280;
            const int height = 720;

            var previousTargetTexture = camera.targetTexture;
            var previousActiveTexture = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);

            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();

            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            texture.Apply();

            File.WriteAllBytes(relativePath, texture.EncodeToPNG());
            Debug.Log("Captured spatial chart visual test frame: " + relativePath);

            camera.targetTexture = previousTargetTexture;
            RenderTexture.active = previousActiveTexture;

            Object.DestroyImmediate(texture);
            renderTexture.Release();
            Object.DestroyImmediate(renderTexture);
        }
    }
}
#endif
