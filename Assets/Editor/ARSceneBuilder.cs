using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using ARUnity.AR;
using ARUnity.Core;
using ARUnity.UI;

namespace ARUnity.Editor
{
    public static class ARSceneBuilder
    {
        private const string ARScenePath = "Assets/Scenes/ARSession.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("ARUnity/Build Scenes/Build AR Session Scene")]
        public static void BuildARSessionScene()
        {
            EnsureScenesFolder();
            PrefabBuilder.EnsurePrefabsExist();
            ImageTrackingBuilder.EnsureAssetsExist();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildARSessionHierarchy();
            EditorSceneManager.SaveScene(scene, ARScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[ARUnity] AR Session scene saved to {ARScenePath}");
        }

        [MenuItem("ARUnity/Build Scenes/Build Main Menu Scene")]
        public static void BuildMainMenuScene()
        {
            EnsureScenesFolder();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildMainMenuHierarchy();
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"[ARUnity] Main Menu scene saved to {MainMenuScenePath}");
        }

        [MenuItem("ARUnity/Build Scenes/Build All Scenes + Update Build Settings")]
        public static void BuildAllScenes()
        {
            BuildARSessionScene();
            BuildMainMenuScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(ARScenePath, true),
            };

            Debug.Log("[ARUnity] All scenes built. Build Settings updated.");
        }

        // ── AR Session scene ──────────────────────────────────────────────

        private static void BuildARSessionHierarchy()
        {
            // ── AR Session ───────────────────────────────────────────────
            var arSessionGO = new GameObject("AR Session");
            arSessionGO.AddComponent<ARSession>();

            // ── XR Origin ────────────────────────────────────────────────
            var xrOriginGO = new GameObject("XR Origin");
            var xrOrigin = xrOriginGO.AddComponent<XROrigin>();

            var cameraOffsetGO = new GameObject("Camera Offset");
            cameraOffsetGO.transform.SetParent(xrOriginGO.transform, false);

            var mainCameraGO = new GameObject("Main Camera");
            mainCameraGO.tag = "MainCamera";
            mainCameraGO.transform.SetParent(cameraOffsetGO.transform, false);

            var cam = mainCameraGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 20f;

            var urpData = mainCameraGO.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderShadows = false;
            urpData.requiresDepthOption = CameraOverrideOption.On;

            mainCameraGO.AddComponent<ARCameraManager>();
            mainCameraGO.AddComponent<ARCameraBackground>();

            // Wire XROrigin to camera hierarchy
            var xrOriginSO = new SerializedObject(xrOrigin);
            xrOriginSO.FindProperty("m_Camera").objectReferenceValue = cam;
            xrOriginSO.FindProperty("m_CameraFloorOffsetObject").objectReferenceValue = cameraOffsetGO;
            xrOriginSO.ApplyModifiedPropertiesWithoutUndo();

            // ── AR Feature Controllers ────────────────────────────────────
            var featureControllersGO = new GameObject("AR Feature Controllers");

            // Plane Detection
            var planeManagerGO = new GameObject("Plane Detection Manager");
            planeManagerGO.transform.SetParent(featureControllersGO.transform, false);
            var planeManager = planeManagerGO.AddComponent<ARPlaneManager>();
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
            var planeDetCtrl = planeManagerGO.AddComponent<PlaneDetectionController>();
            SetField(planeDetCtrl, "_planeManager", planeManager);

            // Image Tracking
            var imageTrackingGO = new GameObject("Image Tracking Manager");
            imageTrackingGO.transform.SetParent(featureControllersGO.transform, false);
            var trackedImageMgr = imageTrackingGO.AddComponent<ARTrackedImageManager>();
            var imageTrackingCtrl = imageTrackingGO.AddComponent<ImageTrackingController>();
            SetField(imageTrackingCtrl, "_trackedImageManager", trackedImageMgr);

            // Wire reference image library onto ARTrackedImageManager
            var refLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(ImageTrackingBuilder.LibraryPath);
            if (refLibrary != null)
            {
                var imgMgrSO = new SerializedObject(trackedImageMgr);
                // m_SerializedLibrary is ARTrackedImageManager's backing field for referenceLibrary
                var libProp = imgMgrSO.FindProperty("m_SerializedLibrary");
                if (libProp != null)
                {
                    libProp.objectReferenceValue = refLibrary;
                    imgMgrSO.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Wire default overlay prefab
            var overlayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ImageTrackingBuilder.OverlayPrefabPath);
            if (overlayPrefab != null)
                SetField(imageTrackingCtrl, "_defaultOverlayPrefab", overlayPrefab);

            // Raycast + ARRaycastHandler (event-based pose feed for placement)
            var raycastManagerGO = new GameObject("Raycast Manager");
            raycastManagerGO.transform.SetParent(featureControllersGO.transform, false);
            var raycastMgr = raycastManagerGO.AddComponent<ARRaycastManager>();
            var raycastHandler = raycastManagerGO.AddComponent<ARRaycastHandler>();
            SetField(raycastHandler, "_raycastManager", raycastMgr);

            // Occlusion
            var occlusionManagerGO = new GameObject("Occlusion Manager");
            occlusionManagerGO.transform.SetParent(featureControllersGO.transform, false);
            var occlusionMgr = occlusionManagerGO.AddComponent<AROcclusionManager>();
            var occlusionCtrl = occlusionManagerGO.AddComponent<AROcclusionController>();
            SetField(occlusionCtrl, "_occlusionManager", occlusionMgr);

            // Anchor
            var anchorManagerGO = new GameObject("Anchor Manager");
            anchorManagerGO.transform.SetParent(featureControllersGO.transform, false);
            var anchorMgr = anchorManagerGO.AddComponent<ARAnchorManager>();

            // ── UI Root ───────────────────────────────────────────────────
            var canvasGO = new GameObject("UI Root");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Status label — anchored to top centre
            var statusGO = CreateGO("Status Label", canvasGO.transform);
            var statusRT = statusGO.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 0.9f);
            statusRT.anchorMax = new Vector2(1f, 1f);
            statusRT.offsetMin = new Vector2(20f, 0f);
            statusRT.offsetMax = new Vector2(-20f, -10f);
            var statusTMP = statusGO.AddComponent<TextMeshProUGUI>();
            statusTMP.text = "";
            statusTMP.fontSize = 22f;
            statusTMP.alignment = TextAlignmentOptions.Center;
            statusTMP.color = Color.white;

            // Scanning panel
            var scanningPanel = CreateFullscreenPanel("Scanning Panel", canvasGO.transform);
            var scanLabel = CreateLabel("Scan Label", scanningPanel.transform,
                "Move your device to detect surfaces...", 24f,
                new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f));

