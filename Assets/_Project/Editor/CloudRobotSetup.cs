using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hackathon.Editor
{
    // Run Tools > Cloud Robot > Create Prefab after the asset import completes.
    // No package dependencies. All created assets stay beside this script's model.
    public static class CloudRobotSetup
    {
        [MenuItem("Tools/Cloud Robot/Create Prefab")]
        public static void CreatePrefab()
        {
            string modelPath = null;
            foreach (string guid in AssetDatabase.FindAssets("CloudRobot_Terminal t:Model"))
            {
                string candidate = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(candidate) == "CloudRobot_Terminal.fbx")
                {
                    modelPath = candidate;
                    break;
                }
            }
            if (modelPath == null)
                throw new InvalidOperationException("Copy the complete CloudRobot folder into Assets first.");

            string modelFolder = Path.GetDirectoryName(modelPath).Replace('\\', '/');
            string folder = Path.GetDirectoryName(modelFolder).Replace('\\', '/');
            string textures = modelFolder + "/tripo_convert_ed1accbd-4268-4f25-8db6-660239cea543.fbm/";
            string albedoPath = textures + "CloudRobot_Terminal_basecolor.JPEG";
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            if (albedo == null)
                throw new InvalidOperationException("The base-color texture is missing: " + albedoPath);

            var pipeline = GraphicsSettings.currentRenderPipeline;
            string pipelineType = pipeline == null ? "" : pipeline.GetType().Name;
            string shaderName = pipeline == null ? "Standard"
                : pipelineType.IndexOf("HDRender", StringComparison.OrdinalIgnoreCase) >= 0
                    ? "HDRP/Lit" : "Universal Render Pipeline/Lit";
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
                throw new InvalidOperationException("Shader unavailable for this pipeline: " + shaderName);

            var material = new Material(shader) { name = "CloudRobot" };
            foreach (string property in new[] { "_MainTex", "_BaseMap", "_BaseColorMap" })
                if (material.HasProperty(property)) material.SetTexture(property, albedo);
            foreach (string property in new[] { "_Color", "_BaseColor" })
                if (material.HasProperty(property)) material.SetColor(property, Color.white);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.25f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.25f);

            // Base color is enough to preserve the face. Original PBR maps are supplied
            // separately for users who want to tune the surface for their own pipeline.
            if (!AssetDatabase.IsValidFolder(folder + "/Materials"))
                AssetDatabase.CreateFolder(folder, "Materials");
            if (!AssetDatabase.IsValidFolder(folder + "/Prefabs"))
                AssetDatabase.CreateFolder(folder, "Prefabs");
            string matPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/Materials/RoomOne_CloudRobot.mat");
            AssetDatabase.CreateAsset(material, matPath);
            var root = new GameObject("CloudRobot");
            try
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                var visual = UnityEngine.Object.Instantiate(model, root.transform, false);
                visual.name = "Visual";
                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) throw new InvalidOperationException("No renderable mesh found.");
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++) materials[i] = material;
                    renderer.sharedMaterials = materials;
                }
                Bounds bounds = CombinedBounds(renderers);
                if (bounds.size.y <= 0.000001f) throw new InvalidOperationException("Invalid model bounds.");
                visual.transform.localScale *= 1f / bounds.size.y;
                bounds = CombinedBounds(renderers);
                visual.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

                string prefabPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/Prefabs/RoomOne_CloudRobot.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created " + prefabPath + ". Drag it into your scene. Height: 1 Unity unit; pivot: ground center.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
