using Unity.Entities;

namespace Integration
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class FixedUpdateGroup : ComponentSystemGroup
    {
        public const float FixedTimeDelta = 1/60.0f;

        private float _TimeSinceLastUpdate = 0f;

        public bool IsPaused { get; set; }

        protected override void OnUpdate()
        {
            if (IsPaused)
                return;

            _TimeSinceLastUpdate += Time.DeltaTime;
            if (_TimeSinceLastUpdate > FixedTimeDelta)
            {
                base.OnUpdate();
                _TimeSinceLastUpdate = (_TimeSinceLastUpdate - FixedTimeDelta) % FixedTimeDelta;
            }
        }
    }
}
