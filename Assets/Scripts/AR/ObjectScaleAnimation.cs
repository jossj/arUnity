using System.Collections;
using UnityEngine;

namespace ARUnity.AR
{
    public class ObjectScaleAnimation : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.35f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private void Start()
        {
            StartCoroutine(ScaleIn());
        }

        private IEnumerator ScaleIn()
        {
            var target = transform.localScale;
            transform.localScale = Vector3.zero;
            var elapsed = 0f;

            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = target * _curve.Evaluate(Mathf.Clamp01(elapsed / _duration));
                yield return null;
            }

            transform.localScale = target;
        }
    }
}
