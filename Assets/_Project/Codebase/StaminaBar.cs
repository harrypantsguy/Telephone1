using UnityEngine;

namespace _Project.Codebase
{
    public class StaminaBar : MonoBehaviour
    {
        private FillImage _fillImage;

        private void Start()
        {
            _fillImage = GetComponent<FillImage>();
        }

        private void Update()
        {
            _fillImage.FillAmount = Player.Singleton.Stamina / Player.Singleton.MaxStamina;
        }
    }
}