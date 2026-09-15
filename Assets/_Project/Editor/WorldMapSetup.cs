using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hackathon.Core;
using Hackathon.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Editor
{
    public static class WorldMapSetup
    {
        const string Terrain = "Assets/_Project/Content/Common/TerrainKit/Prefabs/";
        const string Materials = "Assets/_Project/Content/Features/Map/WorldMaterials";

        [MenuItem("Tools/World Map/Create Map and Connect Room 1")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before creating the map.");
            if (File.Exists(WorldNavigation.MapScene)) throw new InvalidOperationException("WorldMap already exists; preserving scene edits.");
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(CodexRoboBuilder.PrefabPath))
                throw new InvalidOperationException("Create the Codex Robo walking prefab first.");
            TerrainKitSetup.CreatePrefabs();
            Directory.CreateDirectory(Materials);
            AssetDatabase.Refresh();
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(.42f, .48f, .42f);
                RenderSettings.fog = false;
                var water = Material("Water", "7EBCBA");
                var stone = Material("Path", "F1E1B6");
                var wood = Material("Timber", "875B35");
                var leaf = Material("Leaves", "4F8750");
                var paleLeaf = Material("LightLeaves", "A5C461");
                var roof = Material("WorkshopSign", "487366");
                Box("Quiet sea", new Vector3(0, -1.0f, 0), new Vector3(160, .1f, 160), water, false);
                var land = new GameObject("Terrain from PR 2").transform;
                for (int x = -6; x <= 6; x += 4)
                for (int z = -4; z <= 4; z += 4)
                {
                    if (x == 6 && z == -4) continue;
                    Part("Terrain_GrassFloor_4x4", new Vector3(x, 0, z), land, true);
                }
                for (int x = 5; x <= 7; x += 2)
                for (int z = -5; z <= -3; z += 2)
                    Part("Terrain_SandTile_2x2", new Vector3(x, 0, z), land, true);
                // Cliff faces hide the tiled undersides and give the island a readable edge.
                for (int x = -7; x <= 7; x += 2)
                {
                    Part("Terrain_GrassCliff_2x2", new Vector3(x, -.04f, 5), land, false);
                    if (x < 5) Part("Terrain_GrassCliff_2x2", new Vector3(x, -.04f, -5), land, false, 180);
                }
                for (int z = -3; z <= 3; z += 2)
                {
                    Part("Terrain_GrassCliff_2x2", new Vector3(-7, -.04f, z), land, false, -90);
                    Part("Terrain_GrassCliff_2x2", new Vector3(7, -.04f, z), land, false, 90);
                }
                // Invisible rails keep click-to-walk destinations inside the connected island.
                Barrier("North coast", new Vector3(0, 1, 6), new Vector3(17, 3, .15f));
                Barrier("South coast", new Vector3(0, 1, -6), new Vector3(17, 3, .15f));
                Barrier("West coast", new Vector3(-8, 1, 0), new Vector3(.15f, 3, 12));
                Barrier("East coast", new Vector3(8, 1, 0), new Vector3(.15f, 3, 12));

                var workshop = new GameObject("Room 01 - The workshop").transform;
                foreach (int x in new[] { -5, -3 })
                foreach (int z in new[] { 1, 3 })
                    Part("Terrain_WorkshopFloor_2x2", new Vector3(x, .035f, z), workshop, true);
                foreach (int x in new[] { -5, -3 })
                    Part("Terrain_WorkshopWall_2m", new Vector3(x, .035f, 4), workshop, false);
                Part("Terrain_WorkshopWall_2m", new Vector3(-6, .035f, 3), workshop, false, 90);
                Box("Workshop left post", new Vector3(-5.3f, .85f, .45f), new Vector3(.18f, 1.7f, .18f), wood, true);
                Box("Workshop right post", new Vector3(-2.7f, .85f, .45f), new Vector3(.18f, 1.7f, .18f), wood, true);
                Box("Workshop lintel", new Vector3(-4, 1.65f, .45f), new Vector3(2.9f, .28f, .35f), roof, true);
                Box("Workshop crate", new Vector3(-4.9f, .39f, 2.6f), new Vector3(.65f, .7f, .65f), wood, true);
                for (int i = 0; i < 6; i++)
                    Box("Workshop stepping stone " + i, new Vector3(-4 + i * .8f, .035f, -1.0f - i * .38f), new Vector3(.66f, .055f, .57f), stone, false);
                for (int i = 0; i < 5; i++)
                    Box("Meadow stepping stone " + i, new Vector3(.5f + i * .7f, .035f, -2.45f + i * .7f), new Vector3(.60f, .055f, .55f), stone, false);

                Part("Terrain_GrassRound_4m", new Vector3(4, .01f, 2), land, true);
                Box("Meadow left post", new Vector3(3, .75f, 1.2f), new Vector3(.20f, 1.5f, .20f), wood, true);
                Box("Meadow right post", new Vector3(5, .75f, 1.2f), new Vector3(.20f, 1.5f, .20f), wood, true);
                Box("Meadow closed gate", new Vector3(4, .65f, 1.2f), new Vector3(1.8f, .28f, .15f), wood, true);
                foreach (var p in new[] { new Vector3(-7, 0, 3.9f), new Vector3(-.5f, 0, 4.5f), new Vector3(6.5f, 0, 4.5f), new Vector3(-6.8f, 0, -3.6f) })
                {
                    Box("Tree trunk", p + Vector3.up * .5f, new Vector3(.22f, 1, .22f), wood, true);
                    var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    crown.name = "Tree canopy"; crown.transform.position = p + Vector3.up * 1.55f;
                    crown.transform.localScale = new Vector3(1.35f, 1.65f, 1.3f);
                    crown.GetComponent<Renderer>().sharedMaterial = p.x < 0 ? leaf : paleLeaf;
                    UnityEngine.Object.DestroyImmediate(crown.GetComponent<Collider>());
                }
                Part("Backdrop_AmberMesas", new Vector3(-17, -1, 18), land, false);
                Part("Backdrop_CanyonArch", new Vector3(0, -1, 23), land, false);
                Part("Backdrop_IceTerraces", new Vector3(17, -1, 22), land, false);

                var robo = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(CodexRoboBuilder.PrefabPath), scene);
                robo.transform.position = new Vector3(-1, .08f, -2.6f);
                robo.transform.rotation = Quaternion.Euler(0, 180, 0);
                var motor = robo.GetComponent<CodexRoboMotor>(); motor.walkSpeed = 2.7f;
                var camera = new GameObject("World Map Camera").AddComponent<Camera>();
                camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 9.2f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.64f, .81f, .78f);
                camera.nearClipPlane = .1f; camera.farClipPlane = 150;
                camera.transform.position = new Vector3(12, 17, -19);
                camera.transform.LookAt(new Vector3(0, 0, 1));
                camera.gameObject.AddComponent<AudioListener>(); motor.viewCamera = camera;
                var sun = new GameObject("Afternoon sun").AddComponent<Light>();
                sun.type = LightType.Directional; sun.color = new Color(1, .96f, .9f); sun.intensity = .85f;
                sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(48, -35, 0);
                var map = new GameObject("World Map").AddComponent<WorldMapController>();
                map.robo = motor; map.mapCamera = camera;
                map.workshopEntrance = new GameObject("Workshop arrival").transform; map.workshopEntrance.position = new Vector3(-4, .06f, -.45f);
                map.meadowEntrance = new GameObject("Meadow arrival").transform; map.meadowEntrance.position = new Vector3(4, .06f, .35f);
                if (!EditorSceneManager.SaveScene(scene, WorldNavigation.MapScene)) throw new IOException("Could not save WorldMap.");
                ConfigureBuildScenes();
                AssetDatabase.SaveAssets();
                Debug.Log("[WorldMap] Created terrain map and Room 1 navigation.");
            }
            finally
            {
                SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        public static void ConfigureBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene> {
                new EditorBuildSettingsScene(WorldNavigation.MapScene, true),
                new EditorBuildSettingsScene(WorldNavigation.RoomOneScene, true)
            };
            scenes.AddRange(EditorBuildSettings.scenes.Where(s => s.path != WorldNavigation.MapScene && s.path != WorldNavigation.RoomOneScene));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static Material Material(string name, string hex)
        {
            string path = Materials + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material) return material;
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            material = new Material(Shader.Find("Standard")) { name = name, color = color };
            material.SetFloat("_Glossiness", .12f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static GameObject Part(string name, Vector3 position, Transform parent, bool walkable, float rotation = 0)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Terrain + name + ".prefab");
            if (!source) throw new InvalidOperationException("Missing terrain prefab " + name);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, rotation, 0));
            if (walkable) foreach (var t in go.GetComponentsInChildren<Transform>()) t.gameObject.layer = 8;
            return go;
        }

        static void Box(string name, Vector3 position, Vector3 scale, Material material, bool collider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static void Barrier(string name, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name); go.transform.position = position;
            go.AddComponent<BoxCollider>().size = scale;
        }
    }
}
