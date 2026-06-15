using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARUnity.Core;

namespace ARUnity.AR
{
    public enum AppState
    {
        Initializing,
        PermissionCheck,
        PermissionDenied,
        ARStarting,
        Scanning,
        Placement,
        Placed
    }

    public class AppStateManager : MonoBehaviour
    {
        public static AppStateManager Instance { get; private set; }

        public AppState CurrentState { get; private set; } = AppState.Initializing;

        public event Action<AppState, AppState> StateChanged;

        [SerializeField] private ARSessionManager _arSessionManager;
        [SerializeField] private ARUIController _uiController;
        [SerializeField] private PlaneDetectionController _planeController;
        [SerializeField] private ObjectPlacementController _placementController;
        [SerializeField] private PermissionsManager _permissionsManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            TransitionTo(AppState.PermissionCheck);
            _permissionsManager.RequestCameraPermission(
                onGranted: () => TransitionTo(AppState.ARStarting),
                onDenied: () => TransitionTo(AppState.PermissionDenied)
            );
        }

        private void OnEnable()
        {
            ARSession.stateChanged += OnARSessionStateChanged;

            if (_planeController != null)
                _planeController.FirstPlaneDetected += OnFirstPlaneDetected;

            if (_placementController != null)
                _placementController.ObjectPlaced += OnObjectPlacedEvent;

            if (_uiController != null)
                _uiController.ClearRequested += OnClearRequested;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= OnARSessionStateChanged;

            if (_planeController != null)
                _planeController.FirstPlaneDetected -= OnFirstPlaneDetected;

            if (_placementController != null)
                _placementController.ObjectPlaced -= OnObjectPlacedEvent;

            if (_uiController != null)
                _uiController.ClearRequested -= OnClearRequested;
        }

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            if (CurrentState != AppState.ARStarting) return;

            if (args.state == ARSessionState.SessionTracking)
                TransitionTo(AppState.Scanning);
        }

        private void OnFirstPlaneDetected() => OnPlaneDetected();
        private void OnObjectPlacedEvent() => OnObjectPlaced();

        private void OnClearRequested()
        {
            if (CurrentState == AppState.Placed)
                TransitionTo(AppState.Placement);
        }

        public void OnPlaneDetected()
        {
            if (CurrentState == AppState.Scanning)
                TransitionTo(AppState.Placement);
        }

        public void OnObjectPlaced()
        {
            if (CurrentState == AppState.Placement)
                TransitionTo(AppState.Placed);
        }

        public void TransitionTo(AppState next)
        {
            var previous = CurrentState;
            CurrentState = next;
            StateChanged?.Invoke(previous, next);
            ApplyStateEffects(next);
        }

        private void ApplyStateEffects(AppState state)
        {
            switch (state)
            {
                case AppState.ARStarting:
                    // Guard against the race where ARSession reached SessionTracking before
                    // this component's OnEnable subscribed to stateChanged.
                    if (ARSession.state == ARSessionState.SessionTracking)
                        TransitionTo(AppState.Scanning);
                    break;

                case AppState.Scanning:
                    _uiController?.ShowScanningUI();
                    _planeController?.EnableAll();
                    break;

                case AppState.Placement:
                    _uiController?.ShowPlacementUI();
                    break;

                case AppState.Placed:
                    _uiController?.ShowPlacedUI();
                    break;

                case AppState.PermissionDenied:
                    _uiController?.ShowPermissionDeniedUI();
                    break;
            }
        }
    }
}
