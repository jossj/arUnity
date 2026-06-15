using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

namespace ARUnity.AR
{
    public class ObjectPlacementController : MonoBehaviour
    {
        [SerializeField] private ARRaycastHandler _raycastHandler;
        [SerializeField] private ARAnchorManager _anchorManager;
        [SerializeField] private GameObject _placementReticle;
        [SerializeField] private GameObject _objectPrefab;
        [SerializeField] private int _maxPlacedObjects = 5;

        private readonly List<GameObject> _placedObjects = new();
        private bool _placementEnabled;

        public event Action ObjectPlaced;

        public bool PlacementEnabled
        {
            get => _placementEnabled;
            set
            {
                _placementEnabled = value;
                RefreshReticle();
            }
        }

        private void OnEnable()
        {
            if (_raycastHandler == null) return;
            _raycastHandler.PoseUpdated += OnPoseUpdated;
            _raycastHandler.PoseLost += OnPoseLost;
        }

        private void OnDisable()
        {
            if (_raycastHandler == null) return;
            _raycastHandler.PoseUpdated -= OnPoseUpdated;
            _raycastHandler.PoseLost -= OnPoseLost;
        }

        private void Update()
        {
            if (!_placementEnabled) return;
            HandleTouchInput();
        }

        private void OnPoseUpdated(Pose pose, ARTrackable trackable)
        {
            if (_placementReticle == null) return;
            _placementReticle.SetActive(_placementEnabled);
            _placementReticle.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        private void OnPoseLost()
        {
            if (_placementReticle != null)
                _placementReticle.SetActive(false);
        }

        private void RefreshReticle()
        {
            if (_placementReticle == null) return;
            var show = _placementEnabled && _raycastHandler != null && _raycastHandler.HasValidPose;
            _placementReticle.SetActive(show);
        }

        private void HandleTouchInput()
        {
            if (_raycastHandler == null || !_raycastHandler.HasValidPose) return;
            if (Touchscreen.current == null) return;

            if (Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                TryPlaceObject();
        }

        private void TryPlaceObject()
        {
            if (_objectPrefab == null) return;

            var hitPlane = _raycastHandler.HitTrackable as ARPlane;
            if (hitPlane == null) return;

            var anchor = _anchorManager.AttachAnchor(hitPlane, _raycastHandler.CurrentPose);
            if (anchor == null) return;

            if (_placedObjects.Count >= _maxPlacedObjects)
            {
                Destroy(_placedObjects[0]);
                _placedObjects.RemoveAt(0);
            }

            var placed = Instantiate(_objectPrefab, anchor.transform);
            placed.transform.localPosition = Vector3.zero;
            placed.transform.localRotation = Quaternion.identity;
            _placedObjects.Add(placed);

            ObjectPlaced?.Invoke();
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
