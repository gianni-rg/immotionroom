using System;
using System.Linq;
using UnityEditor;

// See: http://answers.unity3d.com/questions/790568/how-to-create-developmentdebug-build-with-command.html
// See: http://forum.unity3d.com/threads/windows-build-outputs-pdb-files.330815/

public class Builder
{
    public static void BuildAndroid()
    {
        Build(BuildTarget.Android);
    }

    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    private static void Build(BuildTarget target, bool debug = false)
    {
        BuildOptions opts = BuildOptions.None;
        if (debug)
        {
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.connectProfiler = true;
            opts |= BuildOptions.Development;
        }

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.connectProfiler = false;

        var scenes = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
        BuildPipeline.BuildPlayer(scenes.ToArray(), Environment.GetCommandLineArgs().Last(), target, opts);
    }
}
