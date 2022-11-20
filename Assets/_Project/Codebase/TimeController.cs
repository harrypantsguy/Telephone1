using UnityEngine;

namespace _Project.Codebase
{
    public class TimeController : MonoSingleton<TimeController>
    {
        public const float MIN_TIMESCALE = 0f;
        public const float MAX_TIMESCALE = 2f;
        public const float TIMESCALE_STEP_SIZE = .125f;
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }

        protected override void Awake()
        {
            base.Awake();
            TimeScale = 1f;
        }

        private void Update()
        {
            float newTimeScale = TimeScale + (GameControls.IncreaseGameSpeed.IsPressed ? TIMESCALE_STEP_SIZE : 0f)
                                 - (GameControls.DecreaseGameSpeed.IsPressed ? TIMESCALE_STEP_SIZE : 0f);
            TimeScale = Mathf.Clamp(newTimeScale, MIN_TIMESCALE, MAX_TIMESCALE);
        }
    }
}