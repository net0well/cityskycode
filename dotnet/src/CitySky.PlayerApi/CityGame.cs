namespace CitySky
{
    /// <summary>Ponto de entrada da Player API: dá acesso à cidade atual.</summary>
    public static class CityGame
    {
        private static readonly City LiveCity = new City(new LiveCityGateway());

        /// <summary>A cidade atual, que o seu programa controla.</summary>
        public static City Current => LiveCity;
    }
}
