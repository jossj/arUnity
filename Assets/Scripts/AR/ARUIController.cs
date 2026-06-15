using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARUnity.AR
{
    public class ARUIController : MonoBehaviour
    {
        [Header("State Panels")]
        [SerializeField] private GameObject _scanningPanel;
        [SerializeField] private GameObject _placementPanel;
        [SerializeField] private GameObject _placedPanel;

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _occlusionToggleButton;
        [SerializeField] private TextMeshProUGUI _occlusionButtonLabel;

        [Header("Shape Selector")]
        [SerializeField] private GameObject _shapeSelectorPanel;

        [Header("References")]
        [SerializeField] private ObjectPlacementController _placementController;
        [SerializeField] private AROcclusionController _occlusionController;
        [SerializeField] private PlaneDetectionController _planeController;

        private bool _occlusionEnabled;

        private void Start()
        {
            ShowScanningUI();
            _clearButton?.onClick.AddListener(OnClearPressed);
            _occlusionToggleButton?.onClick.AddListener(OnOcclusionToggled);
        }

        public void ShowScanningUI()
        {
            SetPanel(_scanningPanel);
            SetShapeSelector(false);
            SetStatus("Move your device to detect surfaces...");
        }

        public void ShowPlacementUI()
        {
            SetPanel(_placementPanel);
            SetShapeSelector(true);
            SetStatus("Tap to place an object");
            _placementController.PlacementEnabled = true;
        }

        public void ShowPlacedUI()
        {
            SetPanel(_placedPanel);
            SetShapeSelector(false);
            SetStatus("Object placed");
            _planeController.SetVisualizationEnabled(false);
        }

        private void SetPanel(GameObject active)
        {
            if (_scanningPanel != null) _scanningPanel.SetActive(_scanningPanel == active);
            if (_placementPanel != null) _placementPanel.SetActive(_placementPanel == active);
            if (_placedPanel != null) _placedPanel.SetActive(_placedPanel == active);
        }

        private void SetShapeSelector(bool visible)
        {
            if (_shapeSelectorPanel != null)
                _shapeSelectorPanel.SetActive(visible);
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null) _statusLabel.text = message;
        }

        private void OnClearPressed()
        {
            _placementController.ClearAllPlacedObjects();
            _planeController.SetVisualizationEnabled(true);
            ShowPlacementUI();
        }

        private void OnOcclusionToggled()
        {
            if (_occlusionController == null || !_occlusionController.IsDepthSupported()) return;

            _occlusionEnabled = !_occlusionEnabled;
            _occlusionController.SetOcclusionEnabled(_occlusionEnabled);

            if (_occlusionButtonLabel != null)
                _occlusionButtonLabel.text = _occlusionEnabled ? "Occlusion: ON" : "Occlusion: OFF";
        }
    }
}
