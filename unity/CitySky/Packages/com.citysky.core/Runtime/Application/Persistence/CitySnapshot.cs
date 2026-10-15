namespace CitySky.Application
{
    /// <summary>
    /// Retrato serializável do estado da cidade para save/load (F1-101). Esqueleto da Fase 1: os mesmos
    /// campos que entram no <c>StateHasher</c>, mais o número do esquema e o hash determinístico. O hash
    /// é gravado junto para detectar corrupção/incompatibilidade ao recarregar (round-trip com hash igual).
    /// Imutável; construído por <see cref="SnapshotService"/>. C# 9 (fonte compartilhada, override O1).
    /// </summary>
    public sealed class CitySnapshot
    {
        /// <summary>Versão do formato do save; incrementa quando o layout muda (migração futura).</summary>
        public const int CurrentSchemaVersion = 1;

        public CitySnapshot(int schemaVersion, ulong worldSeed, long tick, long heartbeat, long dayCounter, ulong hash)
        {
            SchemaVersion = schemaVersion;
            WorldSeed = worldSeed;
            Tick = tick;
            Heartbeat = heartbeat;
            DayCounter = dayCounter;
            Hash = hash;
        }

        public int SchemaVersion { get; }

        public ulong WorldSeed { get; }

        public long Tick { get; }

        public long Heartbeat { get; }

        public long DayCounter { get; }

        /// <summary>Hash determinístico do estado no momento da captura (<c>StateHasher</c>).</summary>
        public ulong Hash { get; }
    }
}
