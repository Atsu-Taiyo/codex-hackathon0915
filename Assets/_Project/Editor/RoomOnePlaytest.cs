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
        static string backup, swahiliBackup;
        static bool hadSwahiliSave;
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
            hadSwahiliSave = PlayerPrefs.HasKey(RoomOneSave.Key + ".sw");
            swahiliBackup = PlayerPrefs.GetString(RoomOneSave.Key + ".sw");
            try
            {
                var room = UnityEngine.Object.FindAnyObjectByType<RoomOneController>();
                Check(room != null, "Controller exists");
                Check(room.SelectLanguage(false), "English selected at entry");
                while (room.IsPlaying) yield return null;
                room.Progress.isCleared = false;
                room.Progress.discoveredWords.Clear(); room.Progress.discoveredCollections.Clear(); room.Progress.executionHistory.Clear();
                room.Progress.globalVocabulary.Remove("robot"); room.Progress.globalVocabulary.Remove("box");
                room.Progress.globalVocabulary.Add("robot"); room.Progress.globalVocabulary.Add("box");
                room.ResetCards();
                Check(!room.PlaceCard("robot", 0) && !room.PlaceCard("box", 2), "Noun cards stay locked before world clicks");
                room.Discover("robot"); room.Discover("box"); room.Discover("robot");
                Check(room.Progress.discoveredWords.Count == 2, "Discovery deduplicated");
                Check(RoomOneSave.Load().discoveredWords.Count == 2, "Discovery saved");
                room.ResetCards();
                Check(room.PlaceCard("robot", 0), "Noun placed from tray");
                Check(room.PlaceCard("lift", 1), "Verb placed with same API");
                Check(room.PlaceCard("box", 2), "Second noun placed");
                room.PlaceCard("robot", 2);
                Check(room.Cards[0] == "box" && room.Cards[2] == "robot", "Existing card swaps without duplication");
                room.PlaceCard("push", 1);
                Check(Array.IndexOf(room.Cards, "lift") < 0 && room.Cards[1] == "push", "Replacement returns old card to tray");
                room.RemoveCard(2);
                Check(Array.IndexOf(room.Cards, "robot") < 0, "Removal releases card");
                Check(!room.PlaceCard("unknown", 0) && !room.PlaceCard("robot", -1), "Invalid drops rejected");
                room.ResetCards();
                Check(Array.TrueForAll(room.Cards, string.IsNullOrEmpty), "Reset returns all cards");
                room.Cards[0] = "robot"; room.Cards[1] = "lift"; room.Cards[2] = "robot";
                Check(!room.RunSentence(), "Duplicate nouns cannot execute");
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
                string englishSave = PlayerPrefs.GetString(RoomOneSave.Key);
                Check(room.SelectLanguage(true), "Swahili selected");
                while (room.IsPlaying) yield return null;
                room.Progress.executionHistory.Clear(); room.Progress.discoveredCollections.Clear(); room.Progress.isCleared = false;
                Check(room.Cards[0] == "robot" && room.Cards[2] == "box", "Swahili nouns fixed initially");
                Check(!room.PlaceCard("box", 0) && !room.PlaceCard("push", 2), "Fixed noun slots reject drops");
                room.RemoveCard(0); room.SwapCards(0, 2);
                Check(room.Cards[0] == "robot" && room.Cards[2] == "box", "Fixed nouns resist removal and swapping");
                string[] expectedVerbs = { "inasukuma", "inavuta", "inainua", "inafungua", "inatikisa", "inavunja" };
                for (int i = 0; i < RoomOneRules.Actions.Length; i++)
                {
                    string action = RoomOneRules.Actions[i];
                    Check(RoomOneRules.SwahiliWord(action) == expectedVerbs[i], "Swahili label " + action);
                    Check(room.PlaceCard(action, 1) && room.RunSentence(), "Swahili action starts " + action);
                    while (room.IsPlaying) yield return null;
                    Check(room.CurrentFrame == 2, "Swahili animation completes " + action);
                }
                var swahili = RoomOneSave.Load(true);
                Check(swahili.executionHistory.Count == 6 && swahili.discoveredCollections.Count == 6 && swahili.isCleared, "Swahili progress persisted");
                Check(PlayerPrefs.GetString(RoomOneSave.Key) == englishSave, "Swahili leaves English save unchanged");
                room.ResetCards();
                Check(room.Cards[0] == "robot" && room.Cards[1] == null && room.Cards[2] == "box", "Swahili reset preserves sentence frame");
                Check(room.SelectLanguage(false), "Return to English");
                while (room.IsPlaying) yield return null;
                Check(room.Progress.executionHistory.Count == 8 && Array.TrueForAll(room.Cards, string.IsNullOrEmpty), "English progress and free builder restored");
                File.WriteAllText("Temp/RoomOne-playtest.txt", checks + " runtime checks passed; English and Swahili; all six actions completed; original save restored");
                Debug.Log("[RoomOne] " + checks + " runtime checks passed.");
            }
            finally { Restore(); }
        }
        static void Restore()
        {
            if (backup == null) return;
            if (hadSave) PlayerPrefs.SetString(RoomOneSave.Key, backup); else PlayerPrefs.DeleteKey(RoomOneSave.Key);
            if (hadSwahiliSave) PlayerPrefs.SetString(RoomOneSave.Key + ".sw", swahiliBackup); else PlayerPrefs.DeleteKey(RoomOneSave.Key + ".sw");
            PlayerPrefs.Save(); backup = swahiliBackup = null;
        }
    }
}
