using System;
using System.IO;
using Hackathon.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hackathon.Editor
{
    /// <summary>Run in a separate project copy: -batchmode -executeMethod Hackathon.Editor.CodexRoboBatchVerify.Run.</summary>
    [InitializeOnLoad]
    public static class CodexRoboBatchVerify
    {
        static double started;
        static CodexRoboBatchVerify()
        {
            if (!Application.isBatchMode) return;
            EditorApplication.playModeStateChanged += state =>
            {
                if(state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("CodexRoboVerify",false))
                {
                    started=EditorApplication.timeSinceStartup;
                    UnityEngine.Object.FindAnyObjectByType<CodexRoboMotor>().StartCoroutine(CodexRoboPlaytest.Run());
                    EditorApplication.update += Wait;
                }
            };
        }
        public static void BuildAndRun() { CodexRoboBuilder.Build(); Run(); }
        public static void Run()
        {
            Directory.CreateDirectory("Temp");
            if(File.Exists("Temp/CodexRobo-playtest.txt")) File.Delete("Temp/CodexRobo-playtest.txt");
            EditorSceneManager.OpenScene(CodexRoboBuilder.ScenePath);
            SessionState.SetBool("CodexRoboVerify",true); EditorApplication.isPlaying=true;
        }
        static void Wait()
        {
            if(File.Exists("Temp/CodexRobo-playtest.txt"))
            {
                bool success=File.ReadAllText("Temp/CodexRobo-playtest.txt").StartsWith("PASS");
                Directory.CreateDirectory("Verification");
                foreach(var file in Directory.GetFiles("Temp","CodexRobo-*")) File.Copy(file,Path.Combine("Verification",Path.GetFileName(file)),true);
                SessionState.SetBool("CodexRoboVerify",false); EditorApplication.Exit(success?0:1);
            }
            if(EditorApplication.timeSinceStartup-started>90) {Debug.LogError("Codex Robo playtest timed out");EditorApplication.Exit(2);}
        }
    }
}
