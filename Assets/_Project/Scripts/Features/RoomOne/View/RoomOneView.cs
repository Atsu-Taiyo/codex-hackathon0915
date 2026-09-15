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
        readonly Rect[] slots = { new Rect(305, 635, 262, 86), new Rect(583, 635, 262, 86), new Rect(861, 635, 262, 86) };
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
            DrawSprite(r, atlas, new Rect(index / 3f, 1f - (actionRow + 1) / 6f, 1f / 3, 1f / 6));
        }
        void DrawSprite(Rect rect, Texture2D texture, Rect uv)
        {
            if (!texture || Event.current.type != EventType.Repaint) return;
            if (!pixelCutout) { GUI.DrawTextureWithTexCoords(rect, texture, uv); return; }
            // Generated PNGs contain alpha. Reject low-alpha extraction haze at render time;
            // the original generated files and opaque dark character outlines are preserved.
            Matrix4x4 matrix = GUI.matrix;
            Vector3 origin = matrix.MultiplyPoint3x4(new Vector3(rect.x, rect.y));
            Vector3 size = matrix.MultiplyVector(new Vector3(rect.width, rect.height));
            GUI.matrix = Matrix4x4.identity;
            Graphics.DrawTexture(new Rect(origin.x, origin.y, size.x, size.y), texture, uv, 0, 0, 0, 0, Color.white, pixelCutout);
            GUI.matrix = matrix;
        }
        void BoxLiftFrame(Rect stage, int index)
        {
            // The generated strip has unequal empty margins; these measured row boundaries
            // retain each entire pose, including the raised robot's head in the final frame.
            int top = index == 0 ? 0 : index == 1 ? 500 : 1000;
            int height = index == 2 ? 536 : 500;
            float scale = stage.height / 536f;
            Rect destination = new Rect(stage.center.x - 512 * scale, stage.yMax - height * scale, 1024 * scale, height * scale);
            DrawSprite(destination, boxLiftAtlas, new Rect(0, 1f - (top + height) / 1536f, 1, height / 1536f));
        }
        void Entity(Rect r, string entity)
        {
            if (entity == "robot")
            {
                // Generated alpha cutout preserves the supplied character and excludes app controls.
                DrawSprite(r, reference, new Rect(.20f, .12f, .60f, .70f));
            }
            else DrawSprite(r, atlas, new Rect(.191f, .846f, .1f, .082f));
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
            if (backdrop) GUI.DrawTexture(new Rect(0, 0, 1600, 900), backdrop, ScaleMode.StretchToFill);
            bool free = modal == "";
            GUI.enabled = free;
            DrawHeader(); DrawWorld(); DrawBuilder(); DrawFooter();
            GUI.enabled = true;
            if (!free) DrawModal();
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape) { modal = ""; selected = -1; e.Use(); }
                else if (free && e.keyCode == KeyCode.Return) { RunSentence(); e.Use(); }
                else if (free && selected >= 0 && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)) { Cards[selected] = null; e.Use(); }
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
            if (Button(new Rect(34, 32, 84, 76), "⌂", White, 42, !playing)) OpenModal("map");
            Label(new Rect(144, 31, 180, 27), "ENGLISH WORLD", 16, Muted, TextAnchor.MiddleLeft, true);
            Label(new Rect(144, 57, 190, 48), "Room 01", 32, Ink, TextAnchor.MiddleLeft, true);
            Panel(new Rect(374, 24, 850, 197), new Color(1, 1, 1, .95f), 25);
            Label(new Rect(397, 32, 195, 25), "MAKE THIS HAPPEN", 14, Muted, TextAnchor.MiddleLeft, true);
            if (Button(new Rect(1046, 31, 155, 28), "Watch goal", Hex("EEF3FC"), 14, !playing)) WatchGoal();
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
            Label(new Rect(1300, 91, 256, 21), "GOAL        WORDS        SCENES", 11, Muted, TextAnchor.MiddleCenter, true);
        }
        void DrawWorld()
        {
            Rect stage = new Rect(458, 229, 684, 342);
            // Three-frame generated artwork remains the source of every standard action.
            if (activeMeaning == null || (activeMeaning.subject == "robot" && activeMeaning.target == "box")) Frame(stage, row, frame);
            else if (UsesBoxLiftAnimation) BoxLiftFrame(stage, frame);
            else DrawUnusual(stage);
            if (playing)
            {
                Panel(new Rect(617, 549, 366, 25), White, 12, false);
                Label(new Rect(617, 549, 366, 25), goalPreview ? "WATCH THE GOAL   ·   " + (frame + 1) + " / 3" : "EXPERIMENT   ·   " + (frame + 1) + " / 3", 13, Muted, TextAnchor.MiddleCenter, true);
            }
            else
            {
                var robotHit = new Rect(548, 281, 230, 251);
                var boxHit = new Rect(850, 367, 223, 179);
                if (Hit(robotHit)) Discover("robot");
                if (Hit(boxHit)) Discover("box");
                if (robotHit.Contains(pointer)) Label(new Rect(576, 246, 190, 36), Progress.discoveredWords.Contains("robot") ? "robot" : "?", 25, Ink, TextAnchor.MiddleCenter, true);
                if (boxHit.Contains(pointer)) Label(new Rect(844, 316, 210, 36), Progress.discoveredWords.Contains("box") ? "box" : "?", 25, Ink, TextAnchor.MiddleCenter, true);
            }
            if (Time.unscaledTime < spotlightUntil)
            {
                Rect spot = new Rect(spotlight == "robot" ? 557 : 853, 278, 210, 46);
                Panel(spot, Blue, 20); Label(spot, "+ " + spotlight, 23, Ink, TextAnchor.MiddleCenter, true);
            }
            if (!playing) Label(new Rect(390, 546, 820, 28), Time.unscaledTime < noticeUntil ? notice : "Look closely. Touch an object. Discover its word.", 18, Ink);
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
            Label(new Rect(stage.x + 55, stage.y + 25, stage.width - 110, 42), activeMeaning.Display, 23, Ink);
            Label(new Rect(stage.x, stage.yMax - 45, stage.width, 30), "A curious experiment!", 17, Muted);
        }
        void DrawBuilder()
        {
            Panel(new Rect(32, 588, 1536, 253), new Color(1, 1, 1, .97f), 27);
            Label(new Rect(61, 603, 220, 24), "BUILD YOUR SENTENCE", 14, Muted, TextAnchor.MiddleLeft, true);
            Label(new Rect(1114, 603, 408, 24), "Drag to reorder  ·  Click a slot to replace", 14, Muted, TextAnchor.MiddleRight);
            Label(new Rect(68, 650, 201, 34), "Your words", 22, Ink, TextAnchor.MiddleLeft, true);
            int n = 0;
            foreach (string noun in new[] { "robot", "box" })
            {
                bool known = Progress.discoveredWords.Contains(noun);
                if (Button(new Rect(64, 702 + n * 53, 189, 43), known ? noun : "? ? ?", Blue, 20, known && !playing)) Choose(noun);
                n++;
            }
            for (int i = 0; i < 3; i++)
            {
                string card = Cards[i];
                bool noun = RoomOneRules.IsNoun(card);
                Color color = string.IsNullOrEmpty(card) ? Hex("F0F3F7") : noun ? Blue : Green;
                if (selected == i) Panel(new Rect(slots[i].x - 3, slots[i].y - 3, slots[i].width + 6, slots[i].height + 6), Hex("71A5EA"), 18, false);
                if (badSlot == i) color = Hex("FFE0CE");
                Panel(slots[i], color, 16);
                string value = string.IsNullOrEmpty(card) ? "+" : noun ? (i == 0 ? "The " : "the ") + card : RoomOneRules.DisplayVerb(card);
                Label(slots[i], value, string.IsNullOrEmpty(card) ? 30 : 28, string.IsNullOrEmpty(card) ? Muted : Ink, TextAnchor.MiddleCenter, true);
                Label(new Rect(slots[i].x, 724, slots[i].width, 20), badSlot == i ? "Try a different card here" : "", 12, Hex("BA7548"));
                var e = Event.current;
                if (!playing && modal == "" && e.type == EventType.MouseDown && slots[i].Contains(pointer))
                {
                    selected = i; dragging = i; dragStart = pointer; hasDragged = false;
                    if (e.clickCount == 2) Cards[i] = null;
                    e.Use();
                }
            }
            if (dragging >= 0 && Event.current.type == EventType.MouseDrag)
            {
                hasDragged |= Vector2.Distance(dragStart, pointer) > 8; Event.current.Use();
            }
            if (dragging >= 0 && Event.current.type == EventType.MouseUp)
            {
                if (hasDragged) for (int i = 0; i < 3; i++) if (slots[i].Contains(pointer))
                { SwapCards(dragging, i); break; }
                dragging = -1; Event.current.Use();
            }
            if (Button(new Rect(1160, 635, 235, 86), playing ? "Playing…" : "▶  RUN", Hex("54D77B"), 28, !playing)) RunSentence();
            if (Button(new Rect(1410, 635, 118, 86), "Reset", Hex("F0F3F7"), 18, !playing))
            { Array.Clear(Cards, 0, Cards.Length); selected = badSlot = -1; row = frame = 0; activeMeaning = null; }
            int hoverRow = -1;
            for (int i = 0; i < 6; i++)
            {
                var r = new Rect(305 + i * 204, 755, 189, 59);
                if (Button(r, RoomOneRules.Verbs[i], Cards[1] == RoomOneRules.Actions[i] ? Green : Hex("F8FAFD"), 23, !playing)) Choose(RoomOneRules.Actions[i]);
                if (modal == "" && !playing && r.Contains(pointer)) hoverRow = i;
            }
            if (hoverRow >= 0)
            {
                Rect hint = new Rect(Mathf.Clamp(305 + hoverRow * 204, 310, 1200), 433, 300, 176);
                Panel(hint, White, 18); Frame(new Rect(hint.x + 12, hint.y + 12, 276, 138), hoverRow, (int)(Time.unscaledTime * 2) % 3);
                Label(new Rect(hint.x, hint.yMax - 26, hint.width, 22), RoomOneRules.Verbs[hoverRow], 16, Ink, TextAnchor.MiddleCenter, true);
            }
            if (dragging >= 0 && hasDragged && !string.IsNullOrEmpty(Cards[dragging]))
            {
                var r = new Rect(pointer.x - 90, pointer.y - 32, 180, 64);
                Panel(r, Blue, 16); Label(r, RoomOneRules.DisplayVerb(Cards[dragging]), 21, Ink);
            }
        }
        void DrawFooter()
        {
            if (Button(new Rect(34, 852, 155, 37), "Collection", White, 17, !playing)) OpenModal("collection");
            Label(new Rect(226, 851, 530, 38), "WORDS  " + (Progress.discoveredWords.Count + 6) + " / 8      SCENES  " + Progress.discoveredCollections.Count + " / 6", 15, Ink, TextAnchor.MiddleLeft, true);
            Label(new Rect(756, 851, 520, 38), Progress.isCleared ? "ROOM CLEAR  ·  Keep experimenting" : "Every experiment teaches you something.", 15, Ink, TextAnchor.MiddleRight);
            if (Button(new Rect(1362, 852, 204, 37), "Map  /  Room 01", White, 17, !playing)) OpenModal("map");
        }
        void DrawModal()
        {
            GUI.DrawTexture(new Rect(0, 0, 1600, 900), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(.08f, .13f, .23f, .55f), 0, 0);
            Panel(new Rect(320, 142, 960, 616), White, 30);
            if (Button(new Rect(1200, 160, 53, 44), "×", Hex("F0F3F7"), 27)) modal = "";
            if (modal == "clear")
            {
                Label(new Rect(500, 177, 600, 67), "★  ROOM CLEAR  ★", 43, Hex("D99A1D"), TextAnchor.MiddleCenter, true);
                Frame(new Rect(576, 264, 448, 224), 2, 2);
                Label(new Rect(475, 508, 650, 50), "You discovered what happens!", 27, Ink, TextAnchor.MiddleCenter, true);
                if (Button(new Rect(491, 586, 298, 64), "Keep exploring", Green, 24)) modal = "";
                if (Button(new Rect(809, 586, 298, 64), "Back to map", Blue, 24)) modal = "map";
                Label(new Rect(500, 682, 600, 30), "Room 02  ·  Coming later", 18, Muted);
            }
            else if (modal == "map")
            {
                Label(new Rect(440, 181, 720, 57), "ENGLISH WORLD", 37, Ink, TextAnchor.MiddleCenter, true);
                Label(new Rect(470, 247, 660, 33), "Small discoveries. A whole new language.", 20, Muted);
                Panel(new Rect(416, 326, 386, 290), Hex("EDF5FF"), 25);
                Label(new Rect(446, 349, 325, 48), "Room 01  ·  The workshop", 25, Ink, TextAnchor.MiddleCenter, true);
                Label(new Rect(446, 414, 325, 84), (Progress.isCleared ? "CLEAR" : "EXPLORE") + "\nWords " + (Progress.discoveredWords.Count + 6) + "/8  ·  Scenes " + Progress.discoveredCollections.Count + "/6", 22, Ink);
                if (Button(new Rect(455, 535, 308, 58), "Enter room", Green)) { modal = ""; row = frame = 0; activeMeaning = null; }
                Panel(new Rect(842, 326, 334, 290), Hex("F3F4F6"), 25);
                Label(new Rect(882, 387, 254, 120), "Room 02\nLOCKED\nComing later", 25, Muted);
            }
            else if (modal == "collection")
            {
                Label(new Rect(405, 165, 780, 57), "Your discoveries", 37, Ink, TextAnchor.MiddleLeft, true);
                if (Button(new Rect(407, 237, 215, 45), "Collection", Blue, 19)) scroll = Vector2.zero;
                if (Button(new Rect(635, 237, 247, 45), "My experiments", Hex("EDF3FA"), 19)) { modal = "history"; scroll = Vector2.zero; }
                Label(new Rect(916, 240, 290, 40), Progress.discoveredCollections.Count + " / 6 scenes", 20, Muted);
                for (int i = 0; i < 6; i++)
                {
                    int col = i % 3, line = i / 3;
                    var r = new Rect(407 + col * 267, 310 + line * 201, 251, 184);
                    string key = "robot:" + RoomOneRules.Actions[i] + ":box";
                    bool known = Progress.discoveredCollections.Contains(key);
                    Panel(r, Hex("F6F7FA"), 18);
                    if (known) Frame(new Rect(r.x + 9, r.y + 8, 233, 117), i, 2);
                    else Label(new Rect(r.x, r.y + 22, r.width, 90), "?", 48, Muted);
                    if (Button(new Rect(r.x + 9, r.y + 137, 233, 36), known ? RoomOneRules.Verbs[i] + "  ›" : "Undiscovered", known ? Green : Hex("E8ECF2"), 18, known))
                        Replay(new SentenceMeaning { subject = "robot", action = RoomOneRules.Actions[i], target = "box" });
                }
            }
            else if (modal == "history")
            {
                Label(new Rect(405, 165, 780, 57), "My experiments", 37, Ink, TextAnchor.MiddleLeft, true);
                if (Button(new Rect(407, 237, 240, 45), "‹  Collection", Blue, 19)) { modal = "collection"; scroll = Vector2.zero; }
                Label(new Rect(734, 237, 470, 45), Progress.executionHistory.Count + " experiments  ·  click to replay", 19, Muted, TextAnchor.MiddleRight);
                scroll = GUI.BeginScrollView(new Rect(405, 308, 812, 408), scroll, new Rect(0, 0, 785, Mathf.Max(408, Progress.executionHistory.Count * 67)));
                Vector2 viewportPointer = pointer;
                pointer = new Vector2(pointer.x - 405 + scroll.x, pointer.y - 308 + scroll.y);
                bool viewportEnabled = GUI.enabled;
                GUI.enabled = viewportEnabled && new Rect(405, 308, 812, 408).Contains(viewportPointer);
                if (Progress.executionHistory.Count == 0) Label(new Rect(0, 40, 770, 90), "Your first experiment is waiting.", 26, Muted);
                for (int i = 0; i < Progress.executionHistory.Count; i++)
                {
                    var meaning = Progress.executionHistory[Progress.executionHistory.Count - 1 - i];
                    if (Button(new Rect(0, i * 67, 778, 55), meaning.Display + "   ›", Hex("F0F5FB"), 22)) Replay(meaning);
                }
                GUI.enabled = viewportEnabled;
                GUI.EndScrollView();
                pointer = viewportPointer;
            }
        }
    }
}
