using System;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Values
{
    [TestFixture]
    public sealed class MoneyTests
    {
        [Test]
        public void FromReais_Converts_To_Cents()
        {
            Assert.That(Money.FromReais(12).Cents, Is.EqualTo(1200L));
        }

        [Test]
        public void Addition_And_Subtraction_Are_Exact()
        {
            Money sum = Money.FromReais(10) + new Money(50);
            Assert.That(sum.Cents, Is.EqualTo(1050L));
            Assert.That((sum - new Money(50)).Cents, Is.EqualTo(1000L));
        }

        [Test]
        public void Overflow_Throws()
        {
            Assert.Throws<OverflowException>(() =>
            {
                Money _ = new Money(long.MaxValue) + new Money(1);
            });
        }

        [Test]
        public void Comparison_And_Equality()
        {
            Assert.That(new Money(100) > new Money(99), Is.True);
            Assert.That(new Money(100) == Money.FromReais(1), Is.True);
            Assert.That(new Money(-1).IsNegative, Is.True);
        }

        [Test]
        public void ToString_Is_Deterministic()
        {
            Assert.That(new Money(-1234).ToString(), Is.EqualTo("-R$12,34"));
        }
    }
}
