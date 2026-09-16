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
        static readonly Rect TitleRect = new Rect(24, 24, 112, 44);
        static readonly Rect MoveHintRect = new Rect(24, 646, 270, 38);
        static readonly Rect EnterRect = new Rect(1014, 636, 242, 54);
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
            bool overHud = TitleRect.Contains(pointer) || MoveHintRect.Contains(pointer) || EnterRect.Contains(pointer) ||
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
            return new Rect(point.x - 86, point.y - 19, 172, 38);
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
            Panel(TitleRect, Paper, 14);
            Text(new Rect(44, 29, 78, 32), "Map", 22, Ink, true);
            Marker(workshopEntrance, Progress.isCleared ? "01  Workshop  ✓" : "01  Workshop", 1);
            Marker(meadowEntrance, "02  Meadow", 2);

            if (SelectedRoom == 1)
            {
                if (Action(EnterRect, NearWorkshop ? "Enter  (E)" : "Enter", Green)) EnterRoomOne();
            }
            else
            {
                Panel(EnterRect, Paper, 10);
                Text(new Rect(1050, 648, 170, 30), "Coming soon", 17, Muted, true);
            }
            Panel(MoveHintRect, Paper, 12);
            Text(new Rect(39, 651, 245, 28), "WASD / Arrow keys / Click", 14, Ink);
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
