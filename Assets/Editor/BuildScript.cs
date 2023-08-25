
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    public static void PerformBuildLinux()
    {
        PerformBuild(BuildTarget.StandaloneLinux64);
    }

    public static void PerformBuildWindows()
    {
        PerformBuild(BuildTarget.StandaloneWindows64, ".exe");
    }

    public static void PerformBuildMacOS()
    {
        PerformBuild(BuildTarget.StandaloneOSX);
    }

    public static void PerformBuildAndroid()
    {
        PerformBuild(BuildTarget.Android, ".apk");
    }

    public static void PerformBuildIOS()
    {
        PerformBuild(BuildTarget.iOS, ".ipa");
    }

    private static void PerformBuild(BuildTarget target, string ext = "")
    {
        BuildPlayerOptions options = new BuildPlayerOptions();

        options.scenes = new[]
        {
            "Assets/UnityVoiceProcessor/Demo/VoiceProcessorDemo.unity",
        };
        options.target = target;
        options.locationPathName = "Build/demo" + ext;

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            throw new Exception("Build failed");
        }
    }
}

