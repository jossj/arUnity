using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.ARFoundation;

namespace ARUnity.Tests
{
    // These tests use XR Simulation — requires com.unity.xr.simulation package
    // and an XRSimulationEnvironment prefab configured in XR Simulation settings.
    public class PlacementIntegrationTests
    {
        private GameObject _sessionGO;
        private ARSession _arSession;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _sessionGO = new GameObject("AR Session");
            _arSession = _sessionGO.AddComponent<ARSession>();

            // Allow one frame for session initialization
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_sessionGO);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ARSession_Initializes_WithoutError()
        {
            yield return new WaitForSeconds(0.5f);

            // Session should move out of Initializing state
            Assert.AreNotEqual(ARSessionState.None, ARSession.state);
            Assert.AreNotEqual(ARSessionState.Unsupported, ARSession.state,
                "AR is unsupported in this environment. Is XR Simulation configured?");
        }

        [UnityTest]
        public IEnumerator ARSession_DoesNotThrow_OnDisable()
        {
            yield return null;

            Assert.DoesNotThrow(() => _arSession.enabled = false);
            yield return null;
            Assert.DoesNotThrow(() => _arSession.enabled = true);
        }
    }
}
