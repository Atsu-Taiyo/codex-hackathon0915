using System;
using System.IO;
using Hackathon.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hackathon.Editor
{
    /// <summary>Runs navigation verification in a disposable project copy.</summary>
    [InitializeOnLoad]
    public static class WorldMapBatchVerify
    {
        const string Key = "WorldMapBatchVerify.Active";
        const string CompanyKey = "WorldMapBatchVerify.OriginalCompany";
        static double started;
        static WorldMapBatchVerify()
        {
            if (!Application.isBatchMode) return;
            EditorApplication.playModeStateChanged += state => {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key, false)) return;
                started = EditorApplication.timeSinceStartup;
                WorldMapPlaytest.Begin();
                EditorApplication.update += Wait;
            };
        }
        public static void Run()
        {
            Directory.CreateDirectory("Temp");
            if (File.Exists("Temp/WorldMap-playtest.txt")) File.Delete("Temp/WorldMap-playtest.txt");
            SessionState.SetString(CompanyKey, PlayerSettings.companyName);
            PlayerSettings.companyName = "HackathonMapVerification";
            EditorSceneManager.OpenScene(WorldNavigation.MapScene);
            SessionState.SetBool(Key, true);
            EditorApplication.isPlaying = true;
        }
        public static void CreateMap()
        {
            if (!Application.isBatchMode) throw new InvalidOperationException("Use the normal World Map menu in the Editor.");
            EditorSceneManager.OpenScene(WorldNavigation.RoomOneScene);
            WorldMapSetup.Create();
        }
        static void Wait()
        {
            if (File.Exists("Temp/WorldMap-playtest.txt"))
            {
                string report = File.ReadAllText("Temp/WorldMap-playtest.txt");
                Directory.CreateDirectory("Verification");
                File.WriteAllText("Verification/WorldMap-playtest.txt", report);
                SessionState.SetBool(Key, false);
                PlayerSettings.companyName = SessionState.GetString(CompanyKey, "DefaultCompany");
                EditorApplication.Exit(report.Contains("checks passed") ? 0 : 1);
            }
            if (EditorApplication.timeSinceStartup - started > 75)
            {
                Debug.LogError("[WorldMap] Navigation verification timed out.");
                PlayerSettings.companyName = SessionState.GetString(CompanyKey, "DefaultCompany");
                EditorApplication.Exit(2);
            }
        }
    }
}
