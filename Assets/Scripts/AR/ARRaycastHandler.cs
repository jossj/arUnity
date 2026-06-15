using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARRaycastHandler : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager _raycastManager;
        [SerializeField] private TrackableType _trackableTypes = TrackableType.PlaneWithinPolygon;

        public bool HasValidPose { get; private set; }
        public Pose CurrentPose { get; private set; }
        public ARTrackable HitTrackable { get; private set; }

        public event Action<Pose, ARTrackable> PoseUpdated;
        public event Action PoseLost;

        private static readonly List<ARRaycastHit> _hits = new();

        private void Reset()
        {
            _raycastManager = GetComponent<ARRaycastManager>();
        }

        private void Update()
        {
            if (_raycastManager == null) return;

            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var didHit = _raycastManager.Raycast(screenCenter, _hits, _trackableTypes);

            if (didHit)
            {
                CurrentPose = _hits[0].pose;
                HitTrackable = _hits[0].trackable;
                HasValidPose = true;
                PoseUpdated?.Invoke(CurrentPose, HitTrackable);
            }
            else if (HasValidPose)
            {
                HasValidPose = false;
                HitTrackable = null;
                PoseLost?.Invoke();
            }
        }
    }
}
