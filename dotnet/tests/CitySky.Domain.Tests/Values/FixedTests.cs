using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Values
{
    [TestFixture]
    public sealed class FixedTests
    {
        [Test]
        public void Integer_Roundtrip()
        {
            Assert.That(Fixed.FromInt(5).ToIntFloor(), Is.EqualTo(5));
        }

        [Test]
        public void Multiplication_Is_Exact_For_Integers()
        {
            Fixed product = Fixed.FromInt(6) * Fixed.FromInt(7);
            Assert.That(product.ToIntFloor(), Is.EqualTo(42));
        }

        [Test]
        public void Fraction_Halves_Sum_To_One()
        {
            Fixed half = Fixed.FromFraction(1, 2);
            Assert.That(half + half, Is.EqualTo(Fixed.OneValue));
        }

        [Test]
        public void Division_Truncates_Toward_Negative_Infinity()
        {
            Fixed q = Fixed.FromInt(7) / Fixed.FromInt(2);
            Assert.That(q.ToIntFloor(), Is.EqualTo(3)); // 3,5 -> floor 3
        }

        [Test]
        public void Negative_Floor()
        {
            Assert.That((-Fixed.FromFraction(3, 2)).ToIntFloor(), Is.EqualTo(-2)); // -1,5 -> floor -2
        }
    }
}
