using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using TMPro;

namespace ARUnity.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _toggleButton;
        [SerializeField] private TextMeshProUGUI _sessionStateLabel;
        [SerializeField] private TextMeshProUGUI _planeCountLabel;
        [SerializeField] private TextMeshProUGUI _trackingReasonLabel;
        [SerializeField] private ARSession _arSession;
        [SerializeField] private ARPlaneManager _planeManager;

        private void Awake()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            if (_toggleButton != null) _toggleButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
#endif
        }

        private void Start()
        {
            _toggleButton?.onClick.AddListener(Toggle);
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            if (_sessionStateLabel != null)
                _sessionStateLabel.text = $"AR: {ARSession.state}";

            if (_planeManager != null && _planeCountLabel != null)
                _planeCountLabel.text = $"Planes: {_planeManager.trackables.count}";

            if (_trackingReasonLabel != null)
                _trackingReasonLabel.text = ARSession.state == ARSessionState.SessionTracking
                    ? ""
                    : $"Reason: {ARSession.notTrackingReason}";
        }

        public void Toggle()
        {
            if (_panel != null)
                _panel.SetActive(!_panel.activeSelf);
        }
    }
}
