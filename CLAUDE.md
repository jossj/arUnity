# CLAUDE.md — arUnity Project

## Unity Version
Unity 6000.0.26f1 (Unity 6 LTS)

## Packages
- AR Foundation 6.0.x
- ARCore XR Plugin 6.0.x
- ARKit XR Plugin 6.0.x
- XR Plugin Management 4.5.x
- Universal Render Pipeline 17.x
- New Input System 1.8.x
- Addressables 2.x

## C# Conventions
- Namespace pattern: `ARUnity.<Module>` (e.g., `ARUnity.AR`, `ARUnity.Core`, `ARUnity.UI`)
- Use `[SerializeField] private` for inspector-exposed fields — never `public`
- No `FindObjectOfType()` at runtime — use inspector references or the ServiceLocator in `AppStateManager`
- All AR subsystem access must be null-checked (subsystems can be unavailable on non-supported devices)
- Subscribe to AR events in `OnEnable`, unsubscribe in `OnDisable`
- Use `XROrigin` — never the deprecated `ARSessionOrigin`

## Scene Conventions
- Primary AR scene: `Assets/Scenes/ARSession.unity`
- Scene must contain exactly one `ARSession` and one `XROrigin`
- All placed objects must be attached via `ARAnchorManager.AttachAnchor()`, not bare `Instantiate()`
- Plane visualizations must be hidden (not destroyed) after object placement

## Build Targets
- **Android**: IL2CPP, ARM64 only, minSdkVersion 26, targetSdkVersion 35, AAB format
- **iOS**: IL2CPP, ARM64 only, deployment target iOS 16.0
- No Mono backend, no 32-bit targets

## Input
- Always use New Input System (`UnityEngine.InputSystem`)
- Never use legacy `Input.GetTouch()` or `Input.GetMouseButton()`

## Testing
- Edit Mode tests: `Tests/EditMode/` — pure C# logic, no MonoBehaviour lifecycle
- Play Mode tests: `Tests/PlayMode/` — use XR Simulation environment
- Assembly definitions required for all test folders
