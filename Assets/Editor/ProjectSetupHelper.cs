using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;

namespace ARUnity.Editor
{
    /// <summary>
    /// One-time project setup steps that cannot be expressed in YAML assets alone.
    /// Run via ARUnity > Project Setup > … menu items after first opening the project.
    /// </summary>
    public static class ProjectSetupHelper
    {
        private const string RendererAssetPath =
            "Assets/Settings/UniversalRenderPipelineAsset_Renderer.asset";

        [MenuItem("ARUnity/Project Setup/Add AR Background Renderer Feature")]
        public static void AddARBackgroundFeature()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
            if (rendererData == null)
            {
                Debug.LogError(
                    "[ARUnity] UniversalRenderPipelineAsset_Renderer.asset not found. " +
                    "Ensure URP is configured via Edit > Project Settings > Graphics.");
                return;
            }

            // Check if the feature is already present
            foreach (var feature in rendererData.rendererFeatures)
            {
                if (feature is ARBackgroundRendererFeature)
                {
                    Debug.Log("[ARUnity] ARBackgroundRendererFeature is already added.");
                    return;
                }
            }

            var arFeature = ScriptableObject.CreateInstance<ARBackgroundRendererFeature>();
            arFeature.name = "AR Background";
            AssetDatabase.AddObjectToAsset(arFeature, rendererData);

            // Insert via SerializedObject to keep Unity's undo / dirty state happy
            var so = new SerializedObject(rendererData);
            var featuresProp = so.FindProperty("m_RendererFeatures");
            featuresProp.arraySize++;
            featuresProp.GetArrayElementAtIndex(featuresProp.arraySize - 1).objectReferenceValue = arFeature;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            Debug.Log("[ARUnity] ARBackgroundRendererFeature added to URP renderer.");
        }

        [MenuItem("ARUnity/Project Setup/Configure XR Plugin Management")]
        public static void ConfigureXRPluginManagement()
        {
            // Open XR Plugin Management settings so the user can enable ARCore / ARKit
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
            Debug.Log("[ARUnity] XR Plug-in Management settings opened. " +
                      "Enable ARCore (Android) and ARKit (iOS) in the tabs shown.");
        }

        [MenuItem("ARUnity/Project Setup/Run All Setup Steps")]
        public static void RunAllSetupSteps()
        {
            AddARBackgroundFeature();
            ConfigureXRPluginManagement();
            Debug.Log("[ARUnity] Setup complete. Build AR scenes via ARUnity > Build Scenes > Build All Scenes.");
        }
    }
}
