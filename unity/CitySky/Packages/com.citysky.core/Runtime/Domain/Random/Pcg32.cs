using System;

namespace CitySky.Domain
{
    /// <summary>
    /// Gerador PCG32 (determinístico entre Mono e .NET; validado no spike S4a). F1-32.
    /// Struct <b>mutável</b>: guarde-o num campo e chame os métodos nele; não copie no caminho quente.
    /// Crie fluxos independentes por <see cref="Stream"/> a partir de <c>(worldSeed, systemId, tick)</c>.
    /// </summary>
    public struct Pcg32 : IEquatable<Pcg32>
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private readonly ulong _inc;

        private Pcg32(ulong state, ulong inc)
        {
            _state = state;
            _inc = inc;
        }

        /// <summary>Cria um fluxo PCG32 semeado de forma determinística.</summary>
        public static Pcg32 Seeded(ulong initState, ulong initSeq)
        {
            var rng = new Pcg32(0UL, (initSeq << 1) | 1UL);
            rng.NextUInt();
            rng._state += initState;
            rng.NextUInt();
            return rng;
        }

        /// <summary>Fluxo independente por sistema e tick, derivado da semente do mundo (SplitMix64).</summary>
        public static Pcg32 Stream(ulong worldSeed, int systemId, long tick)
        {
            ulong a = worldSeed;
            ulong initState = SplitMix64(ref a);
            ulong b = worldSeed ^ ((ulong)(uint)systemId * 0x9E3779B97F4A7C15UL) ^ ((ulong)tick << 1);
            ulong initSeq = SplitMix64(ref b);
            return Seeded(initState, initSeq);
        }

        /// <summary>Próximo inteiro de 32 bits sem sinal (avança o estado).</summary>
        public uint NextUInt()
        {
            ulong old = _state;
            _state = unchecked(old * Multiplier + _inc);
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        /// <summary>Inteiro uniforme em [0, <paramref name="exclusiveMax"/>) sem viés (rejeição).</summary>
        public int NextInt(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
            }

            uint bound = (uint)exclusiveMax;
            uint threshold = (uint)(-(int)bound) % bound; // 2^32 % bound
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold)
                {
                    return (int)(r % bound);
                }
            }
        }

        private static ulong SplitMix64(ref ulong x)
        {
            unchecked
            {
                ulong z = x += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }

        public bool Equals(Pcg32 other) => _state == other._state && _inc == other._inc;
        public override bool Equals(object? obj) => obj is Pcg32 o && Equals(o);
        public override int GetHashCode() => _state.GetHashCode() ^ _inc.GetHashCode();
        public static bool operator ==(Pcg32 a, Pcg32 b) => a.Equals(b);
        public static bool operator !=(Pcg32 a, Pcg32 b) => !a.Equals(b);
    }
}
