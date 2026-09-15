using System;
using System.Collections;
using System.IO;
using System.Text;
using Hackathon.RoomOne;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Hackathon.Editor
{
    [InitializeOnLoad]
    public static class RoomOneVoicePlaytest
    {
        [Serializable] class Reply { public string[] cards; public string text; public string error; }
        static RoomOneVoicePlaytest() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || !File.Exists("Temp/RoomOneVoice.command")) return;
            File.Delete("Temp/RoomOneVoice.command");
            RunTest();
        }
        [MenuItem("Tools/Room One/Validate Voice In Play Mode")]
        public static void RunTest()
        {
            var room = UnityEngine.Object.FindAnyObjectByType<RoomOneController>();
            if (!EditorApplication.isPlaying || room == null) throw new Exception("Play mode required");
            room.StartCoroutine(Run(room));
        }
        static IEnumerator Run(RoomOneController room)
        {
            while (room.IsPlaying) yield return null;
            var original = (string[])room.Cards.Clone();
            var vocabulary = room.Progress.globalVocabulary.ToArray();
            int checks = 0;
            Action<bool, string> check = (ok, label) => {
                if (!ok) { File.WriteAllText("Temp/RoomOneVoice-result.txt", "FAILED: " + label); throw new Exception(label); }
                checks++;
            };
            try
            {
                foreach (var word in new[] { "robot", "box", "lift" })
                    if (!room.Progress.globalVocabulary.Contains(word)) room.Progress.globalVocabulary.Add(word);
                room.Cards[0] = "robot"; room.Cards[1] = "lift"; room.Cards[2] = "box";
                var before = (string[])room.Cards.Clone();
                check(!room.TryApplyVoiceCards(new[] { "robot", "lift", "robot" }, before), "Duplicates rejected");
                check(!room.TryApplyVoiceCards(new[] { "unknown", "lift", "robot" }, before), "Unavailable words rejected");
                check(!room.TryApplyVoiceCards(new[] { "box", "lift", "robot" }, new string[3]), "Stale snapshot rejected");
                check(!room.TryApplyVoiceCards(new[] { "box" }, before), "Malformed result rejected");
                using (var request = new UnityWebRequest("http://127.0.0.1:47831/arrange", "POST"))
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{\"cards\":[\"robot\",\"lift\",\"box\"],\"vocabulary\":[\"robot\",\"box\",\"lift\"],\"text\":\"ロボットと箱を入れ替えて\"}"));
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json"); request.timeout = 80;
                    yield return request.SendWebRequest();
                    check(request.result == UnityWebRequest.Result.Success, "Real Codex request: " + request.downloadHandler.text);
                    var reply = JsonUtility.FromJson<Reply>(request.downloadHandler.text);
                    check(room.TryApplyVoiceCards(reply.cards, before), "Codex order applies");
                    check(room.Cards[0] == "box" && room.Cards[1] == "lift" && room.Cards[2] == "robot", "Spoken swap interpreted");
                    check(!room.IsPlaying, "Voice does not auto-run");
                }
                File.WriteAllText("Temp/RoomOneVoice-result.txt", "PASS: " + checks + " checks, including Unity -> codex-component -> app-server -> card update");
            }
            finally
            {
                Array.Copy(original, room.Cards, 3);
                room.Progress.globalVocabulary.Clear(); room.Progress.globalVocabulary.AddRange(vocabulary);
            }
        }
    }
}
