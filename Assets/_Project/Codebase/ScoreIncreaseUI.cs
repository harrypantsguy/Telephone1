using System.Collections;
using TMPro;
using UnityEngine;

namespace _Project.Codebase
{
    public sealed class ScoreIncreaseUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        public void SetIncrease(in int increase)
        {
            _text.text = $"+{increase}";
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            var t = 0f;
            const float duration = .5f;

            while (t < duration)
            {
                t += Time.deltaTime;

                var col = _text.color;
                col.a = 1f - t / duration;
                _text.color = col;

                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}