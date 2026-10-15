namespace CitySky
{
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
}
