using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EngineeringSimulationLab.EditorTools
{
    public static class BuildLab
    {
        private const string OutputDir = @"D:\Use\Proyect\Build\GravitySimulator";
        private const string ExecutableName = "EngineeringSimulationLab.exe";
        private const string MainScene = "Assets/Scenes/MainMenuScene.unity";

        [MenuItem("Engineering Lab/Build Windows x64 (Release)")]
        public static void BuildWindowsRelease()
        {
            ConfigurePlayerSettings();
            Build(BuildOptions.None);
        }

        [MenuItem("Engineering Lab/Build Windows x64 (Development)")]
        public static void BuildWindowsDevelopment()
        {
            ConfigurePlayerSettings();
            Build(BuildOptions.Development);
        }

        public static void BuildWindowsReleaseFromCli()
        {
            ConfigurePlayerSettings();
            BuildReport report = Build(BuildOptions.None);
            if (report == null || report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "Engineering Simulation Lab — Gravity Simulator";
            PlayerSettings.companyName = "Engineering Simulation Lab";
            PlayerSettings.bundleVersion = "0.1.0";

            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true;
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.runInBackground = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            NamedBuildTarget windowsTarget = NamedBuildTarget.Standalone;
            // Use Mono2x because Windows IL2CPP requires the Unity IL2CPP module + Visual Studio C++ tools,
            // which are not guaranteed on this build environment. Switch to IL2CPP manually if those are installed.
            PlayerSettings.SetScriptingBackend(windowsTarget, ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(windowsTarget, 2);
            PlayerSettings.SetApiCompatibilityLevel(windowsTarget, ApiCompatibilityLevel.NET_Standard);
        }

        private static BuildReport Build(BuildOptions options)
        {
            Directory.CreateDirectory(OutputDir);
            string outputPath = Path.Combine(OutputDir, ExecutableName);

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"BuildLab: build succeeded → {outputPath} ({report.summary.totalSize / 1024 / 1024} MB)");
            }
            else
            {
                Debug.LogError($"BuildLab: build failed with result {report.summary.result} ({report.summary.totalErrors} errors).");
            }
            return report;
        }
    }
}
