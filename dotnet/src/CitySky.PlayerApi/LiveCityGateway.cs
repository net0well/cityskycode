using System;

namespace CitySky
{
    /// <summary>Gateway "ao vivo" do esqueleto (F1-80): por enquanto só registra no console do jogo.</summary>
    public sealed class LiveCityGateway : ICityGateway
    {
        /// <inheritdoc />
        public void Log(string message) => Console.WriteLine(message);
    }
}
