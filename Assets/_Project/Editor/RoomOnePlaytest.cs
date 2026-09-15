using System;
using System.Collections;
using System.IO;
using Hackathon.RoomOne;
using UnityEngine;

namespace Hackathon.Editor
{
    public static class RoomOnePlaytest
    {
        static bool hadSave;
        static string backup;
        static int checks;
        static RoomOnePlaytest()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode) Restore();
            };
        }
        static void Check(bool ok, string name)
        {
            if (!ok) { File.WriteAllText("Temp/RoomOne-playtest.txt", "FAILED: " + name); throw new Exception(name); }
            checks++;
        }
        public static IEnumerator Run()
        {
            checks = 0;
            hadSave = PlayerPrefs.HasKey(RoomOneSave.Key);
            backup = PlayerPrefs.GetString(RoomOneSave.Key);
            try
            {
                var room = UnityEngine.Object.FindAnyObjectByType<RoomOneController>();
                Check(room != null, "Controller exists");
                while (room.IsPlaying) yield return null;
                room.Progress.isCleared = false;
                room.Progress.discoveredWords.Clear(); room.Progress.discoveredCollections.Clear(); room.Progress.executionHistory.Clear();
                room.Discover("robot"); room.Discover("box"); room.Discover("robot");
                Check(room.Progress.discoveredWords.Count == 2, "Discovery deduplicated");
                Check(RoomOneSave.Load().discoveredWords.Count == 2, "Discovery saved");
                room.Cards[0] = "push"; room.Cards[1] = "robot"; room.Cards[2] = "box";
                Check(!room.RunSentence(), "Bad grammar blocked");
                room.SwapCards(0, 1);
                Check(room.Cards[0] == "robot" && room.Cards[1] == "push", "Reordering repairs word order");
                room.SwapCards(0, 2);
                Check(room.Cards[0] == "box" && room.Cards[2] == "robot", "Reordering changes subject and object");
                room.Cards[0] = "box"; room.Cards[1] = "lift"; room.Cards[2] = "robot";
                Check(room.RunSentence(), "Box lifts robot executes");
                while (room.IsPlaying) yield return null;
                Check(room.UsesBoxLiftAnimation && room.CurrentFrame == 2 && !room.Progress.isCleared, "Dedicated reverse lift finishes without clearing forward goal");
                Check(RoomOneSave.Load().executionHistory[0].Key == "box:lift:robot", "Reverse lift saved for replay");
                foreach (string action in new[] { "push", "pull", "open", "shake", "break", "lift" })
                {
                    room.Cards[0] = "robot"; room.Cards[1] = action; room.Cards[2] = "box";
                    Check(room.RunSentence(), "Action starts: " + action);
                    Check(!room.RunSentence(), "Concurrent execution blocked");
                    while (room.IsPlaying) yield return null;
                    Check(room.CurrentFrame == 2, "Three frames completed: " + action);
                    Check(room.Progress.isCleared == (action == "lift"), "Correct clear state: " + action);
                }
                Check(room.Modal == "clear", "First clear modal displayed");
                Check(room.Progress.discoveredCollections.Count == 6, "Six scenes collected");
                room.Cards[0] = "box"; room.Cards[1] = "push"; room.Cards[2] = "robot";
                Check(room.RunSentence(), "Unusual sentence supported");
                while (room.IsPlaying) yield return null;
                Check(room.Progress.executionHistory.Count == 8, "Unusual history recorded");
                Check(room.Progress.discoveredCollections.Count == 6, "Unusual sentence does not inflate collections");
                room.Replay(new SentenceMeaning { subject = "robot", action = "break", target = "box" });
                while (room.IsPlaying) yield return null;
                Check(room.Progress.executionHistory.Count == 8, "Replay does not duplicate history");
                var loaded = RoomOneSave.Load();
                Check(loaded.isCleared && loaded.executionHistory.Count == 8 && loaded.discoveredCollections.Count == 6, "Full progress persistence");
                File.WriteAllText("Temp/RoomOne-playtest.txt", checks + " runtime checks passed; all six actions completed; original save restored");
                Debug.Log("[RoomOne] " + checks + " runtime checks passed.");
            }
            finally { Restore(); }
        }
        static void Restore()
        {
            if (backup == null) return;
            if (hadSave) PlayerPrefs.SetString(RoomOneSave.Key, backup); else PlayerPrefs.DeleteKey(RoomOneSave.Key);
            PlayerPrefs.Save(); backup = null;
        }
    }
}
