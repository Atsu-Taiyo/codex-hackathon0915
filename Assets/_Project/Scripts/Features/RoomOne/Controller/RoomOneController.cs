using System;
using System.Collections;
using UnityEngine;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController : MonoBehaviour
    {
        public bool IsSwahili { get; private set; }
        public bool NeedsLanguageSelection { get; private set; } = true;
        string DisplayWord(string word) => IsSwahili ? RoomOneRules.SwahiliWord(word) : RoomOneRules.DisplayWord(word);
        string DisplaySentence(SentenceMeaning meaning) => IsSwahili
            ? "Roboti " + RoomOneRules.SwahiliWord(meaning.action) + " sanduku."
            : meaning.Display;
        public bool SelectLanguage(bool swahili)
        {
            if (playing || voiceBusy) return false;
            IsSwahili = swahili;
            NeedsLanguageSelection = false;
            Progress = RoomOneSave.Load(swahili);
            if (swahili)
            {
                foreach (string noun in new[] { "robot", "box" })
                    if (!Progress.discoveredWords.Contains(noun)) Progress.discoveredWords.Add(noun);
                Progress.Normalize();
            }
            modal = "";
            ResetCards();
            WatchGoal();
            return true;
        }
        public RoomOneProgress Progress { get; private set; }
        public readonly string[] Cards = new string[3];
        public bool IsPlaying => playing;
        public int CurrentFrame => frame;
        public string Modal => modal;
        public bool UsesBoxLiftAnimation => RoomOneRules.IsBoxLift(activeMeaning);
        RoomOneArtwork artwork;
        bool playing, goalPreview;
        int row, frame, selected = -1, badSlot = -1, dragging = -1;
        Vector2 dragStart, scroll;
        bool hasDragged;
        string draggedWord;
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
            artwork = Resources.Load<RoomOneArtwork>("RoomOne/CloudRobotArtwork");
            if (!artwork || !artwork.IsComplete) Debug.LogError("[RoomOne] CloudRobot artwork is incomplete. Run Tools > Room One > Connect CloudRobot Artwork.");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            const int rate = 22050;
            var samples = new float[rate / 5];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = Mathf.Sin(i * 2 * Mathf.PI * 660 / rate) * .12f * (1f - (float)i / samples.Length);
            chime = AudioClip.Create("Discovery", samples.Length, 1, rate, false);
            chime.SetData(samples, 0);
        }
        void Start() { modal = "language"; }
        public void Discover(string noun)
        {
            if (playing || !RoomOneRules.IsNoun(noun)) return;
            if (!Progress.discoveredWords.Contains(noun))
            {
                Progress.discoveredWords.Add(noun);
                Progress.Normalize();
                RoomOneSave.Store(Progress, IsSwahili);
                audioSource.PlayOneShot(chime);
            }
            row = frame = 0;
            activeMeaning = null;
            spotlight = noun;
            spotlightUntil = Time.unscaledTime + 1.4f;
            Notify(DisplayWord(noun) + "  ·  added to your words");
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
            PlaceCard(word, slot);
            selected = -1;
            badSlot = -1;
        }
        public bool PlaceCard(string word, int slot)
        {
            if (NeedsLanguageSelection || (IsSwahili && (slot != 1 || RoomOneRules.IsNoun(word))) || playing || slot < 0 || slot >= 3 || !Progress.globalVocabulary.Contains(word)) return false;
            int source = Array.IndexOf(Cards, word);
            if (source >= 0) SwapCards(source, slot);
            else Cards[slot] = word;
            badSlot = -1;
            return true;
        }
        public void RemoveCard(int slot)
        {
            if (!playing && (!IsSwahili || slot == 1) && slot >= 0 && slot < 3) Cards[slot] = null;
        }
        public void ResetCards()
        {
            if (playing) return;
            Array.Clear(Cards, 0, Cards.Length);
            if (IsSwahili) { Cards[0] = "robot"; Cards[2] = "box"; }
            selected = badSlot = dragging = -1;
            draggedWord = null;
            row = frame = 0; activeMeaning = null;
        }
        public bool RunSentence()
        {
            if (playing || NeedsLanguageSelection) return false;
            if (IsSwahili && (Cards[0] != "robot" || Cards[2] != "box")) return false;
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
            if (IsSwahili || playing || from < 0 || from > 2 || to < 0 || to > 2) return;
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
                RoomOneSave.Store(Progress, IsSwahili);
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
            ReleaseCollectionStyle();
            if (chime) Destroy(chime);
        }
    }
}
