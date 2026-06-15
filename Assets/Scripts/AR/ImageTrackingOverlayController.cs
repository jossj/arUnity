using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARUnity.AR
{
    public class ImageTrackingOverlayController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _imageNameLabel;
        [SerializeField] private TextMeshProUGUI _imageSizeLabel;
        [SerializeField] private TextMeshProUGUI _trackingStateLabel;

        public void Initialize(ARTrackedImage image)
        {
            var refImage = image.referenceImage;

            if (_imageNameLabel != null)
                _imageNameLabel.text = refImage.name;

            if (_imageSizeLabel != null)
            {
                if (refImage.specifySize)
                {
                    var w = refImage.size.x * 100f;
                    var h = refImage.size.y * 100f;
                    _imageSizeLabel.text = $"{w:F0} × {h:F0} cm";
                }
                else
                {
                    _imageSizeLabel.text = string.Empty;
                }
            }

            UpdateTrackingState(image);
        }

        public void UpdateTrackingState(ARTrackedImage image)
        {
            if (_trackingStateLabel != null)
                _trackingStateLabel.text = image.trackingState.ToString();
        }
    }
}
