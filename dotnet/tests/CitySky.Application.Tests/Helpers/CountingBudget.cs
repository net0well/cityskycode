using CitySky.Application;

namespace CitySky.Application.Tests
{
    /// <summary>Orçamento de teste que permite no máximo <c>N</c> ticks por frame.</summary>
    internal sealed class CountingBudget : ISimulationBudget
    {
        private readonly int _maxPerFrame;
        private int _used;

        public CountingBudget(int maxPerFrame) => _maxPerFrame = maxPerFrame;

        public void BeginFrame() => _used = 0;

        public bool HasTimeLeft()
        {
            if (_used >= _maxPerFrame)
            {
                return false;
            }

            _used++;
            return true;
        }
    }
}
