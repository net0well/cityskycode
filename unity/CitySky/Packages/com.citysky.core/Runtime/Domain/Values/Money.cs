using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>
    /// Dinheiro em centavos (<see cref="long"/>, aritmética <c>checked</c>) — determinístico, sem ponto flutuante.
    /// F1-30 / regra inviolável: dinheiro em centavos.
    /// </summary>
    public readonly struct Money : IEquatable<Money>, IComparable<Money>
    {
        public readonly long Cents;

        public Money(long cents) { Cents = cents; }

        public static readonly Money Zero = default;

        public static Money FromReais(long reais) => new Money(checked(reais * 100L));

        public static Money operator +(Money a, Money b) => new Money(checked(a.Cents + b.Cents));
        public static Money operator -(Money a, Money b) => new Money(checked(a.Cents - b.Cents));
        public static Money operator -(Money a) => new Money(checked(-a.Cents));
        public static Money operator *(Money a, long factor) => new Money(checked(a.Cents * factor));

        public bool IsNegative => Cents < 0;

        public bool Equals(Money other) => Cents == other.Cents;
        public override bool Equals(object? obj) => obj is Money o && Equals(o);
        public override int GetHashCode() => Cents.GetHashCode();
        public int CompareTo(Money other) => Cents.CompareTo(other.Cents);

        public static bool operator ==(Money a, Money b) => a.Cents == b.Cents;
        public static bool operator !=(Money a, Money b) => a.Cents != b.Cents;
        public static bool operator <(Money a, Money b) => a.Cents < b.Cents;
        public static bool operator >(Money a, Money b) => a.Cents > b.Cents;
        public static bool operator <=(Money a, Money b) => a.Cents <= b.Cents;
        public static bool operator >=(Money a, Money b) => a.Cents >= b.Cents;

        public override string ToString()
        {
            long abs = Cents < 0 ? -Cents : Cents;
            string sign = Cents < 0 ? "-" : string.Empty;
            return sign + "R$" + (abs / 100L).ToString(CultureInfo.InvariantCulture)
                 + "," + (abs % 100L).ToString("D2", CultureInfo.InvariantCulture);
        }
    }
}
