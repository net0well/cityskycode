using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Random
{
    [TestFixture]
    public sealed class Pcg32Tests
    {
        [Test]
        public void Matches_Reference_Vector_Seed42_Seq54()
        {
            // Sequência dourada da implementação de referência do PCG32 (initstate=42, initseq=54).
            var rng = Pcg32.Seeded(42UL, 54UL);
            Assert.That(rng.NextUInt(), Is.EqualTo(0xA15C02B7u));
            Assert.That(rng.NextUInt(), Is.EqualTo(0x7B47F409u));
            Assert.That(rng.NextUInt(), Is.EqualTo(0xBA1D3330u));
        }

        [Test]
        public void Same_Stream_Params_Produce_Same_Sequence()
        {
            var a = Pcg32.Stream(worldSeed: 123456789UL, systemId: 7, tick: 42);
            var b = Pcg32.Stream(worldSeed: 123456789UL, systemId: 7, tick: 42);
            for (int i = 0; i < 100; i++)
            {
                Assert.That(a.NextUInt(), Is.EqualTo(b.NextUInt()));
            }
        }

        [Test]
        public void Different_System_Or_Tick_Diverges()
        {
            var a = Pcg32.Stream(1UL, systemId: 1, tick: 0);
            var b = Pcg32.Stream(1UL, systemId: 2, tick: 0);
            var c = Pcg32.Stream(1UL, systemId: 1, tick: 1);
            Assert.That(a.NextUInt(), Is.Not.EqualTo(b.NextUInt()));
            var a2 = Pcg32.Stream(1UL, systemId: 1, tick: 0);
            Assert.That(a2.NextUInt(), Is.Not.EqualTo(c.NextUInt()));
        }

        [Test]
        public void NextInt_Stays_In_Range()
        {
            var rng = Pcg32.Seeded(1UL, 1UL);
            for (int i = 0; i < 10000; i++)
            {
                int v = rng.NextInt(6);
                Assert.That(v, Is.InRange(0, 5));
            }
        }
    }
}
