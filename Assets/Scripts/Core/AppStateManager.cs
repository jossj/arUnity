using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARUnity.AR;

namespace ARUnity.Core
{
    public enum AppState
    {
        Initializing,
        PermissionCheck,
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
                onDenied: () => Debug.LogError("[ARUnity] Camera permission denied.")
            );
        }

        private void OnEnable()
        {
            ARSession.stateChanged += OnARSessionStateChanged;
        }

        private void OnDisable()
        {
            ARSession.stateChanged -= OnARSessionStateChanged;
        }

        private void OnARSessionStateChanged(ARSessionStateChangedEventArgs args)
        {
            if (CurrentState != AppState.ARStarting) return;

            if (args.state == ARSessionState.SessionTracking)
                TransitionTo(AppState.Scanning);
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
            }
        }
    }
}
