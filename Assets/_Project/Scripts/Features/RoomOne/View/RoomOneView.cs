using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController
    {
        static readonly Color Ink = Hex("24375C"), Muted = Hex("8291A7"), Blue = Hex("D5EAFE"), Green = Hex("BEEFCB"), White = Hex("FFFFFF");
        readonly Dictionary<string, GUIStyle> textStyles = new Dictionary<string, GUIStyle>();
        Vector2 pointer;
        readonly Rect[] slots = { new Rect(510, 604, 180, 70), new Rect(710, 604, 180, 70), new Rect(910, 604, 180, 70) };
        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        GUIStyle TextStyle(int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool bold = false)
        {
            string key = size + ":" + color + ":" + align + ":" + bold;
            if (!textStyles.TryGetValue(key, out var style))
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = size, alignment = align, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, wordWrap = true };
                style.normal.textColor = color;
                textStyles[key] = style;
            }
            return style;
        }
        void Label(Rect r, string value, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool bold = false) => GUI.Label(r, value, TextStyle(size, color, align, bold));
        void Panel(Rect r, Color color, int radius = 20, bool shadow = true)
        {
            if (shadow)
            {
                GUI.DrawTexture(new Rect(r.x, r.y + 5, r.width, r.height), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.13f, .20f, .34f, .13f), 0, radius);
            }
            GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, radius);
        }
        bool Button(Rect r, string text, Color color, int size = 23, bool enabled = true)
        {
            bool hover = enabled && r.Contains(pointer);
            Panel(r, enabled ? (hover ? Color.Lerp(color, White, .25f) : color) : Color.Lerp(color, White, .65f), 16);
            Label(r, text, size, enabled ? Ink : Muted, TextAnchor.MiddleCenter, true);
            return Hit(r, enabled);
        }
        bool Hit(Rect r, bool enabled = true)
        {
            if (!GUI.enabled || !enabled || Event.current.type != EventType.MouseDown || Event.current.button != 0 || !r.Contains(pointer)) return false;
            Event.current.Use();
            return true;
        }
        void Frame(Rect r, int actionRow, int index)
        {
            if (artwork && actionRow >= 0 && actionRow < RoomOneRules.Actions.Length)
                DrawPose(r, artwork.Frame("robot:" + RoomOneRules.Actions[actionRow] + ":box", index));
        }
        void DrawPose(Rect rect, Texture2D texture)
        {
            if (!texture || Event.current.type != EventType.Repaint) return;
            // The PR supplies padded, bottom-aligned canvases. Keep their aspect ratio
            // and full alpha silhouettes, including the raised head in reverse lift.
            float scale = Mathf.Min(rect.width / texture.width, rect.height / texture.height);
            var destination = new Rect(rect.center.x - texture.width * scale / 2,
                rect.yMax - texture.height * scale, texture.width * scale, texture.height * scale);
            GUI.DrawTexture(destination, texture, ScaleMode.StretchToFill, true);
        }
        void BoxLiftFrame(Rect stage, int index)
        {
            if (artwork) DrawPose(stage, artwork.Frame("box:lift:robot", index));
        }
        void Entity(Rect r, string entity)
        {
            if (artwork) DrawPose(r, entity == "robot" ? artwork.robot : artwork.box);
        }
        void OnGUI()
        {
            var oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            // Input.mousePosition is in render pixels, independent of IMGUI's matrix/clip state.
            pointer = new Vector2((Input.mousePosition.x - (Screen.width - 1600 * scale) / 2) / scale,
                (Screen.height - Input.mousePosition.y - (Screen.height - 900 * scale) / 2) / scale);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600 * scale) / 2, (Screen.height - 900 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            GUI.DrawTexture(new Rect(0, 0, 1600, 900), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Hex("FBF2E2"), 0, 0);
            if (artwork && artwork.background) GUI.DrawTexture(new Rect(0, 0, 1600, 900), artwork.background, ScaleMode.StretchToFill);
            bool free = modal == "" && !NeedsLanguageSelection;
            GUI.enabled = free;
            DrawHeader(); DrawWorld(); DrawBuilder(); DrawFooter();
            GUI.enabled = true;
            if (!free) DrawModal();
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape && !NeedsLanguageSelection) { modal = ""; selected = -1; draggedWord = null; dragging = -1; hasDragged = false; e.Use(); }
                else if (free && e.keyCode == KeyCode.Return) { RunSentence(); e.Use(); }
                else if (free && selected >= 0 && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)) { RemoveCard(selected); e.Use(); }
                else if (free && selected >= 0 && (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow))
                {
                    int next = Mathf.Clamp(selected + (e.keyCode == KeyCode.LeftArrow ? -1 : 1), 0, 2);
                    SwapCards(selected, next); e.Use();
                }
            }
            GUI.matrix = oldMatrix;
        }
        void DrawHeader()
        {
            if (!IsSwahili) DrawVoiceButton();
            if (Button(new Rect(34, 32, 84, 76), "⌂", White, 42, !playing && !voiceBusy)) ReturnToMap();
            Label(new Rect(144, 31, 180, 27), (IsSwahili ? "SWAHILI WORLD" : "ENGLISH WORLD"), 16, Muted, TextAnchor.MiddleLeft, true);
            Label(new Rect(144, 57, 190, 48), "Room 01", 32, Ink, TextAnchor.MiddleLeft, true);
            Panel(new Rect(374, 24, 850, 197), new Color(1, 1, 1, .95f), 25);
            if (Button(new Rect(1046, 31, 155, 28), "▶", Hex("EEF3FC"), 14, !playing)) WatchGoal();
            for (int i = 0; i < 3; i++)
            {
                Rect r = new Rect(418 + i * 257, 67, 229, 115);
                Panel(r, Hex("FFFAEA"), 14, false); Frame(r, 2, i);
                if (i < 2) Label(new Rect(r.xMax + 4, 94, 25, 46), "›", 38, Muted);
                Panel(new Rect(r.center.x - 4, 193, 8, 8), goalPreview && frame == i ? Hex("5089F5") : Hex("D6DEEA"), 4, false);
            }
            Panel(new Rect(1290, 32, 276, 92), White, 24);
            for (int i = 0; i < 3; i++)
            {
                bool earned = i == 0 ? Progress.isCleared : i == 1 ? Progress.discoveredWords.Count == 2 : Progress.discoveredCollections.Count == 6;
                Label(new Rect(1311 + i * 78, 37, 66, 57), "★", 45, earned ? Hex("FFBA2E") : Hex("DDE4EC"));
            }
        }
        void DrawWorld()
        {
            Rect stage = new Rect(458, 229, 684, 342);
            // The same PR frames drive the stage, goal, hints, collection and clear screen.
            if (activeMeaning == null || (activeMeaning.subject == "robot" && activeMeaning.target == "box")) Frame(stage, row, frame);
            else if (UsesBoxLiftAnimation) BoxLiftFrame(stage, frame);
            else DrawUnusual(stage);
            if (playing)
            {
                Panel(new Rect(617, 549, 366, 25), White, 12, false);
                for (int i = 0; i < 3; i++) Panel(new Rect(769 + i * 26, 557, 10, 10), frame == i ? Hex("5089F5") : Hex("D6DEEA"), 5, false);
            }
            else
            {
                var robotHit = new Rect(598, 305, 240, 266);
                var boxHit = new Rect(837, 395, 187, 168);
                if (!Progress.discoveredWords.Contains("robot")) Label(new Rect(630, 255, 46, 38), "+", 28 + (int)(3 * Mathf.Sin(Time.unscaledTime * 3)), Muted);
                if (!Progress.discoveredWords.Contains("box")) Label(new Rect(934, 325, 46, 38), "+", 28 + (int)(3 * Mathf.Sin(Time.unscaledTime * 3)), Muted);
                if (Hit(robotHit)) Discover("robot");
                if (Hit(boxHit)) Discover("box");
                if (robotHit.Contains(pointer)) Label(new Rect(576, 246, 190, 36), Progress.discoveredWords.Contains("robot") ? DisplayWord("robot") : "?", 25, Ink, TextAnchor.MiddleCenter, true);
                if (boxHit.Contains(pointer)) Label(new Rect(844, 316, 210, 36), Progress.discoveredWords.Contains("box") ? DisplayWord("box") : "?", 25, Ink, TextAnchor.MiddleCenter, true);
            }
            if (Time.unscaledTime < spotlightUntil)
            {
                Rect spot = new Rect(spotlight == "robot" ? 557 : 853, 278, 210, 46);
                Panel(spot, Blue, 20); Label(spot, "+ " + DisplayWord(spotlight), 23, Ink, TextAnchor.MiddleCenter, true);
            }
        }
        void DrawUnusual(Rect stage)
        {
            float amount = frame * 35;
            Rect actor = new Rect(stage.x + 105, stage.y + 110, 150, 150);
            Rect target = new Rect(stage.x + 365, stage.y + 110, 150, 150);
            switch (activeMeaning.action)
            {
                case "push": actor.x += amount; target.x += amount; break;
                case "pull": actor.x -= amount; target.x -= amount; break;
                case "lift": target.y -= amount; break;
                case "open": target.width += amount; target.height -= amount / 2; break;
                case "shake": target.x += frame == 1 ? -35 : frame == 2 ? 35 : 0; break;
                case "break": target.height -= amount; target.y += amount; break;
            }
            Entity(actor, activeMeaning.subject); Entity(target, activeMeaning.target);
            Label(new Rect(stage.x + 275, stage.y + 130, 90, 60), activeMeaning.action == "lift" ? "↑" : "→", 42, Hex("E0A146"));
            Label(new Rect(stage.x + 55, stage.y + 25, stage.width - 110, 42), DisplaySentence(activeMeaning), 23, Ink);
        }
        void WordCard(Rect r, string word, bool empty = false)
        {
            Panel(r, empty ? Hex("EEF2F7") : Blue, 16, !empty);
            Label(r, empty ? "·" : DisplayWord(word), 23, empty ? Muted : Ink, TextAnchor.MiddleCenter, true);
            if (!empty) Label(new Rect(r.x + 7, r.y + 22, 15, 26), "⋮", 20, Muted);
        }
        void DrawBuilder()
        {
            Panel(new Rect(32, 588, 1536, 253), new Color(1, 1, 1, .97f), 27);
            if (!IsSwahili) DrawVoiceControls();
            var e = Event.current;
            Rect tray = new Rect(48, 690, 1504, 80);
            for (int i = 0; i < 3; i++)
            {
                Rect r = slots[i];
                if (selected == i || (draggedWord != null && r.Contains(pointer))) Panel(new Rect(r.x-3,r.y-3,r.width+6,r.height+6), Hex("71A5EA"), 18, false);
                if (badSlot == i) Panel(new Rect(r.x-4,r.y-4,r.width+8,r.height+8), Hex("F5A76C"), 18, false);
                WordCard(r, Cards[i], string.IsNullOrEmpty(Cards[i]) || (hasDragged && dragging == i));
                if ((!IsSwahili || i == 1) && !playing && GUI.enabled && e.type == EventType.MouseDown && e.button == 0 && r.Contains(pointer))
                {
                    selected = i; dragging = i; draggedWord = Cards[i]; dragStart = pointer; hasDragged = false;
                    if (e.clickCount == 2) { RemoveCard(i); draggedWord = null; }
                    e.Use();
                }
            }
            string[] words = IsSwahili ? RoomOneRules.Actions : new[] { "robot", "box", "push", "pull", "lift", "open", "shake", "break" };
            if (IsSwahili) Label(new Rect(1094, 604, 28, 70), ".", 28, Ink);
            int hoverRow = -1;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                Rect r = new Rect((IsSwahili ? 247 : 59) + i * 188, 696, 180, 70);
                bool known = Progress.globalVocabulary.Contains(word), used = Array.IndexOf(Cards, word) >= 0;
                WordCard(r, word, !known || used || (hasDragged && dragging == -1 && draggedWord == word));
                if (!known) Label(r, "?", 26, Muted);
                if (known && !used && !playing && GUI.enabled && e.type == EventType.MouseDown && e.button == 0 && r.Contains(pointer))
                { draggedWord = word; dragging = -1; dragStart = pointer; hasDragged = false; e.Use(); }
                if ((IsSwahili || i >= 2) && known && !used && !playing && modal == "" && r.Contains(pointer) && draggedWord == null) hoverRow = IsSwahili ? i : i - 2;
            }
            if (draggedWord != null && e.type == EventType.MouseDrag)
            { hasDragged |= Vector2.Distance(dragStart, pointer) > 8; e.Use(); }
            if (draggedWord != null && e.type == EventType.MouseUp && e.button == 0)
            {
                if (hasDragged)
                {
                    int destination = Array.FindIndex(slots, r => r.Contains(pointer));
                    if (destination >= 0) PlaceCard(draggedWord, destination);
                    else if (tray.Contains(pointer) && dragging >= 0) RemoveCard(dragging);
                }
                else if (dragging < 0) Choose(draggedWord);
                draggedWord = null; dragging = -1; hasDragged = false; e.Use();
            }
            // Playback controls are a separate row inside the same widget.
            if (Button(new Rect(690, 785, 220, 43), playing ? "•••" : "▶", Hex("54D77B"), 27, !playing)) RunSentence();
            if (Button(new Rect(931, 785, 70, 43), "↺", Hex("EEF2F7"), 28, !playing)) ResetCards();
            if (hoverRow >= 0)
            {
                Rect hint = new Rect(Mathf.Clamp(59 + (hoverRow + 2) * 188, 310, 1200), 414, 300, 170);
                Panel(hint, White, 18); Frame(new Rect(hint.x + 12, hint.y + 12, 276, 138), hoverRow, (int)(Time.unscaledTime * 2) % 3);
            }
            if (draggedWord != null && hasDragged) WordCard(new Rect(pointer.x-90,pointer.y-35,180,70), draggedWord);
        }
        void DrawFooter()
        {
            if (Button(new Rect(34, 852, 155, 37), "Collection", White, 17, !playing)) OpenModal("collection");
            Label(new Rect(226, 851, 530, 38), "WORDS  " + (Progress.discoveredWords.Count + 6) + " / 8      SCENES  " + Progress.discoveredCollections.Count + " / 6", 15, Ink, TextAnchor.MiddleLeft, true);
            if (Button(new Rect(1362, 852, 204, 37), "Map  /  Room 01", White, 17, !playing && !voiceBusy)) ReturnToMap();
        }
        void DrawModal()
        {
            if (NeedsLanguageSelection)
            {
                Panel(new Rect(0, 0, 1600, 900), new Color(.08f, .13f, .23f, .65f), 0, false);
                Panel(new Rect(360, 230, 880, 440), White, 30);
                Label(new Rect(420, 270, 760, 60), "Room 01 · Choose your language", 34, Ink, TextAnchor.MiddleCenter, true);
                Label(new Rect(420, 345, 760, 45), "動詞を選んで、ロボットを動かそう", 24, Muted);
                if (Button(new Rect(420, 432, 360, 110), "English / 英語", Blue, 28)) SelectLanguage(false);
                if (Button(new Rect(820, 432, 360, 110), "Kiswahili / スワヒリ語", Green, 26)) SelectLanguage(true);
                if (Button(new Rect(650, 588, 300, 44), "Back to map", Hex("EEF2F7"), 20)) ReturnToMap();
                return;
            }
            if (modal == "collection" || modal == "history")
            {
                DrawCollection();
                return;
            }
            GUI.DrawTexture(new Rect(0, 0, 1600, 900), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.08f, .13f, .23f, .55f), 0, 0);
            Panel(new Rect(320, 142, 960, 616), White, 30);
            if (Button(new Rect(1200, 160, 53, 44), "×", Hex("F0F3F7"), 27)) modal = "";
            if (modal == "clear")
            {
                Label(new Rect(500, 177, 600, 67), "★  ROOM CLEAR  ★", 43, Hex("D99A1D"), TextAnchor.MiddleCenter, true);
                Frame(new Rect(576, 264, 448, 224), 2, 2);
                if (Button(new Rect(491, 586, 298, 64), "▶", Green, 24)) modal = "";
                if (Button(new Rect(809, 586, 298, 64), "Back to map", Blue, 24, !voiceBusy)) ReturnToMap();
                Label(new Rect(500, 682, 600, 30), "Room 02  ·  Coming later", 18, Muted);
            }
            else if (modal == "map")
            {
                Label(new Rect(440, 181, 720, 57), (IsSwahili ? "SWAHILI WORLD" : "ENGLISH WORLD"), 37, Ink, TextAnchor.MiddleCenter, true);
                Panel(new Rect(416, 326, 386, 290), Hex("EDF5FF"), 25);
                Label(new Rect(446, 349, 325, 48), "Room 01  ·  The workshop", 25, Ink, TextAnchor.MiddleCenter, true);
                Label(new Rect(446, 414, 325, 84), (Progress.isCleared ? "CLEAR" : "EXPLORE") + "\nWords " + (Progress.discoveredWords.Count + 6) + "/8  ·  Scenes " + Progress.discoveredCollections.Count + "/6", 22, Ink);
                if (Button(new Rect(455, 535, 308, 58), "▶", Green)) { modal = ""; row = frame = 0; activeMeaning = null; }
                Panel(new Rect(842, 326, 334, 290), Hex("F3F4F6"), 25);
                Label(new Rect(882, 387, 254, 120), "Room 02\nLOCKED\nComing later", 25, Muted);
            }
        }
    }
}
