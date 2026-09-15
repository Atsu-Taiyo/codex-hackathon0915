using UnityEngine;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController
    {
        static readonly Color CollectionInk = Hex("303D46"), CollectionMuted = Hex("77817F"),
            CollectionAccent = Hex("427564"), CollectionLine = Hex("E6E9E4"),
            CollectionPaper = Hex("FDFDFB"), CollectionTile = Hex("F3F5F0");
        GUISkin collectionScrollSkin;
        Texture2D collectionScrollThumb;

        void EnsureCollectionScrollSkin()
        {
            if (collectionScrollSkin) return;
            collectionScrollSkin = Instantiate(GUI.skin);
            collectionScrollThumb = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            collectionScrollThumb.SetPixel(0, 0, Hex("C4CEC2"));
            collectionScrollThumb.Apply();
            collectionScrollSkin.hideFlags = HideFlags.HideAndDontSave;
            foreach (var style in new[] { collectionScrollSkin.verticalScrollbar, collectionScrollSkin.verticalScrollbarThumb,
                collectionScrollSkin.verticalScrollbarUpButton, collectionScrollSkin.verticalScrollbarDownButton })
            {
                bool thumb = style == collectionScrollSkin.verticalScrollbarThumb;
                style.fixedWidth = 6;
                style.fixedHeight = 0;
                style.border = style.padding = style.margin = new RectOffset();
                foreach (var state in new[] { style.normal, style.hover, style.active, style.focused })
                {
                    state.background = thumb ? collectionScrollThumb : null;
                    state.scaledBackgrounds = null;
                }
            }
        }

        void ReleaseCollectionStyle()
        {
            if (collectionScrollSkin) Destroy(collectionScrollSkin);
            if (collectionScrollThumb) Destroy(collectionScrollThumb);
        }

        void DrawCollection()
        {
            Panel(new Rect(0, 0, 1600, 900), new Color(.08f, .12f, .15f, .66f), 0, false);
            Panel(new Rect(280, 88, 1040, 724), CollectionPaper, 24);
            Label(new Rect(328, 122, 600, 25), "ROOM 01 / THE WORKSHOP", 13, CollectionMuted, TextAnchor.MiddleLeft, true);
            Label(new Rect(328, 152, 680, 48), "Collection", 36, CollectionInk, TextAnchor.MiddleLeft, true);

            var close = new Rect(1232, 120, 44, 44);
            if (close.Contains(Event.current.mousePosition)) Panel(close, CollectionTile, 22, false);
            Label(close, "×", 29, CollectionMuted);
            if (GUI.Button(close, new GUIContent("", "Close collection"), GUIStyle.none)) { modal = ""; return; }

            Panel(new Rect(328, 263, 944, 1), CollectionLine, 0, false);
            CollectionTab(new Rect(328, 218, 88, 46), "Scenes", "collection");
            CollectionTab(new Rect(448, 218, 138, 46), "Experiments", "history");
            bool history = modal == "history";
            string count = history
                ? Progress.executionHistory.Count + (Progress.executionHistory.Count == 1 ? " experiment" : " experiments")
                : Progress.discoveredCollections.Count + " / " + RoomOneRules.Actions.Length + " discovered";
            Label(new Rect(992, 218, 280, 46), count, 16, CollectionMuted, TextAnchor.MiddleRight);

            if (history) DrawCollectionHistory();
            else DrawCollectionScenes();

            Label(new Rect(328, 758, 850, 24), history
                ? (Progress.executionHistory.Count == 0 ? "" : "Latest first · Select an experiment to replay")
                : (Progress.discoveredCollections.Count == 0 ? "Try a sentence in the workshop to discover a scene." : "Select a scene to replay"),
                14, CollectionMuted, TextAnchor.MiddleLeft);
            Label(new Rect(1208, 758, 64, 24), "ESC", 12, CollectionMuted, TextAnchor.MiddleRight);
        }

        void CollectionTab(Rect rect, string title, string destination)
        {
            bool active = modal == destination;
            bool hover = rect.Contains(Event.current.mousePosition);
            Label(rect, title, 19, active || hover ? CollectionInk : CollectionMuted, TextAnchor.MiddleLeft, active);
            if (active) Panel(new Rect(rect.x, rect.yMax - 3, rect.width, 3), CollectionAccent, 1, false);
            if (GUI.Button(rect, new GUIContent("", title), GUIStyle.none) && !active)
            {
                modal = destination;
                scroll = Vector2.zero;
                GUI.FocusControl(null);
            }
        }

        void DrawCollectionScenes()
        {
            for (int i = 0; i < RoomOneRules.Actions.Length; i++)
            {
                var rect = new Rect(328 + i % 3 * 320, 288 + i / 3 * 232, 304, 216);
                string key = "robot:" + RoomOneRules.Actions[i] + ":box";
                bool known = Progress.discoveredCollections.Contains(key);
                bool hover = known && rect.Contains(Event.current.mousePosition);
                Panel(rect, hover ? Hex("EAF1E8") : known ? CollectionTile : Hex("F7F8F5"), 14, false);
                Label(new Rect(rect.x + 18, rect.y + 12, 38, 22), (i + 1).ToString("00"), 12, CollectionMuted, TextAnchor.MiddleLeft);
                if (known)
                {
                    Frame(new Rect(rect.x + 12, rect.y + 23, 280, 140), i, 2);
                    Label(new Rect(rect.x + 20, rect.y + 170, 210, 30), RoomOneRules.Verbs[i], 22, CollectionInk, TextAnchor.MiddleLeft, true);
                    var play = new Rect(rect.xMax - 52, rect.y + 167, 34, 34);
                    Panel(play, hover ? CollectionAccent : Hex("E2E9DF"), 17, false);
                    Label(new Rect(play.x + 2, play.y, play.width, play.height), "▶", 13, hover ? White : CollectionAccent);
                    // One native IMGUI target covers the whole tile, including its illustration.
                    if (GUI.Button(rect, new GUIContent("", "Replay " + RoomOneRules.Verbs[i]), GUIStyle.none))
                        Replay(new SentenceMeaning { subject = "robot", action = RoomOneRules.Actions[i], target = "box" });
                }
                else
                {
                    Label(new Rect(rect.x, rect.y + 54, rect.width, 66), "?", 32, Hex("B8C1B6"));
                    Label(new Rect(rect.x + 20, rect.y + 170, rect.width - 40, 30), "Undiscovered", 16, CollectionMuted, TextAnchor.MiddleLeft);
                }
            }
        }

        void DrawCollectionHistory()
        {
            int count = Progress.executionHistory.Count;
            if (count == 0)
            {
                Label(new Rect(328, 437, 944, 38), "No experiments yet", 26, CollectionInk);
                Label(new Rect(328, 484, 944, 32), "Try a sentence in the workshop.", 18, CollectionMuted);
                return;
            }

            const float rowHeight = 74;
            var viewport = new Rect(328, 288, 944, 448);
            float width = count * rowHeight > viewport.height ? 922 : 944;
            EnsureCollectionScrollSkin();
            var previousSkin = GUI.skin;
            GUI.skin = collectionScrollSkin;
            scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0, 0, width, Mathf.Max(viewport.height, count * rowHeight)));
            // Native GUI buttons handle both the scaled canvas and the scroll view's clipping.
            int first = Mathf.Max(0, Mathf.FloorToInt(scroll.y / rowHeight));
            int end = Mathf.Min(count, Mathf.CeilToInt((scroll.y + viewport.height) / rowHeight));
            SentenceMeaning replay = null;
            for (int i = first; i < end; i++)
            {
                var meaning = Progress.executionHistory[count - 1 - i];
                var rect = new Rect(0, i * rowHeight, width, rowHeight - 1);
                bool hover = rect.Contains(Event.current.mousePosition);
                if (hover) Panel(rect, CollectionTile, 10, false);
                Label(new Rect(18, rect.y, 50, rect.height), (count - i).ToString("00"), 14, CollectionMuted, TextAnchor.MiddleLeft);
                Label(new Rect(86, rect.y, width - 158, rect.height), meaning.Display, 22, CollectionInk, TextAnchor.MiddleLeft);
                Label(new Rect(width - 54, rect.y, 36, rect.height), "▶", 14, hover ? CollectionAccent : CollectionMuted);
                Panel(new Rect(18, rect.yMax, width - 36, 1), CollectionLine, 0, false);
                if (GUI.Button(rect, new GUIContent("", "Replay " + meaning.Display), GUIStyle.none)) replay = meaning;
            }
            GUI.EndScrollView();
            GUI.skin = previousSkin;
            if (replay != null) Replay(replay);
        }
    }
}
