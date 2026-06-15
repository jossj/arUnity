using UnityEngine;

namespace ARUnity.AR
{
    // Keeps the attached transform facing parallel to the camera plane.
    // Used on world-space overlay canvases so they're always readable regardless
    // of whether the tracked image is on a floor, wall, or tilted surface.
    public class BillboardOverlay : MonoBehaviour
    {
        private Transform _cameraTransform;

        private void Start()
        {
            var mainCam = Camera.main;
            if (mainCam != null) _cameraTransform = mainCam.transform;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;
            // LookRotation with camera.forward (not toward camera position) gives a flat
            // billboard that doesn't tilt at extreme viewing angles.
            transform.rotation = Quaternion.LookRotation(_cameraTransform.forward);
        }
    }
}
