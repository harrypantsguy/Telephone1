using UnityEngine;

namespace _Project.Codebase
{
    public class LoseScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform _loseScreen;
        private void Update()
        {
            if (Player.Singleton.fellOffScreen && !_loseScreen.gameObject.activeSelf)
            {
                _loseScreen.gameObject.SetActive(true);
            }
        }
    }
}