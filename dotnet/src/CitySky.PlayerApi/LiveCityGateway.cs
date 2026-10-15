using System;

namespace CitySky
{
    /// <summary>
    /// Gateway "ao vivo" do esqueleto (F1-80): por enquanto só registra no console do jogo. Internal —
    /// o jogador chega à cidade por <see cref="CityGame.Current"/>, não constrói o gateway ao vivo
    /// (reduz a superfície exposta; revisão F1-81). <see cref="ICityGateway"/> continua público p/ fakes.
    /// </summary>
    internal sealed class LiveCityGateway : ICityGateway
    {
        /// <inheritdoc />
        public void Log(string message) => Console.WriteLine(message);
    }
}
