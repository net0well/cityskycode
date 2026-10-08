namespace CitySky.Spike.Domain
{
    /// <summary>Viagens ativas em lista compacta (swap-remove). Progresso em Q16.16.</summary>
    public sealed class TripStore
    {
        public const int FixedOne = 1 << 16;

        private readonly int[] _citizen;
        private readonly long[] _endTick;
        private readonly int[] _progress;
        private readonly int[] _step;
        private int _count;

        public TripStore(int capacity)
        {
            _citizen = new int[capacity];
            _endTick = new long[capacity];
            _progress = new int[capacity];
            _step = new int[capacity];
        }

        public int Count => _count;

        public int CitizenAt(int slot) => _citizen[slot];

        public long EndTickAt(int slot) => _endTick[slot];

        public int ProgressAt(int slot) => _progress[slot];

        public int Add(int citizen, long endTick, int durationTicks)
        {
            int slot = _count++;
            _citizen[slot] = citizen;
            _endTick[slot] = endTick;
            _progress[slot] = 0;
            _step[slot] = FixedOne / durationTicks;
            return slot;
        }

        public void Advance(int slot)
        {
            int next = _progress[slot] + _step[slot];
            _progress[slot] = next > FixedOne ? FixedOne : next;
        }

        /// <summary>Remove a viagem do slot e devolve o cidadão que foi movido para ele (ou 0).</summary>
        public int RemoveAt(int slot)
        {
            int last = --_count;
            if (slot == last)
            {
                return 0;
            }

            _citizen[slot] = _citizen[last];
            _endTick[slot] = _endTick[last];
            _progress[slot] = _progress[last];
            _step[slot] = _step[last];
            return _citizen[slot];
        }
    }
}
