using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

namespace ARUnity.UI
{
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _sessionStateLabel;
        [SerializeField] private TextMeshProUGUI _planeCountLabel;
        [SerializeField] private TextMeshProUGUI _trackingReasonLabel;
        [SerializeField] private ARSession _arSession;
        [SerializeField] private ARPlaneManager _planeManager;

        private void Awake()
        {
            // Only show in development builds and editor
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            if (_sessionStateLabel != null)
                _sessionStateLabel.text = $"AR State: {ARSession.state}";

            if (_planeManager != null && _planeCountLabel != null)
                _planeCountLabel.text = $"Planes: {_planeManager.trackables.count}";
        }

        public void Toggle()
        {
            if (_panel != null)
                _panel.SetActive(!_panel.activeSelf);
        }
    }
}
