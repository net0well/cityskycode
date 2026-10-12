using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>Identidade de um cidadão (DOMAIN_MODEL §3.1). Começa em 1; 0 = inválido.</summary>
    public readonly struct CitizenId : IEquatable<CitizenId>
    {
        public readonly int Value;
        public CitizenId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(CitizenId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is CitizenId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "C" + Value.ToString(CultureInfo.InvariantCulture);
        public static bool operator ==(CitizenId a, CitizenId b) => a.Value == b.Value;
        public static bool operator !=(CitizenId a, CitizenId b) => a.Value != b.Value;
        public static readonly CitizenId None = default;
    }
}
