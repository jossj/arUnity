using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using ARUnity.AR;

namespace ARUnity.Editor
{
    public static class ImageTrackingBuilder
    {
        public const string LibraryPath = "Assets/Art/ReferenceImages/TrackingLibrary.asset";
        public const string OverlayPrefabPath = "Assets/Prefabs/AR/Overlays/ImageOverlayDefault.prefab";

        private const string PosterAPath = "Assets/Art/ReferenceImages/PosterA.png";
        private const string PosterBPath = "Assets/Art/ReferenceImages/PosterB.png";

        [MenuItem("ARUnity/Build Assets/Build Image Tracking Assets")]
        public static void BuildImageTrackingAssets()
        {
            EnsureFolders();

            var texA = EnsureSampleTexture(PosterAPath,
                new Color(0.85f, 0.15f, 0.15f), new Color(1f, 1f, 1f));
            var texB = EnsureSampleTexture(PosterBPath,
                new Color(0.15f, 0.25f, 0.85f), new Color(1f, 1f, 1f));

            EnsureReferenceImageLibrary(texA, texB);
            EnsureOverlayPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ARUnity] Image tracking assets ready in Assets/Art/ReferenceImages/ and Assets/Prefabs/AR/Overlays/");
        }

        // Called by ARSceneBuilder so assets exist before scene wiring.
        public static void EnsureAssetsExist()
        {
            var libMissing = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath) == null;
            var overlayMissing = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath) == null;
            if (libMissing || overlayMissing)
                BuildImageTrackingAssets();
        }

        // ── Reference Image Library ───────────────────────────────────────

        private static void EnsureReferenceImageLibrary(Texture2D texA, Texture2D texB)
        {
            var existing = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);
            if (existing != null) return;

            var library = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            library.name = "TrackingLibrary";
            AssetDatabase.CreateAsset(library, LibraryPath);

            // Use SerializedObject to populate the library's internal image list,
            // avoiding a dependency on the editor-only XRReferenceImageLibraryExtensions
            // whose assembly name varies across AR Foundation versions.
            var so = new SerializedObject(library);
            var imagesProp = so.FindProperty("m_Images");

            if (imagesProp == null)
            {
                Debug.LogWarning("[ARUnity] Could not find 'm_Images' on XRReferenceImageLibrary. " +
                    "Open Assets/Art/ReferenceImages/TrackingLibrary.asset and add images manually.");
                return;
            }

            AddImageEntry(imagesProp, 0, "PosterA", texA, new Vector2(0.15f, 0.10f));
            AddImageEntry(imagesProp, 1, "PosterB", texB, new Vector2(0.20f, 0.14f));

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
        }

        private static void AddImageEntry(SerializedProperty imagesProp, int index,
            string imageName, Texture2D texture, Vector2 physicalSizeMeters)
        {
            imagesProp.arraySize = Mathf.Max(imagesProp.arraySize, index + 1);
            var entry = imagesProp.GetArrayElementAtIndex(index);

            SetEntryField(entry, "m_Name", imageName);
            SetEntryField(entry, "m_SpecifySize", true);
            SetEntryField(entry, "m_Size", physicalSizeMeters);
            SetEntryField(entry, "m_Texture", texture);

            // Each reference image requires a stable GUID for ARCore/ARKit to identify it
            var guidProp = entry.FindPropertyRelative("m_SerializedGuid");
            if (guidProp != null)
                WriteGuid(guidProp, Guid.NewGuid());
        }

        private static void SetEntryField(SerializedProperty entry, string fieldName, string value)
        {
            var prop = entry.FindPropertyRelative(fieldName);
            if (prop != null) prop.stringValue = value;
        }

        private static void SetEntryField(SerializedProperty entry, string fieldName, bool value)
        {
            var prop = entry.FindPropertyRelative(fieldName);
            if (prop != null) prop.boolValue = value;
        }

        private static void SetEntryField(SerializedProperty entry, string fieldName, Vector2 value)
        {
            var prop = entry.FindPropertyRelative(fieldName);
            if (prop != null) prop.vector2Value = value;
        }

        private static void SetEntryField(SerializedProperty entry, string fieldName, UnityEngine.Object value)
        {
            var prop = entry.FindPropertyRelative(fieldName);
            if (prop != null) prop.objectReferenceValue = value;
        }

        private static void WriteGuid(SerializedProperty guidProp, Guid guid)
        {
            var bytes = guid.ToByteArray();
            var low = guidProp.FindPropertyRelative("m_GuidLow");
            var high = guidProp.FindPropertyRelative("m_GuidHigh");
            if (low != null) low.longValue = BitConverter.ToInt64(bytes, 0);
            if (high != null) high.longValue = BitConverter.ToInt64(bytes, 8);
        }

        // ── Placeholder textures ──────────────────────────────────────────

        private static Texture2D EnsureSampleTexture(string assetPath, Color primary, Color secondary)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (existing != null) return existing;

            const int size = 256;
            const int squareCount = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            int sq = size / squareCount;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool even = ((x / sq) + (y / sq)) % 2 == 0;
                    tex.SetPixel(x, y, even ? primary : secondary);
                }
            }
            tex.Apply();

            // Write as PNG so Unity can import it as a proper Texture2D asset
            var absPath = Path.Combine(Application.dataPath, "../", assetPath).Replace("\\", "/");
            var absPathNorm = Path.GetFullPath(absPath);
            File.WriteAllBytes(absPathNorm, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(assetPath);

            // Configure import settings: sRGB, no alpha, no mipmaps (image tracking textures)
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.mipmapEnabled = false;
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        // ── Overlay prefab ────────────────────────────────────────────────

        private static void EnsureOverlayPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath) != null) return;

            // Root — carries BillboardOverlay and ImageTrackingOverlayController
            var root = new GameObject("ImageOverlayDefault");
            root.AddComponent<BillboardOverlay>();
            var overlayCtrl = root.AddComponent<ImageTrackingOverlayController>();

            // World-space canvas — 300×160 units at 0.001 scale = 30cm × 16cm display
            var canvasGO = new GameObject("Canvas");
            canvasGO.transform.SetParent(root.transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var canvasRT = canvasGO.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(300f, 160f);
            canvasRT.localScale = Vector3.one * 0.001f;
            // Offset 5mm in front of the image plane so it doesn't z-fight
            canvasRT.localPosition = new Vector3(0f, 0f, -0.005f);

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Background panel
            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.72f);

            // Image name label (large, centre)
            var nameGO = new GameObject("Image Name");
            nameGO.transform.SetParent(canvasGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.45f);
            nameRT.anchorMax = new Vector2(0.95f, 0.85f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Image Name";
            nameTMP.fontSize = 42f;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;

            // Physical size label (small, below name)
            var sizeGO = new GameObject("Size Label");
            sizeGO.transform.SetParent(canvasGO.transform, false);
            var sizeRT = sizeGO.AddComponent<RectTransform>();
            sizeRT.anchorMin = new Vector2(0.05f, 0.2f);
            sizeRT.anchorMax = new Vector2(0.95f, 0.44f);
            sizeRT.offsetMin = Vector2.zero;
            sizeRT.offsetMax = Vector2.zero;
            var sizeTMP = sizeGO.AddComponent<TextMeshProUGUI>();
            sizeTMP.text = "0 × 0 cm";
            sizeTMP.fontSize = 24f;
            sizeTMP.alignment = TextAlignmentOptions.Center;
            sizeTMP.color = new Color(0.8f, 0.8f, 0.8f);

            // Tracking state label (small, bottom)
            var stateGO = new GameObject("State Label");
            stateGO.transform.SetParent(canvasGO.transform, false);
            var stateRT = stateGO.AddComponent<RectTransform>();
            stateRT.anchorMin = new Vector2(0.05f, 0.02f);
            stateRT.anchorMax = new Vector2(0.95f, 0.2f);
            stateRT.offsetMin = Vector2.zero;
            stateRT.offsetMax = Vector2.zero;
            var stateTMP = stateGO.AddComponent<TextMeshProUGUI>();
            stateTMP.text = "Tracking";
            stateTMP.fontSize = 18f;
            stateTMP.alignment = TextAlignmentOptions.Center;
            stateTMP.color = new Color(0.4f, 1f, 0.5f);

            // Wire ImageTrackingOverlayController references
            var ctrlSO = new SerializedObject(overlayCtrl);
            ctrlSO.FindProperty("_imageNameLabel").objectReferenceValue = nameTMP;
            ctrlSO.FindProperty("_imageSizeLabel").objectReferenceValue = sizeTMP;
            ctrlSO.FindProperty("_trackingStateLabel").objectReferenceValue = stateTMP;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, OverlayPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        // ── Folder setup ──────────────────────────────────────────────────

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "ReferenceImages");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "AR");
            EnsureFolder("Assets/Prefabs/AR", "Overlays");
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
