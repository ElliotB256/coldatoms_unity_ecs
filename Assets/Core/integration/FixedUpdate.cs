using Unity.Entities;
using Unity.Mathematics;

namespace Integration
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class FixedUpdateGroup : ComponentSystemGroup
    {
        public const float FIXED_TIME_DELTA = 1/120.0f;

        private float _TimeSinceLastUpdate = 0f;

        protected override void OnUpdate()
        {
            _TimeSinceLastUpdate += Time.DeltaTime;
            if (_TimeSinceLastUpdate > FIXED_TIME_DELTA)
            {
                base.OnUpdate();
                _TimeSinceLastUpdate = (_TimeSinceLastUpdate - FIXED_TIME_DELTA) % FIXED_TIME_DELTA;
            }
        }
    }
}
