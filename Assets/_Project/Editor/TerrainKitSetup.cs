using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hackathon.Editor
{
    /// <summary>Creates individual terrain prefabs; never assembles or modifies a scene.</summary>
    public static class TerrainKitSetup
    {
        private const string Root = "Assets/_Project/Content/Common/TerrainKit";
        private static readonly Dictionary<string, string> Palette = new Dictionary<string, string>
        {
            {"Terrain_Grass", "86BD45"}, {"Terrain_GrassEdge", "639A38"},
            {"Terrain_Sand", "E8C889"}, {"Terrain_Soil", "C6955C"},
            {"Terrain_SoilLight", "D8AE72"}, {"Terrain_SoilDeep", "AE7B49"},
            {"Terrain_Ice", "C5E9E6"}, {"Terrain_IceRock", "87ACA9"},
            {"Terrain_Amber", "CB8863"}, {"Terrain_AmberLight", "DEA579"},
            {"Terrain_Studio", "F3EEE4"}, {"Terrain_Ink", "374A48"},
            {"Terrain_Cream", "FFFAEB"}, {"Terrain_Honey", "DCAD6B"},
            {"Terrain_HoneyLight", "E9C28B"}, {"Terrain_WoodEdge", "BD8C50"}
        };

        [MenuItem("Tools/Terrain Kit/Create Individual Prefabs")]
        public static void CreatePrefabs()
        {
            if (!AssetDatabase.IsValidFolder(Root + "/Models"))
                throw new InvalidOperationException("Copy the supplied Assets folder into the Unity project first.");
            Shader shader = Shader.Find(GraphicsSettings.currentRenderPipeline == null
                ? "Standard" : "Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("This helper supports Built-in and URP. Configure materials manually for other pipelines.");
            Directory.CreateDirectory(Root + "/Materials");
            Directory.CreateDirectory(Root + "/Prefabs");
            AssetDatabase.Refresh();
            var materials = new Dictionary<string, Material>();
            foreach (var entry in Palette)
            {
                string path = Root + "/Materials/" + entry.Key + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    if (!ColorUtility.TryParseHtmlString("#" + entry.Value, out Color color))
                        throw new InvalidOperationException("Invalid palette color.");
                    material = new Material(shader) { name = entry.Key, color = color };
                    if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                    if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.2f);
                    if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.2f);
                    if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
                    AssetDatabase.CreateAsset(material, path);
                }
                materials.Add(entry.Key, material);
            }

            int created = 0, skipped = 0;
            foreach (string pathIn in Directory.GetFiles(Root + "/Models", "*.fbx"))
            {
                string path = pathIn.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(path);
                string prefabPath = Root + "/Prefabs/" + name + ".prefab";
                // Preserve user edits on subsequent runs.
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) { skipped++; continue; }
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) throw new InvalidOperationException("No model importer for " + path);
                importer.globalScale = 1f;
                importer.useFileUnits = true;
                importer.bakeAxisConversion = true;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.addCollider = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.importNormals = ModelImporterNormals.Import;
                importer.SaveAndReimport();
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) throw new InvalidOperationException("Model import failed: " + path);
                GameObject instance = UnityEngine.Object.Instantiate(model);
                instance.name = name;
                instance.transform.position = Vector3.zero;
                try
                {
                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        Material[] slots = renderer.sharedMaterials;
                        for (int i = 0; i < slots.Length; i++)
                        {
                            if (slots[i] == null) throw new InvalidOperationException("Missing material in " + name);
                            string key = slots[i].name.Replace(" (Instance)", "");
                            if (!materials.TryGetValue(key, out Material material))
                                throw new InvalidOperationException("Unknown material " + key + " in " + name);
                            slots[i] = material;
                        }
                        renderer.sharedMaterials = slots;
                    }
                    if (!name.StartsWith("Backdrop_", StringComparison.Ordinal))
                    {
                        foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
                        {
                            var collider = filter.gameObject.AddComponent<MeshCollider>();
                            collider.sharedMesh = filter.sharedMesh;
                            collider.convex = false;
                        }
                    }
                    if (PrefabUtility.SaveAsPrefabAsset(instance, prefabPath) == null)
                        throw new InvalidOperationException("Could not save " + prefabPath);
                    created++;
                }
                finally { UnityEngine.Object.DestroyImmediate(instance); }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Terrain Kit: {created} prefabs created, {skipped} existing prefabs preserved. No scene was modified.");
        }
    }
}
