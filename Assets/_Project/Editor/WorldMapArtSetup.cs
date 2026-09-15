using System;
using System.IO;
using System.Linq;
using Hackathon.Core;
using Hackathon.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Hackathon.Editor.IslandArtBuilder;
using Object = UnityEngine.Object;

namespace Hackathon.Editor
{
    public static class WorldMapArtSetup
    {
        const string ArtRoot = "World Map Art";

        [MenuItem("Tools/World Map/Refresh Island Artwork")]
        public static void Refresh()
        {
            if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Stop Play mode and save your scene before refreshing artwork.");
            var scene = SceneManager.GetActiveScene();
            if (scene.path != WorldNavigation.MapScene) scene = EditorSceneManager.OpenScene(WorldNavigation.MapScene);
            // Retain a recoverable, uniquely named copy before editing the saved scene.
            Directory.CreateDirectory("Temp/ArtBackups");
            File.Copy(WorldNavigation.MapScene, "Temp/ArtBackups/WorldMap-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".unity");
            var previous = GameObject.Find(ArtRoot); if (previous) Object.DestroyImmediate(previous);
            var root = Group(ArtRoot, null);
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Tree trunk" || go.name == "Tree canopy" || go.name.StartsWith("Workshop stepping stone") ||
                    go.name.StartsWith("Meadow stepping stone") || go.name.StartsWith("Workshop left post") ||
                    go.name.StartsWith("Workshop right post") || go.name == "Workshop lintel" || go.name == "Workshop crate") go.SetActive(false);
            }
            var water = GameObject.Find("Quiet sea");
            var waterMat = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Ocean.mat");
            if (!waterMat) { waterMat = new Material(Shader.Find("EnglishWorld/IslandWater")); AssetDatabase.CreateAsset(waterMat, Root + "/Ocean.mat"); }
            water.GetComponent<Renderer>().sharedMaterial = waterMat;
            water.transform.position = new Vector3(0, -1.35f, 0);

            var workshop = Group("Furnished workshop", root); workshop.position = new Vector3(-4, .04f, .3f);
            Workshop(workshop, false);
            var path = Mat("Path limestone", "ECD8AC");
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                Rock("Workshop path", root, Vector3.Lerp(new Vector3(-.5f, .05f, -2.65f), new Vector3(-4, .05f, -.3f), t), new Vector3(.64f, .12f, .55f), path).transform.localRotation = Quaternion.Euler(0, i * 21, 0);
            }
            for (int i = 0; i < 8; i++)
                Rock("Meadow path", root, Vector3.Lerp(new Vector3(0, .05f, -2.65f), new Vector3(4, .05f, .15f), i / 7f), new Vector3(.63f, .12f, .54f), path);

            var terrain = GameObject.Find("Terrain from PR 2").transform;
            foreach (Transform t in terrain)
            {
                if (t.name == "Terrain_GrassRound_4m") t.gameObject.SetActive(false);
                if (t.name == "Backdrop_AmberMesas") { t.position = new Vector3(-9, -1.35f, 9); t.localScale = Vector3.one * .55f; }
                if (t.name == "Backdrop_CanyonArch") { t.position = new Vector3(0, -1.35f, 12); t.localScale = Vector3.one * .6f; }
                if (t.name == "Backdrop_IceTerraces") { t.position = new Vector3(9, -1.35f, 10); t.localScale = Vector3.one * .55f; }
            }
            Part("Terrain_GrassRound_4m", root, new Vector3(4, 1.0f, 3.5f), true);
            Part("Terrain_GrassRamp_2x4_Rise1", root, new Vector3(4, .02f, .5f), true);
            Part("Terrain_GrassSteps_2x2_Rise1", root, new Vector3(1.1f, .02f, 3.5f), true, 90);
            // Round pieces and submerged shelves break up the old rectangular shoreline.
            foreach (var p in new[] { new Vector3(-7.3f, -.12f, -4.7f), new Vector3(-7.4f, -.16f, 4.7f), new Vector3(6.8f, -.1f, 4.8f) })
                Part("Terrain_GrassOuterCorner_2x2", root, p, false, p.x > 0 ? 0 : 90);
            for (int i = 0; i < 3; i++)
                Part("Terrain_ShoreSlope_2x2_Drop05", root, new Vector3(4 + i * 1.5f, -.28f, -5.65f), false);
            Part("Terrain_SandCliff_2x2", root, new Vector3(7.2f, -.5f, -4.7f));

