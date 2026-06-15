using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace ARUnity.Editor
{
    public static class IOSPostBuildProcessor
    {
        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;

#if UNITY_IOS
            PatchInfoPlist(buildPath);
            PatchXcodeProject(buildPath);
#endif
        }

#if UNITY_IOS
        private static void PatchInfoPlist(string buildPath)
        {
            var plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var root = plist.root;
            root.SetString("NSCameraUsageDescription",
                "This app uses your camera to provide augmented reality experiences.");
            root.SetBoolean("UIRequiresFullScreen", true);

            plist.WriteToFile(plistPath);
        }

        private static void PatchXcodeProject(string buildPath)
        {
            var projPath = PBXProject.GetPBXProjectPath(buildPath);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            var targetGuid = proj.GetUnityMainTargetGuid();

            // ARKit requires these frameworks
            proj.AddFrameworkToProject(targetGuid, "ARKit.framework", false);
            proj.AddFrameworkToProject(targetGuid, "AVFoundation.framework", false);

            proj.WriteToFile(projPath);
        }
#endif
    }
}
