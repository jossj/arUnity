using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARUnity.AR
{
    public class PlacedObjectSelector : MonoBehaviour
    {
        [SerializeField] private ObjectPlacementController _placementController;
        [SerializeField] private GameObject[] _objectPrefabs;
        [SerializeField] private string[] _objectNames;
        [SerializeField] private TextMeshProUGUI _currentObjectLabel;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;

        private int _currentIndex;

        private void Start()
        {
            _prevButton?.onClick.AddListener(SelectPrevious);
            _nextButton?.onClick.AddListener(SelectNext);
            ApplySelection();
        }

        public void SelectNext()
        {
            if (_objectPrefabs.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % _objectPrefabs.Length;
            ApplySelection();
        }

        public void SelectPrevious()
        {
            if (_objectPrefabs.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + _objectPrefabs.Length) % _objectPrefabs.Length;
            ApplySelection();
        }

        private void ApplySelection()
        {
            if (_objectPrefabs.Length == 0) return;
            _placementController?.SetObjectPrefab(_objectPrefabs[_currentIndex]);
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (_currentObjectLabel == null) return;
            var name = (_objectNames != null && _objectNames.Length > _currentIndex)
                ? _objectNames[_currentIndex]
                : $"Object {_currentIndex + 1}";
            _currentObjectLabel.text = name;
        }
    }
}
