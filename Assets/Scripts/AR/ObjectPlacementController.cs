using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    public class ObjectPlacementController : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private ARAnchorManager _anchorManager;
        [SerializeField] private GameObject _placementReticle;
        [SerializeField] private GameObject _objectPrefab;
        [SerializeField] private int _maxPlacedObjects = 5;

        private static readonly List<ARRaycastHit> _hits = new();
        private readonly List<GameObject> _placedObjects = new();
        private bool _placementEnabled;
        private Pose _placementPose;
        private bool _hasValidPlacementPose;

        public bool PlacementEnabled
        {
            get => _placementEnabled;
            set
            {
                _placementEnabled = value;
                if (_placementReticle != null)
                    _placementReticle.SetActive(value && _hasValidPlacementPose);
            }
        }

        private void Update()
        {
            if (!_placementEnabled) return;

            UpdatePlacementPose();
            UpdateReticle();
            HandleTouchInput();
        }

        private void UpdatePlacementPose()
        {
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            _hasValidPlacementPose = _raycastManager.Raycast(
                screenCenter, _hits, TrackableType.PlaneWithinPolygon);

            if (_hasValidPlacementPose)
                _placementPose = _hits[0].pose;
        }

        private void UpdateReticle()
        {
            if (_placementReticle == null) return;
            _placementReticle.SetActive(_hasValidPlacementPose);

            if (_hasValidPlacementPose)
            {
                _placementReticle.transform.SetPositionAndRotation(
                    _placementPose.position, _placementPose.rotation);
            }
        }

        private void HandleTouchInput()
        {
            if (!_hasValidPlacementPose) return;
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                TryPlaceObject();
        }

        private void TryPlaceObject()
        {
            if (_placedObjects.Count >= _maxPlacedObjects)
            {
                var oldest = _placedObjects[0];
                _placedObjects.RemoveAt(0);
                Destroy(oldest);
            }

            // AttachAnchor keeps the object fixed to the plane as ARCore/ARKit refines geometry
            var hitPlane = _hits[0].trackable as ARPlane;
            if (hitPlane == null) return;

            var anchor = _anchorManager.AttachAnchor(hitPlane, _placementPose);
            if (anchor == null) return;

            var placed = Instantiate(_objectPrefab, anchor.transform);
            placed.transform.localPosition = Vector3.zero;
            placed.transform.localRotation = Quaternion.identity;
            _placedObjects.Add(placed);
        }

        public void ClearAllPlacedObjects()
        {
            foreach (var obj in _placedObjects)
            {
                if (obj != null) Destroy(obj);
            }
            _placedObjects.Clear();
        }
    }
}
