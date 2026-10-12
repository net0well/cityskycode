using System;

namespace CitySky
{
    /// <summary>
    /// Ponte entre o código do jogador e o jogo. No modo "ao vivo" (<see cref="LiveCityGateway"/>) as
    /// chamadas viram comandos/consultas validados; nos testes do jogador, uma implementação falsa.
    /// </summary>
    public interface ICityGateway
    {
        /// <summary>Escreve uma mensagem no console do jogo.</summary>
        void Log(string message);
    }

    /// <summary>Gateway "ao vivo" do esqueleto (F1-80): por enquanto só registra no console do jogo.</summary>
    public sealed class LiveCityGateway : ICityGateway
    {
        /// <inheritdoc />
        public void Log(string message) => Console.WriteLine(message);
    }

    /// <summary>
    /// A sua cidade. Ponto de partida de tudo o que o seu programa faz. Obtenha a cidade atual com
    /// <see cref="CityGame.Current"/>.
    /// </summary>
    public sealed class City
    {
        private readonly ICityGateway _gateway;

        internal City(ICityGateway gateway)
        {
            _gateway = gateway;
        }

        /// <summary>Escreve uma mensagem no console do jogo.</summary>
        /// <param name="message">O texto a mostrar.</param>
        public void Log(string message) => _gateway.Log(message);

        /// <summary>Escreve um valor qualquer no console do jogo (chama <c>ToString</c>).</summary>
        /// <param name="value">O valor a mostrar.</param>
        public void Log(object value) => _gateway.Log(value?.ToString() ?? string.Empty);
    }

    /// <summary>Ponto de entrada da Player API: dá acesso à cidade atual.</summary>
    public static class CityGame
    {
        private static readonly City LiveCity = new City(new LiveCityGateway());

        /// <summary>A cidade atual, que o seu programa controla.</summary>
        public static City Current => LiveCity;
    }
}
