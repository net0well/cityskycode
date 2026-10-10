using System;

namespace CitySky.Domain
{
    // F1-30 / DOMAIN_MODEL §3.1. IDs fortemente tipados: readonly struct (C# 9, sem record struct).
    // Começam em 1; 0 = inválido (None). No MVP são monotônicos (geração/reuso só na Fase 18).

    /// <summary>Identidade de um cidadão.</summary>
    public readonly struct CitizenId : IEquatable<CitizenId>
    {
        public readonly int Value;
        public CitizenId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(CitizenId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is CitizenId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "C" + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static bool operator ==(CitizenId a, CitizenId b) => a.Value == b.Value;
        public static bool operator !=(CitizenId a, CitizenId b) => a.Value != b.Value;
        public static readonly CitizenId None = default;
    }

    /// <summary>Identidade de um edifício.</summary>
    public readonly struct BuildingId : IEquatable<BuildingId>
    {
        public readonly int Value;
        public BuildingId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(BuildingId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is BuildingId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "B" + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static bool operator ==(BuildingId a, BuildingId b) => a.Value == b.Value;
        public static bool operator !=(BuildingId a, BuildingId b) => a.Value != b.Value;
        public static readonly BuildingId None = default;
    }

    /// <summary>Identidade de um lote.</summary>
    public readonly struct LotId : IEquatable<LotId>
    {
        public readonly int Value;
        public LotId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(LotId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is LotId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "L" + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static bool operator ==(LotId a, LotId b) => a.Value == b.Value;
        public static bool operator !=(LotId a, LotId b) => a.Value != b.Value;
        public static readonly LotId None = default;
    }
}
