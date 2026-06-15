using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ImageTrackingController : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _trackedImageManager;

        // Fallback overlay shown for any image that has no specific entry below
        [SerializeField] private GameObject _defaultOverlayPrefab;

        // Per-image overrides: maps reference image name to a custom prefab
        [SerializeField] private ImagePrefabEntry[] _imagePrefabs;

        private readonly Dictionary<string, GameObject> _prefabLookup = new();
        private readonly Dictionary<TrackableId, GameObject> _activeOverlays = new();

        private void Awake()
        {
            foreach (var entry in _imagePrefabs)
            {
                if (entry.prefab != null)
                    _prefabLookup[entry.imageName] = entry.prefab;
            }
        }

        private void Reset()
        {
            _trackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        private void OnEnable()
        {
            if (_trackedImageManager != null)
                _trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }

        private void OnDisable()
        {
            if (_trackedImageManager != null)
                _trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }

        private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var image in args.added)
                SpawnOverlay(image);

            foreach (var image in args.updated)
                UpdateOverlay(image);

            foreach (var removed in args.removed)
                DestroyOverlay(removed.Key);
        }

        private void SpawnOverlay(ARTrackedImage image)
        {
            // Prefer a per-image prefab, fall back to the default
            if (!_prefabLookup.TryGetValue(image.referenceImage.name, out var prefab))
                prefab = _defaultOverlayPrefab;

            if (prefab == null) return;

            var overlay = Instantiate(prefab, image.transform);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localRotation = Quaternion.identity;

            overlay.GetComponent<ImageTrackingOverlayController>()?.Initialize(image);
            _activeOverlays[image.trackableId] = overlay;
        }

        private void UpdateOverlay(ARTrackedImage image)
        {
            if (!_activeOverlays.TryGetValue(image.trackableId, out var overlay)) return;

            // The overlay is a child of the tracked image — its world transform follows
            // automatically. Only visibility and state label need updating here.
            var isTracked = image.trackingState == TrackingState.Tracking;
            overlay.SetActive(isTracked);

            if (isTracked)
                overlay.GetComponent<ImageTrackingOverlayController>()?.UpdateTrackingState(image);
        }

        private void DestroyOverlay(TrackableId id)
        {
            if (!_activeOverlays.TryGetValue(id, out var overlay)) return;
            _activeOverlays.Remove(id);
            if (overlay != null) Destroy(overlay);
        }

        [System.Serializable]
        private struct ImagePrefabEntry
        {
            public string imageName;
            public GameObject prefab;
        }
    }
}
