using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController
    {
        [Serializable] class VoiceRequest { public string[] cards; public string[] vocabulary; }
        [Serializable] class VoiceResponse { public string[] cards; public string text; public string error; }
        bool voiceBusy;
        string voiceStatus = "";
        UnityWebRequest voiceRequest;
        int voiceGeneration;
        Coroutine voiceRoutine;
        public bool IsVoiceBusy => voiceBusy;

        void ToggleVoice()
        {
            if (voiceBusy)
            {
                voiceGeneration++;
                voiceRequest?.Abort();
                voiceRequest = null;
                voiceBusy = false;
                voiceStatus = "Cancelled";
                return;
            }
            if (playing) return;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            voiceRoutine = StartCoroutine(ArrangeFromVoice());
#else
            voiceStatus = "Voice currently requires macOS and the local VoiceBridge.";
#endif
        }

        IEnumerator ArrangeFromVoice()
        {
            int generation = ++voiceGeneration;
            var snapshot = (string[])Cards.Clone();
            voiceBusy = true;
            voiceStatus = "Listening / arranging… Speak, then pause.";
            var data = new VoiceRequest { cards = snapshot, vocabulary = Progress.globalVocabulary.ToArray() };
            using (var request = new UnityWebRequest("http://127.0.0.1:47831/listen", "POST"))
            {
                voiceRequest = request;
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 90;
                yield return request.SendWebRequest();
                if (generation != voiceGeneration) yield break;
                voiceRequest = null;
                voiceBusy = false;
                VoiceResponse result = null;
                try { result = JsonUtility.FromJson<VoiceResponse>(request.downloadHandler.text); }
                catch (Exception) { /* Connection and malformed response handled below. */ }
                if (request.result != UnityWebRequest.Result.Success || result == null || !string.IsNullOrEmpty(result.error))
                {
                    voiceStatus = result?.error ?? "Start VoiceBridge first (Tools/VoiceBridge).";
                    yield break;
                }
                if (!TryApplyVoiceCards(result.cards, snapshot))
                {
                    voiceStatus = "Cards changed or response invalid. Please try again.";
                    yield break;
                }
                voiceStatus = "Heard: " + result.text;
                Notify("Cards arranged. Press RUN to try them.");
            }
        }

        // Apply the entire result atomically; never overwrite edits made while Codex was thinking.
        public bool TryApplyVoiceCards(string[] proposed, string[] expected)
        {
            if (IsSwahili || NeedsLanguageSelection || playing || proposed == null || proposed.Length != 3 || expected == null || expected.Length != 3) return false;
            for (int i = 0; i < 3; i++)
            {
                if (Cards[i] != expected[i]) return false;
                if (!string.IsNullOrEmpty(proposed[i]) && !Progress.globalVocabulary.Contains(proposed[i])) return false;
                for (int j = 0; j < i; j++)
                    if (!string.IsNullOrEmpty(proposed[i]) && proposed[i] == proposed[j]) return false;
            }
            for (int i = 0; i < 3; i++) Cards[i] = string.IsNullOrEmpty(proposed[i]) ? null : proposed[i];
            selected = badSlot = dragging = -1;
            draggedWord = null; hasDragged = false;
            row = frame = 0; activeMeaning = null;
            return true;
        }

        void OnDisable()
        {
            voiceGeneration++;
            voiceRequest?.Abort();
            voiceRequest = null;
            voiceBusy = false;
            if (voiceRoutine != null) StopCoroutine(voiceRoutine);
            voiceRoutine = null;
        }

        void DrawVoiceButton()
        {
            var r = new Rect(34, 128, 84, 76);
            bool enabled = !playing || voiceBusy;
            Color background = voiceBusy ? Hex("F5A76C") : White;
            if (!enabled) background = Color.Lerp(background, White, .65f);
            else if (r.Contains(pointer)) background = Color.Lerp(background, White, .25f);
            Color icon = enabled ? Ink : Muted;
            Panel(r, background, 16);
            // Draw the microphone directly so it does not depend on emoji/font support.
            Panel(new Rect(r.x + 23, r.y + 27, 38, 32), icon, 19, false);
            Panel(new Rect(r.x + 27, r.y + 23, 30, 31), background, 15, false);
            Panel(new Rect(r.x + 31, r.y + 12, 22, 37), icon, 11, false);
            Panel(new Rect(r.x + 40, r.y + 57, 4, 10), icon, 2, false);
            Panel(new Rect(r.x + 29, r.y + 65, 26, 4), icon, 2, false);
            if (voiceBusy) Panel(new Rect(r.x + 64, r.y + 10, 9, 9), Hex("D74444"), 5, false);
            GUI.Label(r, new GUIContent("", voiceBusy ? "Stop listening" : "Arrange cards with your voice"));
            if (Hit(r, enabled)) ToggleVoice();
        }

        void DrawVoiceControls()
        {
            string status = voiceStatus;
            if (status.Length > 65) status = status.Substring(0, 62) + "…";
            Label(new Rect(52, 789, 620, 35), status, 16, Muted);
        }
    }
}
