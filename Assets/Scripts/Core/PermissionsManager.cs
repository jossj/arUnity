using System;
using UnityEngine;
using UnityEngine.Android;

namespace ARUnity.Core
{
    public class PermissionsManager : MonoBehaviour
    {
        public void RequestCameraPermission(Action onGranted, Action onDenied)
        {
#if UNITY_ANDROID
            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                onGranted?.Invoke();
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => onGranted?.Invoke();
            callbacks.PermissionDenied += _ => onDenied?.Invoke();
            callbacks.PermissionDeniedAndDontAskAgain += _ => onDenied?.Invoke();
            Permission.RequestUserPermission(Permission.Camera, callbacks);
#else
            // iOS handles camera permission automatically when ARKit session starts.
            // NSCameraUsageDescription in Info.plist (injected by IOSPostBuildProcessor) is required.
            onGranted?.Invoke();
#endif
        }

        public bool HasCameraPermission()
        {
#if UNITY_ANDROID
            return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
            return true;
#endif
        }
    }
}
