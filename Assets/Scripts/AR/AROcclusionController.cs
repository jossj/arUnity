using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARUnity.AR
{
    [RequireComponent(typeof(AROcclusionManager))]
    public class AROcclusionController : MonoBehaviour
    {
        [SerializeField] private AROcclusionManager _occlusionManager;

        private void Reset()
        {
            _occlusionManager = GetComponent<AROcclusionManager>();
        }

        private void Start()
        {
            // Check depth API availability before enabling — not all ARCore devices support it
            if (!IsDepthSupported())
            {
                _occlusionManager.enabled = false;
                Debug.Log("[ARUnity] Depth/occlusion not supported on this device.");
                return;
            }

            SetQuality(highQuality: false);
        }

        public bool IsDepthSupported()
        {
            var occlusionSubsystem = _occlusionManager.subsystem;
            if (occlusionSubsystem == null) return false;

            return occlusionSubsystem.TryGetSupportedEnvironmentDepthMode(
                out var supported) && supported != EnvironmentDepthMode.Disabled;
        }

        public void SetQuality(bool highQuality)
        {
            if (!_occlusionManager.enabled) return;

            _occlusionManager.requestedEnvironmentDepthMode = highQuality
                ? EnvironmentDepthMode.Best
                : EnvironmentDepthMode.Fastest;

            _occlusionManager.requestedHumanDepthMode = HumanSegmentationDepthMode.Disabled;
            _occlusionManager.requestedHumanStencilMode = HumanSegmentationStencilMode.Disabled;
        }

        public void SetOcclusionEnabled(bool enabled)
        {
            _occlusionManager.requestedEnvironmentDepthMode = enabled
                ? EnvironmentDepthMode.Fastest
                : EnvironmentDepthMode.Disabled;
        }
    }
}
