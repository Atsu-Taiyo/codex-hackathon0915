using System;
using System.Collections;
using UnityEngine;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController : MonoBehaviour
    {
        public RoomOneProgress Progress { get; private set; }
        public readonly string[] Cards = new string[3];
        public bool IsPlaying => playing;
        public int CurrentFrame => frame;
        public string Modal => modal;
        public bool UsesBoxLiftAnimation => RoomOneRules.IsBoxLift(activeMeaning);
        Texture2D atlas, backdrop, reference, boxLiftAtlas;
        Material pixelCutout;
        bool playing, goalPreview;
        int row, frame, selected = -1, badSlot = -1, dragging = -1;
        Vector2 dragStart, scroll;
        bool hasDragged;
        string modal = "", notice = "", spotlight = "";
        float noticeUntil, spotlightUntil;
        SentenceMeaning activeMeaning;
        Coroutine playbackRoutine;
        AudioSource audioSource;
        AudioClip chime;

        void Awake()
        {
            Application.runInBackground = true;
            if (!Application.isEditor && Application.platform != RuntimePlatform.WebGLPlayer)
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            Progress = RoomOneSave.Load();
            atlas = Resources.Load<Texture2D>("RoomOne/RoomOne_Actions_Atlas");
            backdrop = Resources.Load<Texture2D>("RoomOne/RoomOne_Workshop_Background");
            reference = Resources.Load<Texture2D>("RoomOne/RoomOne_Robot_Cutout");
            boxLiftAtlas = Resources.Load<Texture2D>("RoomOne/RoomOne_BoxLiftsRobot_Atlas");
            var shader = Resources.Load<Shader>("RoomOne/RoomOne_PixelCutout");
            if (shader) pixelCutout = new Material(shader);
            if (!atlas || !backdrop || !reference || !boxLiftAtlas || !pixelCutout) Debug.LogError("[RoomOne] Missing artwork.");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            const int rate = 22050;
            var samples = new float[rate / 5];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 660 / rate) * .12f * (1f - (float)i / samples.Length);
            chime = AudioClip.Create("Discovery", samples.Length, 1, rate, false);
            chime.SetData(samples, 0);
        }
        void Start() { WatchGoal(); }
        public void Discover(string noun)
        {
            if (playing || !RoomOneRules.IsNoun(noun)) return;
            if (!Progress.discoveredWords.Contains(noun))
            {
                Progress.discoveredWords.Add(noun);
                Progress.Normalize();
                RoomOneSave.Store(Progress);
                audioSource.PlayOneShot(chime);
            }
            row = frame = 0;
            activeMeaning = null;
            spotlight = noun;
            spotlightUntil = Time.unscaledTime + 1.4f;
            Notify(noun + "  ·  added to your words");
        }
        void Choose(string word)
        {
            if (playing) return;
            int slot = selected;
            if (slot < 0)
            {
                if (!RoomOneRules.IsNoun(word)) slot = 1;
                else slot = string.IsNullOrEmpty(Cards[0]) ? 0 : 2;
            }
            Cards[slot] = word;
            selected = -1;
            badSlot = -1;
        }
        public bool RunSentence()
        {
            if (playing) return false;
            if (!RoomOneRules.TryInterpret(Cards, out var meaning, out badSlot))
            {
                Notify("Something is missing here. Try moving a card.");
                return false;
            }
            BeginAnimation(meaning, true, false);
            return true;
        }
        public void SwapCards(int from, int to)
        {
            if (playing || from < 0 || from > 2 || to < 0 || to > 2) return;
            (Cards[from], Cards[to]) = (Cards[to], Cards[from]);
            selected = to;
            badSlot = -1;
        }
        public void WatchGoal()
        {
            if (playing) return;
            BeginAnimation(new SentenceMeaning { subject = "robot", action = "lift", target = "box" }, false, true);
        }
        public void Replay(SentenceMeaning meaning)
        {
            if (playing) return;
            modal = "";
            BeginAnimation(meaning, false, false);
        }
        void BeginAnimation(SentenceMeaning meaning, bool record, bool preview)
        {
            if (playbackRoutine != null) StopCoroutine(playbackRoutine);
            playbackRoutine = StartCoroutine(Animate(meaning, record, preview));
        }
        IEnumerator Animate(SentenceMeaning meaning, bool record, bool preview)
        {
            playing = true;
            goalPreview = preview;
            activeMeaning = null;
            row = frame = 0;
            yield return new WaitForSecondsRealtime(.25f);
            activeMeaning = meaning;
            row = Array.IndexOf(RoomOneRules.Actions, meaning.action);
            for (int i = 0; i < 3; i++)
            {
                frame = i;
                yield return new WaitForSecondsRealtime(.7f);
            }
            bool firstClear = !Progress.isCleared && record && RoomOneRules.IsGoal(meaning);
            RoomOneRules.Complete(Progress, meaning, record);
            if (record)
            {
                RoomOneSave.Store(Progress);
                Notify(RoomOneRules.IsGoal(meaning) ? "Goal discovered!" : "Experiment saved. What will you try next?");
            }
            playing = false;
            goalPreview = false;
            if (preview) { row = frame = 0; activeMeaning = null; }
            if (firstClear) { modal = "clear"; audioSource.PlayOneShot(chime); }
            playbackRoutine = null;
        }
        void Notify(string text) { notice = text; noticeUntil = Time.unscaledTime + 3.5f; }
        void OpenModal(string value) { if (!playing) { modal = value; scroll = Vector2.zero; } }
        void OnDestroy()
        {
            if (chime) Destroy(chime);
            if (pixelCutout) Destroy(pixelCutout);
        }
    }
}
