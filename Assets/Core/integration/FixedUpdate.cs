using Unity.Entities;

namespace Integration
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class FixedUpdateGroup : ComponentSystemGroup
    {
        public const float FIXED_TIME_DELTA = 1/60.0f;

        private float _TimeSinceLastUpdate = 0f;

        public bool IsPaused { get; set; }

        protected override void OnUpdate()
        {
            if (IsPaused)
                return;

            _TimeSinceLastUpdate += Time.DeltaTime;
            if (_TimeSinceLastUpdate > FIXED_TIME_DELTA)
            {
                base.OnUpdate();
                _TimeSinceLastUpdate = (_TimeSinceLastUpdate - FIXED_TIME_DELTA) % FIXED_TIME_DELTA;
            }
        }
    }
}
