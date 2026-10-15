using System;
using System.Globalization;

namespace CitySky.Domain
{
    /// <summary>
    /// Potência elétrica em watts (<see cref="long"/>, inteiro) — determinístico. F1-30.
    /// </summary>
    public readonly struct Power : IEquatable<Power>, IComparable<Power>
    {
        public readonly long Watts;

        public Power(long watts) { Watts = watts; }

        public static readonly Power Zero = default;

        public static Power FromKilowatts(long kilowatts) => new Power(checked(kilowatts * 1000L));

        public long Kilowatts => Watts / 1000L;

        public static Power operator +(Power a, Power b) => new Power(checked(a.Watts + b.Watts));
        public static Power operator -(Power a, Power b) => new Power(checked(a.Watts - b.Watts));
        public static Power operator *(Power a, long factor) => new Power(checked(a.Watts * factor));

        public bool Equals(Power other) => Watts == other.Watts;
        public override bool Equals(object? obj) => obj is Power o && Equals(o);
        public override int GetHashCode() => Watts.GetHashCode();
        public int CompareTo(Power other) => Watts.CompareTo(other.Watts);

        public static bool operator ==(Power a, Power b) => a.Watts == b.Watts;
        public static bool operator !=(Power a, Power b) => a.Watts != b.Watts;
        public static bool operator <(Power a, Power b) => a.Watts < b.Watts;
        public static bool operator >(Power a, Power b) => a.Watts > b.Watts;
        public static bool operator <=(Power a, Power b) => a.Watts <= b.Watts;
        public static bool operator >=(Power a, Power b) => a.Watts >= b.Watts;

        public override string ToString() => Watts.ToString(CultureInfo.InvariantCulture) + " W";
    }
}
