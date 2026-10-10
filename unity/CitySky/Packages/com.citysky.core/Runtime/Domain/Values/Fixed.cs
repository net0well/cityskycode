using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>
    /// Ponto fixo Q16.16 (base <see cref="int"/>, 16 bits fracionários) — determinístico entre Mono e .NET,
    /// ao contrário de <c>float</c>/<c>double</c>. Mul/div usam intermediário de 64 bits. F1-30.
    /// </summary>
    public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>
    {
        public const int FractionalBits = 16;
        private const int One = 1 << FractionalBits;

        /// <summary>Valor bruto (inteiro × 2^16).</summary>
        public readonly int Raw;

        private Fixed(int raw) { Raw = raw; }

        public static readonly Fixed Zero = default;
        public static readonly Fixed OneValue = new Fixed(One);

        public static Fixed FromRaw(int raw) => new Fixed(raw);
        public static Fixed FromInt(int value) => new Fixed(checked(value * One));

        /// <summary>Fração exata <paramref name="numerator"/>/<paramref name="denominator"/>.</summary>
        public static Fixed FromFraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException();
            }

            long raw = ((long)numerator << FractionalBits) / denominator;
            return new Fixed((int)raw);
        }

        public int ToIntFloor() => Raw >> FractionalBits; // arredonda para baixo (inclui negativos)

        public static Fixed operator +(Fixed a, Fixed b) => new Fixed(checked(a.Raw + b.Raw));
        public static Fixed operator -(Fixed a, Fixed b) => new Fixed(checked(a.Raw - b.Raw));
        public static Fixed operator -(Fixed a) => new Fixed(checked(-a.Raw));
        public static Fixed operator *(Fixed a, Fixed b) => new Fixed((int)(((long)a.Raw * b.Raw) >> FractionalBits));
        public static Fixed operator /(Fixed a, Fixed b) => new Fixed((int)(((long)a.Raw << FractionalBits) / b.Raw));

        public bool Equals(Fixed other) => Raw == other.Raw;
        public override bool Equals(object? obj) => obj is Fixed o && Equals(o);
        public override int GetHashCode() => Raw;
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);

        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;
        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

        public override string ToString()
        {
            // Representação decimal determinística (6 casas), sem usar float.
            long scaled = ((long)Raw * 1_000_000L) >> FractionalBits;
            long abs = scaled < 0 ? -scaled : scaled;
            string sign = scaled < 0 ? "-" : string.Empty;
            return sign + (abs / 1_000_000L).ToString(CultureInfo.InvariantCulture)
                 + "." + (abs % 1_000_000L).ToString("D6", CultureInfo.InvariantCulture);
        }
    }
}
