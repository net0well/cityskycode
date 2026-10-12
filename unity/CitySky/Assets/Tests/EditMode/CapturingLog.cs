using System.Collections.Generic;
using CitySky.Application;

namespace CitySky.EditModeTests
{
    /// <summary>Dublê de teste da porta <see cref="ILog"/>: guarda as mensagens.</summary>
    internal sealed class CapturingLog : ILog
    {
        public List<string> Messages { get; } = new List<string>();

        public void Info(string message) => Messages.Add(message);

        public void Warning(string message) => Messages.Add(message);

        public void Error(string message) => Messages.Add(message);
    }
}
