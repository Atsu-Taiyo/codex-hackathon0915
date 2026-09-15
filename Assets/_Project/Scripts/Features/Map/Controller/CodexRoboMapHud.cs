using UnityEngine;

namespace Hackathon.Map
{
    public sealed class CodexRoboMapHud : MonoBehaviour
    {
        public CodexRoboMotor robo;
        GUIStyle title, text;
        void OnGUI()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            }
            var previous = GUI.matrix;
            float scale = Mathf.Max(.7f, Screen.width / 1280f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUI.Box(new Rect(20,20,370,100), GUIContent.none);
            GUI.Label(new Rect(36,28,340,36), "CODEX ROBO  /  WALKING MAP", title);
            GUI.Label(new Rect(36,66,340,24), "WASD / Arrow keys   ·   Click the floor to walk", text);
            GUI.Label(new Rect(36,91,340,24), robo != null && robo.IsWalking ? "WALKING" : "IDLE", text);
            GUI.matrix = previous;
        }
    }
}
