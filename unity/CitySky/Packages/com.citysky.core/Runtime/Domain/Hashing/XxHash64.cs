using System;

namespace CitySky.Domain
{
    /// <summary>
    /// xxHash64 (implementação própria, sem dependências — O6). Determinístico entre Mono e .NET.
    /// Usado pelo <see cref="StateHasher"/> para detectar divergência de determinismo. F1-36.
    /// </summary>
    public static class XxHash64
    {
        private const ulong P1 = 0x9E3779B185EBCA87UL;
        private const ulong P2 = 0xC2B2AE3D27D4EB4FUL;
        private const ulong P3 = 0x165667B19E3779F9UL;
        private const ulong P4 = 0x85EBCA77C2B2AE63UL;
        private const ulong P5 = 0x27D4EB2F165667C5UL;

        public static ulong Hash(ReadOnlySpan<byte> data, ulong seed = 0)
        {
            unchecked
            {
                int len = data.Length;
                int i = 0;
                ulong h;

                if (len >= 32)
                {
                    ulong v1 = seed + P1 + P2;
                    ulong v2 = seed + P2;
                    ulong v3 = seed;
                    ulong v4 = seed - P1;

                    int limit = len - 32;
                    do
                    {
                        v1 = Round(v1, ReadU64(data, i)); i += 8;
                        v2 = Round(v2, ReadU64(data, i)); i += 8;
                        v3 = Round(v3, ReadU64(data, i)); i += 8;
                        v4 = Round(v4, ReadU64(data, i)); i += 8;
                    }
                    while (i <= limit);

                    h = Rotl(v1, 1) + Rotl(v2, 7) + Rotl(v3, 12) + Rotl(v4, 18);
                    h = MergeRound(h, v1);
                    h = MergeRound(h, v2);
                    h = MergeRound(h, v3);
                    h = MergeRound(h, v4);
                }
                else
                {
                    h = seed + P5;
                }

                h += (ulong)len;

                while (i + 8 <= len)
                {
                    h ^= Round(0, ReadU64(data, i));
                    h = Rotl(h, 27) * P1 + P4;
                    i += 8;
                }

                if (i + 4 <= len)
                {
                    h ^= ReadU32(data, i) * P1;
                    h = Rotl(h, 23) * P2 + P3;
                    i += 4;
                }

                while (i < len)
                {
                    h ^= data[i] * P5;
                    h = Rotl(h, 11) * P1;
                    i++;
                }

                h ^= h >> 33;
                h *= P2;
                h ^= h >> 29;
                h *= P3;
                h ^= h >> 32;
                return h;
            }
        }

        private static ulong Round(ulong acc, ulong input)
        {
            unchecked
            {
                acc += input * P2;
                acc = Rotl(acc, 31);
                acc *= P1;
                return acc;
            }
        }

        private static ulong MergeRound(ulong acc, ulong val)
        {
            unchecked
            {
                val = Round(0, val);
                acc ^= val;
                acc = acc * P1 + P4;
                return acc;
            }
        }

        private static ulong Rotl(ulong x, int r) => (x << r) | (x >> (64 - r));

        private static ulong ReadU64(ReadOnlySpan<byte> d, int i) =>
            d[i]
            | ((ulong)d[i + 1] << 8)
            | ((ulong)d[i + 2] << 16)
            | ((ulong)d[i + 3] << 24)
            | ((ulong)d[i + 4] << 32)
            | ((ulong)d[i + 5] << 40)
            | ((ulong)d[i + 6] << 48)
            | ((ulong)d[i + 7] << 56);

        private static uint ReadU32(ReadOnlySpan<byte> d, int i) =>
            (uint)(d[i] | (d[i + 1] << 8) | (d[i + 2] << 16) | (d[i + 3] << 24));
    }
}
