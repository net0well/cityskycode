using System;
using System.IO;
using CitySky.Contracts;
using NUnit.Framework;

namespace CitySky.Contracts.Tests
{
    [TestFixture]
    public sealed class FramingTests
    {
        [Test]
        public void Roundtrip_Preserves_Payload()
        {
            byte[] payload = { 1, 2, 3, 250, 255, 0, 42 };
            using var stream = new MemoryStream();
            MessageFraming.WriteFrame(stream, payload);
            stream.Position = 0;

            bool read = MessageFraming.TryReadFrame(stream, out byte[] got);
            Assert.That(read, Is.True);
            Assert.That(got, Is.EqualTo(payload));
        }

        [Test]
        public void Clean_Eof_Returns_False()
        {
            using var stream = new MemoryStream();
            Assert.That(MessageFraming.TryReadFrame(stream, out _), Is.False);
        }

        [Test]
        public void Truncated_Body_Throws()
        {
            using var stream = new MemoryStream();
            // Cabeçalho anuncia 10 bytes; só 3 presentes.
            stream.Write(new byte[] { 10, 0, 0, 0, 1, 2, 3 });
            stream.Position = 0;
            Assert.Throws<EndOfStreamException>(() => MessageFraming.TryReadFrame(stream, out _));
        }

        [Test]
        public void Oversize_Write_Throws()
        {
            using var stream = new MemoryStream();
            var big = new byte[Protocol.MaxMessageBytes + 1];
            Assert.Throws<InvalidDataException>(() => MessageFraming.WriteFrame(stream, big));
        }

        [Test]
        public void Oversize_Announced_Length_Throws()
        {
            using var stream = new MemoryStream();
            uint len = (uint)Protocol.MaxMessageBytes + 1;
            stream.Write(new[] { (byte)len, (byte)(len >> 8), (byte)(len >> 16), (byte)(len >> 24) });
            stream.Position = 0;
            Assert.Throws<InvalidDataException>(() => MessageFraming.TryReadFrame(stream, out _));
        }

        [Test]
        public void Empty_Payload_Roundtrips()
        {
            using var stream = new MemoryStream();
            MessageFraming.WriteFrame(stream, ReadOnlySpan<byte>.Empty);
            stream.Position = 0;
            Assert.That(MessageFraming.TryReadFrame(stream, out byte[] got), Is.True);
            Assert.That(got, Is.Empty);
        }
    }
}
