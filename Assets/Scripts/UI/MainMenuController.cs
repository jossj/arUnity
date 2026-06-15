using UnityEngine;
using UnityEngine.UI;
using ARUnity.Core;

namespace ARUnity.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        private void Start()
        {
            _startButton?.onClick.AddListener(OnStartPressed);
        }

        private void OnStartPressed()
        {
            SceneLoader.Instance?.LoadScene("ARSession");
        }
    }
}
