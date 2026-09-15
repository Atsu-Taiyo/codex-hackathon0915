using System;
using System.IO;
using Hackathon.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hackathon.Editor
{
    public static class ProjectSetup
    {
        public static void CreateBootstrapScene()
        {
            const string path = "Assets/_Project/Scenes/Bootstrap.unity";
            if (File.Exists(path))
                throw new InvalidOperationException("Bootstrap scene already exists; refusing to overwrite.");

            Directory.CreateDirectory("Assets/_Project/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            new GameObject("AppBootstrap").AddComponent<AppBootstrap>();
            if (!EditorSceneManager.SaveScene(scene, path))
                throw new InvalidOperationException("Could not save Bootstrap scene.");

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[Hackathon] Bootstrap scene created and enabled in build settings.");
        }
    }
}
