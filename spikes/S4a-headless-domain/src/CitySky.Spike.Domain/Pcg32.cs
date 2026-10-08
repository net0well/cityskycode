namespace CitySky.Spike.Domain
{
    /// <summary>PCG32 (O'Neill): RNG determinístico, igual em Mono, IL2CPP e .NET.</summary>
    public struct Pcg32
    {
        private ulong _state;
        private readonly ulong _increment;

        public Pcg32(ulong seed, ulong stream)
        {
            _state = 0UL;
            _increment = (stream << 1) | 1UL;
            NextUInt();
            _state += seed;
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong old = _state;
            _state = unchecked(old * 6364136223846793005UL + _increment);
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & 31));
        }

        /// <summary>Inteiro em [min, max). O viés do módulo é irrelevante para o benchmark.</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }
    }
}
