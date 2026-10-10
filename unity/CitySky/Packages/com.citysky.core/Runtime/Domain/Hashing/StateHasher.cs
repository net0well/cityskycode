using System;

namespace CitySky.Domain
{
    /// <summary>
    /// Hash determinístico do <see cref="CityState"/> (xxHash64). Mesma seed + mesmos comandos → mesmo hash,
    /// em Mono e .NET (F1-36, F1-110). À medida que o estado cresce, novos campos entram aqui, em ordem fixa.
    /// </summary>
    public static class StateHasher
    {
        public static ulong Hash(CityState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            Span<byte> buffer = stackalloc byte[32];
            WriteU64(buffer, 0, state.WorldSeed);
            WriteU64(buffer, 8, (ulong)state.Clock.Tick);
            WriteU64(buffer, 16, (ulong)state.Heartbeat);
            WriteU64(buffer, 24, (ulong)state.DayCounter);
            return XxHash64.Hash(buffer);
        }

        private static void WriteU64(Span<byte> d, int i, ulong v)
        {
            d[i] = (byte)v;
            d[i + 1] = (byte)(v >> 8);
            d[i + 2] = (byte)(v >> 16);
            d[i + 3] = (byte)(v >> 24);
            d[i + 4] = (byte)(v >> 32);
            d[i + 5] = (byte)(v >> 40);
            d[i + 6] = (byte)(v >> 48);
            d[i + 7] = (byte)(v >> 56);
        }
    }
}
