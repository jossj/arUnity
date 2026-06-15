using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    [RequireComponent(typeof(ARPlaneManager))]
    public class PlaneDetectionController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager _planeManager;

        public event Action FirstPlaneDetected;

        private bool _firstPlaneDetected;

        private void Reset()
        {
            _planeManager = GetComponent<ARPlaneManager>();
        }

        private void OnEnable()
        {
            if (_planeManager != null)
                _planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        }

        private void OnDisable()
        {
            if (_planeManager != null)
                _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }

        private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
        {
            if (!_firstPlaneDetected && args.added.Count > 0)
            {
                _firstPlaneDetected = true;
                FirstPlaneDetected?.Invoke();
            }
        }

        public void SetDetectionMode(PlaneDetectionMode mode)
        {
            _planeManager.requestedDetectionMode = mode;
        }

        public void EnableHorizontalOnly() =>
            SetDetectionMode(PlaneDetectionMode.Horizontal);

        public void EnableAll() =>
            SetDetectionMode(PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical);

        // Hides plane mesh visualizations without stopping detection.
        // ARRaycastManager continues using plane data even when meshes are hidden.
        public void SetVisualizationEnabled(bool enabled)
        {
            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(enabled);
        }

        public bool HasDetectedAnyPlane => _planeManager.trackables.count > 0;
    }
}
