using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARUnity.Core;

namespace ARUnity.AR
{
    public class ARSessionManager : MonoBehaviour
    {
        [SerializeField] private ARSession _arSession;
        [SerializeField] private GameObject _unsupportedUI;
        [SerializeField] private GameObject _checkingUI;
        [SerializeField] private GameObject _requiresInstallUI;

        public static ARSessionManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
            switch (args.state)
            {
                case ARSessionState.Unsupported:
                    SetUIState(unsupported: true);
                    break;

                case ARSessionState.NeedsInstall:
                    SetUIState(requiresInstall: true);
                    break;

                case ARSessionState.Installing:
                    SetUIState(checking: true);
                    break;

                case ARSessionState.Ready:
                case ARSessionState.SessionInitializing:
                    SetUIState(checking: true);
                    break;

                case ARSessionState.SessionTracking:
                    SetUIState();
                    break;
            }
        }

        private void SetUIState(bool unsupported = false, bool requiresInstall = false, bool checking = false)
        {
            if (_unsupportedUI != null) _unsupportedUI.SetActive(unsupported);
            if (_requiresInstallUI != null) _requiresInstallUI.SetActive(requiresInstall);
            if (_checkingUI != null) _checkingUI.SetActive(checking);
        }

        public bool IsSessionTracking =>
            ARSession.state == ARSessionState.SessionTracking;
    }
}
