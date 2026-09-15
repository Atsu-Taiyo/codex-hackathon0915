using System;
using System.IO;
using System.Linq;
using Hackathon.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Editor
{
    [InitializeOnLoad]
    public static class WorldMapTools
    {
        const string Command = "Temp/WorldMap.command";
        static WorldMapTools() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Command)) return;
            string command = File.ReadAllText(Command).Trim(); File.Delete(Command);
            try
            {
                switch (command)
                {
                    case "create": WorldMapSetup.Create(); break;
                    case "open":
                        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
                            throw new InvalidOperationException("Stop Play mode and save the current scene before opening the map.");
                        EditorSceneManager.OpenScene(WorldNavigation.MapScene); break;
                    case "play": EditorApplication.isPlaying = true; break;
                    case "stop": EditorApplication.isPlaying = false; break;
                    case "test": WorldMapPlaytest.Begin(); break;
                    case "capture": ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/WorldMap-preview.png")); break;
                    case "status":
                        File.WriteAllText("Temp/WorldMap-status.txt", "Scene=" + SceneManager.GetActiveScene().path + "\nPlaying=" + EditorApplication.isPlaying + "\nDirty=" + SceneManager.GetActiveScene().isDirty); break;
                    case "playerbuild": BuildPlayer(); break;
                    default: throw new ArgumentException("Unknown WorldMap command: " + command);
                }
                File.WriteAllText("Temp/WorldMap-command-result.txt", command + " completed at " + DateTime.UtcNow.ToString("O"));
            }
            catch (Exception error)
            {
                File.WriteAllText("Temp/WorldMap-error.txt", error.ToString()); Debug.LogException(error);
            }
        }

        [MenuItem("Tools/World Map/Build macOS")]
        public static void BuildPlayer()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/EnglishWorld.app", target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            File.WriteAllText("Temp/WorldMap-build.txt", report.summary.result + " errors=" + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("World Map player build failed.");
        }
    }
}
