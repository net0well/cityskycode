using System;
using System.Text;
using CitySky.Domain;
using NUnit.Framework;

namespace CitySky.Domain.Tests.Hashing
{
    [TestFixture]
    public sealed class XxHash64Tests
    {
        [Test]
        public void Empty_Input_Matches_Canonical_Vector()
        {
            // Vetor canônico do xxHash64: hash("") com seed 0.
            Assert.That(XxHash64.Hash(ReadOnlySpan<byte>.Empty), Is.EqualTo(0xEF46DB3751D8E999UL));
        }

        [Test]
        public void Deterministic_And_Sensitive()
        {
            byte[] a = Encoding.UTF8.GetBytes("City: SkyCode — determinismo");
            byte[] b = Encoding.UTF8.GetBytes("City: SkyCode — determinismo!");
            Assert.That(XxHash64.Hash(a), Is.EqualTo(XxHash64.Hash(a)));
            Assert.That(XxHash64.Hash(a), Is.Not.EqualTo(XxHash64.Hash(b)));
        }

        [Test]
        public void Large_Input_Uses_Main_Loop()
        {
            var data = new byte[256];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(i * 7);
            }

            // Estável entre execuções (exercita o laço principal de 32 bytes).
            Assert.That(XxHash64.Hash(data), Is.EqualTo(XxHash64.Hash(data)));
        }
    }
}
