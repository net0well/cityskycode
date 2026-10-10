using NUnit.Framework;

namespace CitySky.Domain.Tests
{
    // Fumaça: confirma que o harness NUnit (override O2, Assert.That) compila e roda.
    // Substituído pelos testes reais do núcleo do domínio no E4.
    [TestFixture]
    public sealed class HarnessSmokeTests
    {
        [Test]
        public void Harness_Runs()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }
    }
}
