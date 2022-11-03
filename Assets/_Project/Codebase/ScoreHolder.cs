using TMPro;
using UnityEngine;

namespace _Project.Codebase
{
    public sealed class ScoreHolder : MonoSingleton<ScoreHolder>
    {
        [SerializeField] private TMP_Text _scoreText;
        
        public int Score { get; private set; }
        
        public void IncreaseScoreBy(in int amount)
        {
            Score += amount;
            _scoreText.text = $"Score: {Score}";
        }
    }
}