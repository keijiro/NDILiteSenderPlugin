using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class PbxModifier
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "/NewTek\\ NDI\\ SDK/include");
            proj.AddBuildProperty(target, "LIBRARY_SEARCH_PATHS", "/NewTek\\ NDI\\ SDK/lib/iOS");
            proj.AddFrameworkToProject(target, "libndi_ios.a", false);

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
