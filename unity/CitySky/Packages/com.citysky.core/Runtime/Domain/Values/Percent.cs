using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>
    /// Porcentagem em pontos-base (1% = 100 bp; 100% = 10 000 bp) — inteiro, determinístico. F1-30.
    /// </summary>
    public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>
    {
        public const int ScalePerPercent = 100;        // 1% = 100 bp
        public const int Full = 100 * ScalePerPercent; // 100%

        /// <summary>Valor em pontos-base.</summary>
        public readonly int BasisPoints;

        public Percent(int basisPoints) { BasisPoints = basisPoints; }

        public static readonly Percent Zero = default;
        public static readonly Percent OneHundred = new Percent(Full);

        public static Percent FromPercent(int percent) => new Percent(checked(percent * ScalePerPercent));

        /// <summary>Aplica a porcentagem a um valor em dinheiro (trunca para centavo).</summary>
        public Money Of(Money money) => new Money((money.Cents * BasisPoints) / Full);

        /// <summary>Limita a [0%, 100%].</summary>
        public Percent Clamp() => new Percent(BasisPoints < 0 ? 0 : (BasisPoints > Full ? Full : BasisPoints));

        public bool Equals(Percent other) => BasisPoints == other.BasisPoints;
        public override bool Equals(object? obj) => obj is Percent o && Equals(o);
        public override int GetHashCode() => BasisPoints;
        public int CompareTo(Percent other) => BasisPoints.CompareTo(other.BasisPoints);

        public static bool operator ==(Percent a, Percent b) => a.BasisPoints == b.BasisPoints;
        public static bool operator !=(Percent a, Percent b) => a.BasisPoints != b.BasisPoints;
        public static bool operator <(Percent a, Percent b) => a.BasisPoints < b.BasisPoints;
        public static bool operator >(Percent a, Percent b) => a.BasisPoints > b.BasisPoints;
        public static bool operator <=(Percent a, Percent b) => a.BasisPoints <= b.BasisPoints;
        public static bool operator >=(Percent a, Percent b) => a.BasisPoints >= b.BasisPoints;

        public override string ToString()
        {
            int abs = BasisPoints < 0 ? -BasisPoints : BasisPoints;
            string sign = BasisPoints < 0 ? "-" : string.Empty;
            return sign + (abs / ScalePerPercent).ToString(CultureInfo.InvariantCulture)
                 + "." + (abs % ScalePerPercent).ToString("D2", CultureInfo.InvariantCulture) + "%";
        }
    }
}
