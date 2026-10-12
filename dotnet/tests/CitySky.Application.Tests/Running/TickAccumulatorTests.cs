using CitySky.Application;
using NUnit.Framework;

namespace CitySky.Application.Tests.Running
{
    [TestFixture]
    public sealed class TickAccumulatorTests
    {
        private static int ConsumeAll(TickAccumulator acc)
        {
            int ran = 0;
            while (acc.TryConsume(ran))
            {
                ran++;
            }

            return ran;
        }

        [Test]
        public void Accumulates_Whole_Ticks()
        {
            var acc = new TickAccumulator();
            acc.Accumulate(0.1, GameSpeed.Fastest); // 0,1 s × 40 = 4 ticks
            Assert.That(ConsumeAll(acc), Is.EqualTo(4));
        }

        [Test]
        public void Caps_Ticks_Per_Frame()
        {
            var acc = new TickAccumulator();
            for (int i = 0; i < 5; i++)
            {
                acc.Accumulate(0.1, GameSpeed.Fastest); // acumula 20 ticks de backlog
            }

            Assert.That(ConsumeAll(acc), Is.EqualTo(TickAccumulator.MaxTicksPerFrame));
        }

        [Test]
        public void Clamps_Large_Delta()
        {
            var acc = new TickAccumulator();
            acc.Accumulate(100.0, GameSpeed.Normal); // delta clampado a 0,1 s → 0,4 ticks
            Assert.That(acc.Due, Is.EqualTo(0.4).Within(1e-9));
        }

        [Test]
        public void Backlog_Clamped_Flags_Limited()
        {
            var acc = new TickAccumulator();
            for (int i = 0; i < 20; i++)
            {
                acc.Accumulate(0.1, GameSpeed.Fastest); // 80 ticks de backlog, sem consumir
            }

            bool limited = acc.CloseFrame();
            Assert.That(limited, Is.True);
            Assert.That(acc.Due, Is.EqualTo(TickAccumulator.MaxBacklogTicks));
        }
    }
}
