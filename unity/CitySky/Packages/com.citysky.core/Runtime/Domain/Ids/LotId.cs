using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>Identidade de um lote (DOMAIN_MODEL §3.1). Começa em 1; 0 = inválido.</summary>
    public readonly struct LotId : IEquatable<LotId>
    {
        public readonly int Value;
        public LotId(int value) { Value = value; }
        public bool IsValid => Value > 0;
        public bool Equals(LotId other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is LotId o && Equals(o);
        public override int GetHashCode() => Value;
        public override string ToString() => "L" + Value.ToString(CultureInfo.InvariantCulture);
        public static bool operator ==(LotId a, LotId b) => a.Value == b.Value;
        public static bool operator !=(LotId a, LotId b) => a.Value != b.Value;
        public static readonly LotId None = default;
    }
}
