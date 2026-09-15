using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Editor
{
    [InitializeOnLoad]
    public static class CodexRoboTools
    {
        const string Command = "Temp/CodexRobo.command";
        static CodexRoboTools() { EditorApplication.update += Poll; }
        static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(Command)) return;
            string command = File.ReadAllText(Command).Trim(); File.Delete(Command);
            try
            {
                if (command == "stop") EditorApplication.isPlaying = false;
                else if (command == "inspect") Inspect();
                else if (command == "build") CodexRoboBuilder.Build();
                else if (command == "open") { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(CodexRoboBuilder.ScenePath); }
                else if (command == "play") EditorApplication.isPlaying = true;
                else if (command == "test") RunPlaytest();
                else if (command == "capture") CodexRoboPlaytest.Capture(Camera.main,"Temp/CodexRobo-map.png");
            }
            catch (Exception e) { File.WriteAllText("Temp/CodexRobo-error.txt", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("Tools/Codex Robo/Run Walking Playtest")]
        public static void RunPlaytest()
        {
            var robo = UnityEngine.Object.FindAnyObjectByType<Hackathon.Map.CodexRoboMotor>();
            if (!EditorApplication.isPlaying || robo == null) throw new InvalidOperationException("Open CodexRoboMap and enter Play mode first.");
            robo.StartCoroutine(CodexRoboPlaytest.Run());
        }
        static void Inspect()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Content/Features/RoomOne/CloudRobot/Models/CloudRobot_Terminal.fbx");
            var preview = new PreviewRenderUtility();
            try
            {
                var go = UnityEngine.Object.Instantiate(source); preview.AddSingleGO(go);
                var renderers = go.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                go.transform.localScale /= bounds.size.y;
                bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                go.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                var mat = new Material(Shader.Find("Standard"));
                mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Content/Features/RoomOne/CloudRobot/Models/tripo_convert_ed1accbd-4268-4f25-8db6-660239cea543.fbm/CloudRobot_Terminal_basecolor.JPEG");
                foreach (var r in renderers) r.sharedMaterial = mat;
                preview.camera.transform.position = new Vector3(0, .5f, 2.5f);
                preview.camera.transform.LookAt(new Vector3(0,.5f,0));
                preview.camera.orthographic = true; preview.camera.orthographicSize = .6f;
                preview.camera.nearClipPlane = .01f; preview.camera.farClipPlane = 10;
                preview.camera.backgroundColor = new Color(.12f,.14f,.2f); preview.camera.clearFlags=CameraClearFlags.SolidColor;
                preview.lights[0].intensity=1.4f; preview.lights[0].transform.rotation=Quaternion.Euler(35,-140,0);
                preview.lights[1].intensity=.7f;
                preview.BeginStaticPreview(new Rect(0,0,800,800)); preview.Render();
                var image=preview.EndStaticPreview(); File.WriteAllBytes("Temp/CodexRobo-source.png",image.EncodeToPNG());
                var report="";
                foreach (var mf in go.GetComponentsInChildren<MeshFilter>()) report += mf.name+" vertices="+mf.sharedMesh.vertexCount+" localBounds="+mf.sharedMesh.bounds+" transform="+mf.transform.localToWorldMatrix+"\n";
                File.WriteAllText("Temp/CodexRobo-inspect.txt",report);
                UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(mat);
            }
            finally { preview.Cleanup(); }
        }
    }
}
