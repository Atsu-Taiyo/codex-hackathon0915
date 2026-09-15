using System;
using System.IO;
using Hackathon.RoomOne;
using UnityEditor;
using UnityEngine;

namespace Hackathon.Editor
{
    [InitializeOnLoad]
    public static class RoomOneTools
    {
        const string CommandPath = "Temp/RoomOne.command";
        static RoomOneTools() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(CommandPath)) return;
            string command = File.ReadAllText(CommandPath).Trim();
            File.Delete(CommandPath);
            try
            {
                switch (command)
                {
                    case "validate": Validate(); break;
                    case "play": EditorApplication.isPlaying = true; break;
                    case "stop": EditorApplication.isPlaying = false; break;
                    case "playtest":
                        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play mode required");
                        UnityEngine.Object.FindAnyObjectByType<RoomOneController>().StartCoroutine(RoomOnePlaytest.Run()); break;
                    case "arttest":
                        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play mode required");
                        UnityEngine.Object.FindAnyObjectByType<RoomOneController>().StartCoroutine(RoomOneArtPlaytest.Run()); break;
                    case "capture": ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/RoomOne-preview.png")); break;
                    case "build": Build(); break;
                }
            }
            catch (Exception e) { Debug.LogException(e); File.WriteAllText("Temp/RoomOne-error.txt", e.ToString()); }
        }
        [MenuItem("Tools/Room One/Validate Rules")]
        public static void Validate()
        {
            int passed = 0;
            Action<bool, string> check = (ok, name) => { if (!ok) throw new Exception(name); passed++; };
            var progress = new RoomOneProgress(); progress.Normalize();
            check(progress.globalVocabulary.Count == 6, "Six verbs initially available");
            check(progress.discoveredWords.Count == 0, "Nouns initially undiscovered");
            foreach (string subject in new[] { "robot", "box" })
            foreach (string target in new[] { "robot", "box" })
            foreach (string action in RoomOneRules.Actions)
            {
                check(RoomOneRules.TryInterpret(new[] { subject, action, target }, out var m, out _), "Semantic parse");
                check(RoomOneRules.IsGoal(m) == (subject == "robot" && target == "box" && action == "lift"), "Only intended goal clears");
            }
            check(!RoomOneRules.TryInterpret(new[] { "push", "robot", "box" }, out _, out int bad) && bad == 0, "Bad word order highlights first slot");
            check(!RoomOneRules.TryInterpret(new[] { "robot", "box", "lift" }, out _, out bad) && bad == 1, "Bad middle slot");
            check(!RoomOneRules.TryInterpret(new string[] { "robot", "lift", null }, out _, out bad) && bad == 2, "Missing object");
            check(!RoomOneRules.TryInterpret(new[] { "robot", "lift", "unknown" }, out _, out _), "Unknown noun rejected");
            foreach (string action in RoomOneRules.Actions)
            {
                var m = new SentenceMeaning { subject = "robot", action = action, target = "box" };
                RoomOneRules.Complete(progress, m, true);
                RoomOneRules.Complete(progress, m, true);
            }
            check(progress.discoveredCollections.Count == 6, "Collection deduplicated");
            check(progress.executionHistory.Count == 12, "History preserves repeated experiments");
            check(progress.isCleared, "Lift clears");
            RoomOneRules.Complete(progress, new SentenceMeaning { subject = "box", action = "lift", target = "robot" }, true);
            check(progress.discoveredCollections.Count == 6 && progress.executionHistory.Count == 13, "Unusual action recorded separately");
            var fresh = new RoomOneProgress(); fresh.Normalize();
            var boxLift = new SentenceMeaning { subject = "box", action = "lift", target = "robot" };
            check(RoomOneRules.IsBoxLift(boxLift), "Reverse lift selects dedicated animation");
            RoomOneRules.Complete(fresh, boxLift, true);
            check(!fresh.isCleared && fresh.discoveredCollections.Count == 0 && fresh.executionHistory.Count == 1, "Reverse lift records history but does not clear forward goal");
            fresh = new RoomOneProgress(); fresh.Normalize();
            RoomOneRules.Complete(fresh, new SentenceMeaning { subject = "robot", action = "lift", target = "box" }, false);
            check(!fresh.isCleared && fresh.executionHistory.Count == 0, "Goal preview and replay never grant progress");
            var copy = JsonUtility.FromJson<RoomOneProgress>(JsonUtility.ToJson(progress)); copy.Normalize();
            check(copy.isCleared && copy.executionHistory.Count == 13 && copy.discoveredCollections.Count == 6, "Save round trip");
            foreach (string asset in new[] { "RoomOne_Actions_Atlas", "RoomOne_Workshop_Background", "RoomOne_Robot_Reference", "RoomOne_Robot_Cutout", "RoomOne_BoxLiftsRobot_Atlas" })
                check(Resources.Load<Texture2D>("RoomOne/" + asset) != null, "Asset available: " + asset);
            check(Resources.Load<Shader>("RoomOne/RoomOne_PixelCutout").isSupported, "Transparent sprite shader supported");
            File.WriteAllText("Temp/RoomOne-validation.txt", passed + " checks passed");
            Debug.Log("[RoomOne] " + passed + " checks passed.");
        }
        [MenuItem("Tools/Room One/Build macOS MVP")]
        public static void Build()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/_Project/Scenes/Bootstrap.unity" },
                locationPathName = "Builds/RoomOne.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            File.WriteAllText("Temp/RoomOne-build.txt", report.summary.result + " errors=" + report.summary.totalErrors);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Room One build failed");
        }
    }

    public sealed class RoomOneArtImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/RoomOne/Art/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = assetPath.Contains("Background") ? FilterMode.Bilinear : FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
