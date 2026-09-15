using Hackathon.Core;
using Hackathon.RoomOne;
using UnityEngine;

namespace Hackathon.Map
{
    [DefaultExecutionOrder(-100)]
    public sealed class WorldMapController : MonoBehaviour
    {
        public CodexRoboMotor robo;
        public Camera mapCamera;
        public Transform workshopEntrance;
        public Transform meadowEntrance;
        public RoomOneProgress Progress { get; private set; }
        public int SelectedRoom { get; private set; } = 1;
        public bool NearWorkshop => robo != null && workshopEntrance != null &&
            Vector3.Distance(robo.transform.position, workshopEntrance.position) < 1.7f;

        static readonly Color Ink = new Color(.12f, .23f, .24f);
        static readonly Color Muted = new Color(.38f, .47f, .46f);
        static readonly Color Paper = new Color(.99f, .98f, .93f, .97f);
        static readonly Color Green = new Color(.19f, .43f, .32f);
        readonly Rect details = new Rect(934, 432, 314, 246);
        GUIStyle label, button;

        void Awake()
        {
            Application.runInBackground = true;
            if (!Application.isEditor && Application.platform != RuntimePlatform.WebGLPlayer)
                Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
            Progress = RoomOneSave.Load();
        }

        void Update()
        {
            if (robo == null) return;
            // IMGUI buttons must not also send a floor destination to the motor.
            Vector2 pointer = ScreenToCanvas(Input.mousePosition);
            bool overHud = pointer.y < 106 || pointer.y > 626 || details.Contains(pointer) ||
                (NearWorkshop && new Rect(432, 556, 384, 49).Contains(pointer)) ||
                MarkerRect(workshopEntrance).Contains(pointer) || MarkerRect(meadowEntrance).Contains(pointer);
            robo.acceptInput = !overHud;
            if (overHud && !robo.HasDestination) robo.StopWalking();
            if (overHud && Input.GetMouseButtonDown(0)) robo.StopWalking();
            if (NearWorkshop && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))) EnterRoomOne();
        }

        public void SelectRoom(int room)
        {
            if (room != 1 && room != 2) return;
            SelectedRoom = room;
            if (robo != null) robo.WalkTo((room == 1 ? workshopEntrance : meadowEntrance).position);
        }

        public void EnterRoomOne()
        {
            if (robo != null) robo.StopWalking();
            WorldNavigation.OpenRoomOne();
        }

        Vector2 ScreenToCanvas(Vector3 point)
        {
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            return new Vector2((point.x - (Screen.width - 1280 * scale) / 2) / scale,
                (Screen.height - point.y - (Screen.height - 720 * scale) / 2) / scale);
        }

        Rect MarkerRect(Transform entrance)
        {
            if (entrance == null || mapCamera == null) return Rect.zero;
            var point = ScreenToCanvas(mapCamera.WorldToScreenPoint(entrance.position + Vector3.up * 2.6f));
            return new Rect(point.x - 104, point.y - 20, 208, 44);
        }

        void OnGUI()
        {
            if (Progress == null) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label);
                button = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                button.normal.textColor = Color.white;
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2,
                (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            Panel(new Rect(24, 22, 1232, 79), Paper, 18);
            Text(new Rect(45, 32, 430, 20), "ENGLISH WORLD  /  YOUR JOURNEY", 12, Muted, true);
            Text(new Rect(44, 50, 550, 43), "A little world of discoveries", 27, Ink, true);
            Text(new Rect(940, 35, 286, 27), Progress.isCleared ? "01 / 01   ROOM COMPLETE" : "01   ROOM TO EXPLORE", 16, Green, true);
            Text(new Rect(940, 64, 286, 23), (Progress.discoveredWords.Count + 6) + " words   /   " + Progress.discoveredCollections.Count + " discoveries", 14, Muted);

            Marker(workshopEntrance, Progress.isCleared ? "01  Workshop  /  CLEAR" : "01  The workshop", 1);
            Marker(meadowEntrance, "02  The meadow  /  SOON", 2);

            Panel(details, Paper, 20);
            Text(new Rect(958, 452, 266, 22), SelectedRoom == 1 ? "ROOM 01  /  READY TO EXPLORE" : "ROOM 02  /  COMING LATER", 12, Green, true);
            Text(new Rect(956, 481, 268, 37), SelectedRoom == 1 ? "The workshop" : "The meadow", 27, Ink, true);
            Text(new Rect(958, 526, 258, 54), SelectedRoom == 1 ? "Meet a robot. Move a box.\nDiscover what your words can do." : "Another place to discover.\nThis room is still being built.", 16, Muted);
            if (SelectedRoom == 1)
            {
                Text(new Rect(958, 582, 258, 24), "Words  " + (Progress.discoveredWords.Count + 6) + " / 8     ·     Scenes  " + Progress.discoveredCollections.Count + " / 6", 13, Green, true);
                if (Action(new Rect(956, 619, 270, 42), Progress.isCleared ? "Explore again  >" : "Enter Room 1  >", Green)) EnterRoomOne();
            }
            else
            {
                Panel(new Rect(956, 619, 270, 42), new Color(.86f, .88f, .83f), 10);
                Text(new Rect(972, 627, 238, 27), "Not open yet", 17, Muted, true);
            }
            Panel(new Rect(24, 626, 570, 52), Paper, 14);
            Text(new Rect(42, 640, 539, 28), "WASD / Arrows   Move     ·     Click the ground to walk", 16, Ink);
            if (NearWorkshop)
            {
                if (Action(new Rect(432, 556, 384, 49), "E / Enter   ·   Enter the workshop", Green)) EnterRoomOne();
            }
            Text(new Rect(28, 688, 650, 22), "Follow your curiosity. Every experiment is saved.", 13, Ink);
            GUI.matrix = previous;
        }

        void Marker(Transform entrance, string title, int room)
        {
            Rect rect = MarkerRect(entrance);
            button.fontSize = 15;
            if (Action(rect, title, room == SelectedRoom ? Green : new Color(.22f, .32f, .31f))) SelectRoom(room);
            button.fontSize = 18;
        }

        static void Panel(Rect rect, Color color, float radius)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, radius);
        }

        bool Action(Rect rect, string title, Color color)
        {
            // Use render-pixel input, as Room 1 does, so Retina IMGUI scaling cannot offset clicks.
            bool hover = rect.Contains(ScreenToCanvas(Input.mousePosition));
            Panel(rect, hover ? Color.Lerp(color, Color.white, .12f) : color, 10);
            GUI.Label(rect, title, button);
            if (!hover || Event.current.type != EventType.MouseDown || Event.current.button != 0) return false;
            Event.current.Use();
            return true;
        }

        void Text(Rect rect, string value, int size, Color color, bool bold = false)
        {
            label.fontSize = size;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.normal.textColor = color;
            GUI.Label(rect, value, label);
        }
    }
}
