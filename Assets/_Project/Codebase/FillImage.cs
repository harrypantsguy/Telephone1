using UnityEngine;
using UnityEngine.UI;

namespace _Project.Codebase
{
    public class FillImage : MonoBehaviour
    {
        public float FillAmount
        {
            set => image.fillAmount = value;
            get => image.fillAmount;
        }
        public Color Color
        {
            set => image.color = value;
            get => image.color;
        }

        public float Alpha
        {
            set => image.color = image.color.SetAlpha(value);
            get => image.color.a;
        }
        [SerializeField] private Image image;
    }
}