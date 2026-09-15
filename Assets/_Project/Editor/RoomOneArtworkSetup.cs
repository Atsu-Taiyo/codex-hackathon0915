using System;
using System.IO;
using System.Linq;
using Hackathon.RoomOne;
using UnityEditor;
using UnityEngine;
using static Hackathon.Editor.IslandArtBuilder;
using Object = UnityEngine.Object;

namespace Hackathon.Editor
{
    public static class RoomOneArtworkSetup
    {
        const string Sprites = "Assets/_Project/Content/Features/RoomOne/CloudRobotSprites/";
        const string Output = "Assets/_Project/Content/Features/RoomOne/Art/Resources/RoomOne/";
        [Serializable] class Manifest { public SourceClip[] clips; public SourceFrame[] characters; }
        [Serializable] class SourceClip { public string id; public SourceFrame[] frames; }
        [Serializable] class SourceFrame { public string id; public string file; public int order; }

        [MenuItem("Tools/Room One/Connect CloudRobot Artwork")]
        public static void Connect()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop playback before connecting artwork.");
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Sprites + "animation-manifest.json"));
            if (manifest.clips.Length != 7) throw new InvalidOperationException("Expected seven PR #3 clips.");
            var clips = manifest.clips.Select(clip => new RoomOneArtwork.Clip {
                id = clip.id,
                frames = clip.frames.OrderBy(frame => frame.order).Select(frame => Texture(Sprites + frame.file)).ToArray()
            }).ToArray();
            var robot = Texture(Sprites + manifest.characters.Single(c => c.id == "RobotIdle").file);
            var box = Texture(Sprites + manifest.characters.Single(c => c.id == "BoxIdle").file);
            RenderWorkshop();
            var artwork = AssetDatabase.LoadAssetAtPath<RoomOneArtwork>(Output + "CloudRobotArtwork.asset");
            if (!artwork) { artwork = ScriptableObject.CreateInstance<RoomOneArtwork>(); AssetDatabase.CreateAsset(artwork, Output + "CloudRobotArtwork.asset"); }
            artwork.clips = clips; artwork.robot = robot; artwork.box = box;
            artwork.background = Texture(Output + "RoomOne_CloudWorkshop.png");
            if (!artwork.IsComplete) throw new InvalidOperationException("CloudRobot artwork validation failed.");
            EditorUtility.SetDirty(artwork); AssetDatabase.SaveAssets();
            Debug.Log("[RoomOne] All 21 PR poses, both characters and the new workshop background connected.");
        }

        static Texture2D Texture(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (!texture) throw new InvalidOperationException("Missing PNG; fetch PR #3 LFS data: " + path);
            return texture;
        }

        static void RenderWorkshop()
        {
            // A real render of the supplied PR terrain keeps room art consistent with the island.
            // Everything is temporary and on a dedicated layer, so this cannot alter the scene.
            var root = Group("Room 1 background render", null);
            var previousActive = RenderTexture.active;
            var lights = Object.FindObjectsByType<Light>();
            var lightMasks = lights.Select(light => light.cullingMask).ToArray();
            RenderTexture target = null; Texture2D pixels = null;
            try
            {
                foreach (var existingLight in lights) existingLight.cullingMask &= ~(1 << 30);
                for (int x = -5; x <= 5; x += 2) for (int z = -3; z <= 5; z += 2)
                    Part("Terrain_WorkshopFloor_2x2", root, new Vector3(x, 0, z));
                for (int x = -5; x <= 5; x += 2)
                {
                    var wall = Part("Terrain_WorkshopWall_2m", root, new Vector3(x, 0, 5.8f));
                    wall.transform.localScale = new Vector3(1, 1.8f, 1);
                }
                foreach (int side in new[] { -1, 1 }) for (int z = -1; z <= 5; z += 2)
                {
                    var wall = Part("Terrain_WorkshopWall_2m", root, new Vector3(side * 6, 0, z), false, 90);
                    wall.transform.localScale = new Vector3(1, 1.8f, 1);
                }
                var furnishing = Group("Back workstations", root); furnishing.localPosition = new Vector3(-2.5f, .02f, 2.05f);
                Workshop(furnishing, true);
                var oak = Mat("Oak edges", "DEC18C"); var teal = Mat("Workshop teal", "426F67");
                Shape("Wainscot", root, new Vector3(0, .65f, 5.66f), new Vector3(12, 1.3f, .09f), teal);
                Shape("Wall rail", root, new Vector3(0, 1.34f, 5.58f), new Vector3(12, .08f, .13f), oak);
                Shape("Notice board", root, new Vector3(2.65f, 2.5f, 5.5f), new Vector3(2.8f, 1.6f, .16f), oak);
                Shape("Green board", root, new Vector3(2.65f, 2.5f, 5.39f), new Vector3(2.6f, 1.4f, .06f), teal);
                // Small paper notes with no words; the language puzzle stays in the game UI.
                for (int i = 0; i < 3; i++)
                {
                    var note = Shape("Pinned blank note", root, new Vector3(1.85f + i * .75f, 2.5f + (i % 2) * .18f, 5.33f), new Vector3(.5f, .57f, .025f), Mat("Paper", "FFF1CD"));
                    note.transform.localRotation = Quaternion.Euler(0, 0, i * 9 - 8);
                }
                for (int i = 0; i < 3; i++)
                {
                    Shape("Side shelf", root, new Vector3(4.85f, .6f + i * .8f, 4.7f), new Vector3(1.4f, .11f, .65f), oak);
                    Crate(root, new Vector3(4.65f, .68f + i * .8f, 4.75f), .38f);
                }
                Pot(root, new Vector3(4.9f, 0, 2), 1.6f);
                Crate(root, new Vector3(-5, .03f, 1), .9f); Crate(root, new Vector3(-5, .95f, 1.05f), .65f);
                var camera = new GameObject("Artwork camera").AddComponent<Camera>(); camera.transform.SetParent(root, false);
                camera.transform.localPosition = new Vector3(0, 7.8f, -11.6f); camera.transform.LookAt(new Vector3(0, 1, 1.7f));
                camera.orthographic = true; camera.orthographicSize = 3.35f; camera.aspect = 16f / 9;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color("EFE6CC");
                camera.allowHDR = false; camera.allowMSAA = true; camera.cullingMask = 1 << 30;
                var light = new GameObject("Workshop softbox").AddComponent<Light>(); light.transform.SetParent(root, false);
                light.type = LightType.Directional; light.color = Color("FFF0D5"); light.intensity = .65f;
                light.shadows = LightShadows.Soft; light.shadowStrength = .4f; light.cullingMask = 1 << 30;
                light.transform.localRotation = Quaternion.Euler(48, -30, 0);
                foreach (var t in root.GetComponentsInChildren<Transform>()) t.gameObject.layer = 30;
                root.position = new Vector3(500, 0, 0);
                target = new RenderTexture(1600, 900, 24) { antiAliasing = 4 };
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                pixels = new Texture2D(1600, 900, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); pixels.Apply();
                File.WriteAllBytes(Output + "RoomOne_CloudWorkshop.png", pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                for (int i = 0; i < lights.Length; i++) if (lights[i]) lights[i].cullingMask = lightMasks[i];
                Object.DestroyImmediate(root.gameObject);
                if (target) { target.Release(); Object.DestroyImmediate(target); }
                if (pixels) Object.DestroyImmediate(pixels);
            }
            AssetDatabase.ImportAsset(Output + "RoomOne_CloudWorkshop.png", ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(Output + "RoomOne_CloudWorkshop.png");
            importer.filterMode = FilterMode.Bilinear; importer.SaveAndReimport();
        }
    }
}
