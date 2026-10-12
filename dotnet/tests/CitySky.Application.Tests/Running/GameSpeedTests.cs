using CitySky.Application;
using NUnit.Framework;

namespace CitySky.Application.Tests.Running
{
    [TestFixture]
    public sealed class GameSpeedTests
    {
        [Test]
        public void Ticks_Per_Second_Mapping()
        {
            Assert.That(GameSpeed.Pause.TicksPerSecond, Is.EqualTo(0));
            Assert.That(GameSpeed.Normal.TicksPerSecond, Is.EqualTo(4));
            Assert.That(GameSpeed.Fast.TicksPerSecond, Is.EqualTo(8));
            Assert.That(GameSpeed.Faster.TicksPerSecond, Is.EqualTo(12));
            Assert.That(GameSpeed.VeryFast.TicksPerSecond, Is.EqualTo(20));
            Assert.That(GameSpeed.Fastest.TicksPerSecond, Is.EqualTo(40));
        }

        [Test]
        public void Multiplier_And_Pause()
        {
            Assert.That(GameSpeed.Pause.IsPaused, Is.True);
            Assert.That(GameSpeed.Fastest.Multiplier, Is.EqualTo(10));
            Assert.That(GameSpeed.Fastest.ToString(), Is.EqualTo("10x"));
            Assert.That(GameSpeed.Pause.ToString(), Is.EqualTo("Pausa"));
        }
    }
}
