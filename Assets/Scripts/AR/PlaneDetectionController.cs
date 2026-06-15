using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class PlaneDetectionController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager _planeManager;

        private void Reset()
        {
            _planeManager = GetComponent<ARPlaneManager>();
        }

        public void SetDetectionMode(PlaneDetectionMode mode)
        {
            _planeManager.requestedDetectionMode = mode;
        }

        public void EnableHorizontalOnly()
        {
            SetDetectionMode(PlaneDetectionMode.Horizontal);
        }

        public void EnableAll()
        {
            SetDetectionMode(PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical);
        }

        // Hides plane mesh visualizations without stopping detection.
        // ARRaycastManager continues using plane data even when meshes are hidden.
        public void SetVisualizationEnabled(bool enabled)
        {
            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(enabled);

            _planeManager.planePrefab?.SetActive(enabled);
        }

        public bool HasDetectedAnyPlane => _planeManager.trackables.count > 0;
    }
}