            Vector3[] trees = {
                new Vector3(-7,0,3.7f), new Vector3(-6.8f,0,1.3f), new Vector3(-7,0,-3.7f),
                new Vector3(-6.3f,0,-4.7f), new Vector3(-.5f,0,4.8f), new Vector3(1,0,5),
                new Vector3(6.7f,0,4.7f), new Vector3(7,0,2.7f), new Vector3(6.7f,0,.7f),
                new Vector3(4.7f,1,4.1f), new Vector3(3.5f,1,4.4f)
            };
            for (int i = 0; i < trees.Length; i++) Tree(root, trees[i], .62f + (i % 3) * .13f, i);
            var rock = Mat("Coastal stone", "78958E"); var shrub = Mat("Meadow shrubs", "86AA69");
            var random = new System.Random(915);
            for (int i = 0; i < 40; i++)
            {
                bool north = i % 2 == 0;
                float x = -7.2f + (float)random.NextDouble() * 14.1f;
                float z = north ? 4.6f + (float)random.NextDouble() * .7f : -4.3f - (float)random.NextDouble() * .7f;
                if (x > 3 && !north) continue;
                float size = .35f + (float)random.NextDouble() * .48f;
                Rock(i % 3 == 0 ? "Shore rock" : "Low shrub", root, new Vector3(x, size * .25f, z), new Vector3(size * 1.6f, size, size), i % 3 == 0 ? rock : shrub);
            }
            for (int i = 0; i < 28; i++)
            {
                float x = i < 14 ? -5.4f + (i % 7) * .36f : 3.2f + (i % 7) * .4f;
                float z = i < 14 ? -2.5f - (i / 7) * .42f : -1.8f - (i % 2) * .4f;
                var p = new Vector3(x, .12f, z);
                Shape("Flower stalk", root, p, new Vector3(.025f, .22f, .025f), shrub);
                Rock("Wildflower", root, p + Vector3.up * .14f, new Vector3(.16f, .1f, .16f), Mat(i % 3 == 0 ? "Lavender" : "Buttercream", i % 3 == 0 ? "BBAAD4" : "FFF1BA"));
            }
            // A small jetty and shoreline furniture give the beach a destination.
            var jetty = Group("Shore jetty", root); jetty.localPosition = new Vector3(6, -.15f, -4.5f);
            for (int i = 0; i < 8; i++) Shape("Jetty plank", jetty, new Vector3(0, 0, -i * .24f), new Vector3(1.2f, .13f, .21f), Mat("Warm oak", "B98B58"));
            foreach (int side in new[] { -1, 1 }) foreach (float z in new[] { .15f, -1.65f })
                Shape("Jetty mooring", jetty, new Vector3(side * .55f, -.15f, z), new Vector3(.13f, .65f, .13f), Mat("Bark", "866549"), PrimitiveType.Cylinder);
            Lantern(jetty, new Vector3(-.48f, .52f, .15f));
            Crate(root, new Vector3(5.3f, .04f, -3.7f), .48f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                Rock("Sea stack", root, new Vector3(Mathf.Cos(angle) * 10.5f, -1.0f, Mathf.Sin(angle) * 8), new Vector3(1.0f + i % 2, .7f, .8f), rock);
            }
            for (int i = 0; i < 6; i++)
                Rock("Beach pebble", root, new Vector3(7.1f + .14f * (i % 2), .06f, -2.9f - i * .24f), new Vector3(.35f, .23f, .29f), rock);
            var map = Object.FindAnyObjectByType<WorldMapController>();
            map.mapCamera.orthographicSize = 9.4f;
            map.mapCamera.transform.position = new Vector3(12, 17, -19);
            map.mapCamera.transform.LookAt(new Vector3(0, .1f, 1.6f));
            map.mapCamera.backgroundColor = Color("9DCEC5");
            map.mapCamera.allowHDR = false;
            var sun = GameObject.Find("Afternoon sun").GetComponent<Light>();
            sun.intensity = .8f; sun.color = Color("FFF0D5"); sun.shadows = LightShadows.Soft; sun.shadowStrength = .7f;
            sun.transform.rotation = Quaternion.Euler(48, -32, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color("6B807F"); RenderSettings.ambientEquatorColor = Color("515F57"); RenderSettings.ambientGroundColor = Color("38332E");
            RenderSettings.fog = false;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Map artwork could not be saved.");
            AssetDatabase.SaveAssets();
            Debug.Log("[WorldMap] Rich island artwork saved with PR terrain, furnished workshop, meadow terrace and coast.");
        }
    }
}
