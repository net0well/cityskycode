using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>Identidade de um edifício (DOMAIN_MODEL §3.1). Começa em 1; 0 = inválido.</summary>
    public readonly struct BuildingId : IEquatable<BuildingId>
    {
        public readonly int Value;
        public BuildingId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(BuildingId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is BuildingId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "B" + Value.ToString(CultureInfo.InvariantCulture);
        public static bool operator ==(BuildingId a, BuildingId b) => a.Value == b.Value;
        public static bool operator !=(BuildingId a, BuildingId b) => a.Value != b.Value;
        public static readonly BuildingId None = default;
    }
}
