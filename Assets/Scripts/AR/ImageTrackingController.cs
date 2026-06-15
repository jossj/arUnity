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

        // Maps reference image name to the prefab to spawn when that image is tracked
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
            _trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        }

        private void OnDisable()
        {
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
            var name = image.referenceImage.name;
            if (!_prefabLookup.TryGetValue(name, out var prefab)) return;

            var overlay = Instantiate(prefab, image.transform);
            _activeOverlays[image.trackableId] = overlay;
        }

        private void UpdateOverlay(ARTrackedImage image)
        {
            if (!_activeOverlays.TryGetValue(image.trackableId, out var overlay)) return;

            // Hide overlay when tracking is lost; show again when regained
            var isTracked = image.trackingState == TrackingState.Tracking;
            overlay.SetActive(isTracked);

            if (isTracked)
            {
                overlay.transform.SetPositionAndRotation(
                    image.transform.position, image.transform.rotation);
            }
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
