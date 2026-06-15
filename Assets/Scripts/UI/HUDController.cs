using UnityEngine;
using TMPro;
using ARUnity.AR;

namespace ARUnity.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stateLabel;
        [SerializeField] private TextMeshProUGUI _fpsLabel;

        private float _fpsTimer;
        private const float FpsUpdateInterval = 0.5f;

        private void OnEnable()
        {
            if (AppStateManager.Instance != null)
                AppStateManager.Instance.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (AppStateManager.Instance != null)
                AppStateManager.Instance.StateChanged -= OnStateChanged;
        }

        private void Update()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            _fpsTimer += Time.deltaTime;
            if (_fpsTimer >= FpsUpdateInterval)
            {
                _fpsTimer = 0f;
                if (_fpsLabel != null)
                    _fpsLabel.text = $"{(int)(1f / Time.smoothDeltaTime)} fps";
            }
#endif
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_stateLabel != null)
                _stateLabel.text = next.ToString();
        }
    }
}