            // Placement panel (hidden by default)
            var placementPanel = CreateFullscreenPanel("Placement Panel", canvasGO.transform);
            placementPanel.SetActive(false);
            CreateLabel("Place Label", placementPanel.transform,
                "Tap to place an object", 24f,
                new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.52f));

            // Placed panel (hidden by default)
            var placedPanel = CreateFullscreenPanel("Placed Panel", canvasGO.transform);
            placedPanel.SetActive(false);
            CreateLabel("Placed Label", placedPanel.transform,
                "Object placed!", 24f,
                new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.62f));
            var clearBtn = CreateButton("Clear Button", placedPanel.transform, "Clear All",
                new Vector2(0.3f, 0.38f), new Vector2(0.7f, 0.47f));

            // Occlusion toggle — bottom-right corner
            var occlusionBtn = CreateButton("Occlusion Toggle", canvasGO.transform, "Occlusion: OFF",
                new Vector2(0.55f, 0.02f), new Vector2(0.95f, 0.09f));
            var occlusionBtnLabel = occlusionBtn.GetComponentInChildren<TextMeshProUGUI>();

            // Placement reticle — crosshair at screen centre
            var reticleGO = CreateGO("Placement Reticle", canvasGO.transform);
            var reticleRT = reticleGO.AddComponent<RectTransform>();
            reticleRT.anchorMin = new Vector2(0.5f, 0.5f);
            reticleRT.anchorMax = new Vector2(0.5f, 0.5f);
            reticleRT.sizeDelta = new Vector2(48f, 48f);
            reticleRT.anchoredPosition = Vector2.zero;
            var reticleImg = reticleGO.AddComponent<Image>();
            reticleImg.color = new Color(1f, 1f, 1f, 0.85f);
            reticleGO.SetActive(false);

            // Debug overlay panel (dev builds only)
            var debugPanel = CreateGO("Debug Panel", canvasGO.transform);
            var debugPanelRT = debugPanel.AddComponent<RectTransform>();
            debugPanelRT.anchorMin = new Vector2(0f, 0.7f);
            debugPanelRT.anchorMax = new Vector2(0.5f, 0.9f);
            debugPanelRT.offsetMin = new Vector2(10f, 0f);
            debugPanelRT.offsetMax = new Vector2(-10f, 0f);
            var debugBg = debugPanel.AddComponent<Image>();
            debugBg.color = new Color(0f, 0f, 0f, 0.6f);
            debugPanel.SetActive(false);

            var stateLabel = CreateLabel("State Label", debugPanel.transform,
                "AR State: -", 16f, new Vector2(0f, 0.6f), new Vector2(1f, 0.9f));
            var planeCountLabel = CreateLabel("Plane Count Label", debugPanel.transform,
                "Planes: 0", 16f, new Vector2(0f, 0.3f), new Vector2(1f, 0.6f));

            // EventSystem + Input System UI module
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            // ── Scene Controllers ─────────────────────────────────────────
            var controllersGO = new GameObject("Scene Controllers");

            var permMgr = controllersGO.AddComponent<PermissionsManager>();
            var sceneLoader = controllersGO.AddComponent<SceneLoader>();
            var arSessionMgr = controllersGO.AddComponent<ARSessionManager>();
            var appStateMgr = controllersGO.AddComponent<AppStateManager>();
            var hudCtrl = controllersGO.AddComponent<HUDController>();
            var debugOverlay = controllersGO.AddComponent<DebugOverlay>();
            var objectPlacementCtrl = controllersGO.AddComponent<ObjectPlacementController>();
            var arUICtrl = controllersGO.AddComponent<ARUIController>();

            // Wire ARSessionManager
            SetField(arSessionMgr, "_arSession", arSessionGO.GetComponent<ARSession>());
            SetField(arSessionMgr, "_checkingUI", scanningPanel);

            // Wire ObjectPlacementController
            SetField(objectPlacementCtrl, "_raycastHandler", raycastHandler);
            SetField(objectPlacementCtrl, "_anchorManager", anchorMgr);
            SetField(objectPlacementCtrl, "_placementReticle", reticleGO);

            // Wire prefab assets if they exist (built by PrefabBuilder)
            var planePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.PlaneVisualizationPrefabPath);
            if (planePrefab != null)
            {
                var planeMgrSO = new SerializedObject(planeManager);
                planeMgrSO.FindProperty("m_PlanePrefab").objectReferenceValue = planePrefab;
                planeMgrSO.ApplyModifiedPropertiesWithoutUndo();
            }

            var placedObjectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.PlacedObjectPrefabPath);
            if (placedObjectPrefab != null)
                SetField(objectPlacementCtrl, "_objectPrefab", placedObjectPrefab);

            // Wire ARUIController
            SetField(arUICtrl, "_scanningPanel", scanningPanel);
            SetField(arUICtrl, "_placementPanel", placementPanel);
            SetField(arUICtrl, "_placedPanel", placedPanel);
            SetField(arUICtrl, "_statusLabel", statusTMP);
            SetField(arUICtrl, "_clearButton", clearBtn.GetComponent<Button>());
            SetField(arUICtrl, "_occlusionToggleButton", occlusionBtn.GetComponent<Button>());
            SetField(arUICtrl, "_occlusionButtonLabel", occlusionBtnLabel);
            SetField(arUICtrl, "_placementController", objectPlacementCtrl);
            SetField(arUICtrl, "_occlusionController", occlusionCtrl);
            SetField(arUICtrl, "_planeController", planeDetCtrl);

            // Wire AppStateManager
            SetField(appStateMgr, "_arSessionManager", arSessionMgr);
            SetField(appStateMgr, "_uiController", arUICtrl);
            SetField(appStateMgr, "_planeController", planeDetCtrl);
            SetField(appStateMgr, "_placementController", objectPlacementCtrl);
            SetField(appStateMgr, "_permissionsManager", permMgr);

            // Wire DebugOverlay
            SetField(debugOverlay, "_panel", debugPanel);
            SetField(debugOverlay, "_arSession", arSessionGO.GetComponent<ARSession>());
            SetField(debugOverlay, "_planeManager", planeManager);
            SetField(debugOverlay, "_sessionStateLabel", stateLabel);
            SetField(debugOverlay, "_planeCountLabel", planeCountLabel);
        }

        // ── Main Menu scene ───────────────────────────────────────────────

        private static void BuildMainMenuHierarchy()
        {
            // Camera
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
            camGO.AddComponent<UniversalAdditionalCameraData>();

            // Canvas
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Title
            var title = CreateLabel("Title", canvasGO.transform, "AR Unity", 72f,
                new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.78f));
            title.fontStyle = FontStyles.Bold;

            // Subtitle
            CreateLabel("Subtitle", canvasGO.transform, "Augmented Reality Experience", 26f,
                new Vector2(0.1f, 0.54f), new Vector2(0.9f, 0.62f));

            // Start button
            var startBtn = CreateButton("Start AR Button", canvasGO.transform, "Start AR",
                new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.49f));

            // Wire start button to load AR scene
            var sceneBtnHandler = startBtn.AddComponent<MainMenuController>();
            SetField(sceneBtnHandler, "_startButton", startBtn.GetComponent<Button>());

            // EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        private static GameObject CreateGO(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateFullscreenPanel(string name, Transform parent)
        {
            var go = CreateGO(name, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            return go;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text,
            float fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = CreateGO(name, parent);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private static GameObject CreateButton(string name, Transform parent, string labelText,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGO = CreateGO(name, parent);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.15f, 0.55f, 0.95f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGO = CreateGO("Label", btnGO.transform);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 22f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnGO;
        }

        // Sets a [SerializeField] private field via SerializedObject so Unity serializes the reference.
        private static void SetField<T>(Object target, string fieldName, T value) where T : Object
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[ARSceneBuilder] '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
