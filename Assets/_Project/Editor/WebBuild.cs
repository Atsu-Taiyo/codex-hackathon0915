using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Hackathon.Editor
{
    public static class WebBuild
    {
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled build scenes");
            // Static hosting does not need Content-Encoding headers with decompression fallback.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                target = BuildTarget.WebGL,
                locationPathName = "Builds/WebGL",
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("WebGL build failed: " + report.summary.result);
        }
    }
}
