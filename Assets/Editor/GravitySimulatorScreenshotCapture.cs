using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EngineeringSimulationLab.EditorTools
{
    [InitializeOnLoad]
    public static class GravitySimulatorScreenshotCapture
    {
        private const string ScenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string OutputDirectory = @"D:\Use\Proyect\screenshots";
        private const string PendingKey = "GravityCapturePending";
        private const string ExitAfterKey = "GravityCaptureExitAfter";
        private const string OutputPathKey = "GravityCaptureOutputPath";

        private static int framesRemaining;
        private static string outputPath;

        static GravitySimulatorScreenshotCapture()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            if (SessionState.GetBool(PendingKey, false) && EditorApplication.isPlaying)
            {
                BeginFrameWait();
            }
        }

        [MenuItem("Engineering Lab/Capture Gravity Simulator Screenshot")]
        public static void CaptureFromMenu()
        {
            StartCapture(false);
        }

        public static void CaptureAndExit()
        {
            StartCapture(true);
        }

        public static void OpenAndPlay()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        private static void StartCapture(bool exitAfterCapture)
        {
            Directory.CreateDirectory(OutputDirectory);
            outputPath = Path.Combine(OutputDirectory, "codex-gravity-simulator.png");
            SessionState.SetString(OutputPathKey, outputPath);
            SessionState.SetBool(PendingKey, true);
            SessionState.SetBool(ExitAfterKey, exitAfterCapture);
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            BeginFrameWait();
        }

        private static void BeginFrameWait()
        {
            outputPath = SessionState.GetString(OutputPathKey, Path.Combine(OutputDirectory, "codex-gravity-simulator.png"));
            framesRemaining = 120;
            EditorApplication.update -= WaitThenCapture;
            EditorApplication.update += WaitThenCapture;
        }

        private static void WaitThenCapture()
        {
            framesRemaining--;
            if (framesRemaining > 0)
            {
                return;
            }

            EditorApplication.update -= WaitThenCapture;
            Texture2D capture = ScreenCapture.CaptureScreenshotAsTexture();
            if (capture == null)
            {
                Debug.LogError("GravitySimulatorScreenshotCapture: CaptureScreenshotAsTexture returned null.");
                FinishAfterCaptureFailure();
                return;
            }

            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(capture);
            Debug.Log($"GravitySimulatorScreenshotCapture: saved {outputPath}");
            SessionState.SetBool(PendingKey, false);

            bool exitAfterCapture = SessionState.GetBool(ExitAfterKey, false);
            if (exitAfterCapture)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.Exit(0);
            }
        }

        private static void FinishAfterCaptureFailure()
        {
            SessionState.SetBool(PendingKey, false);
            EditorApplication.isPlaying = false;
            if (SessionState.GetBool(ExitAfterKey, false))
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
