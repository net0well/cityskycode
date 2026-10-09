namespace CitySky.Spike.EditorHost;

/// <summary>Stub mínimo da Player API (PLAYER_API.md §2) só para alimentar completions e diagnósticos.</summary>
internal static class PlayerApiStub
{
    public const string Source = """
        namespace CitySky
        {
            /// <summary>Ponto de entrada: a cidade em que você está jogando.</summary>
            public static class CityGame
            {
                public static City Current => null!;
            }

            public sealed class City
            {
                public string Name => "";
                public int Population => 0;
                public Budget Budget => null!;
                public EnergyReport Energy => null!;
                public BuildResult Build(Building building) => default;
                public BuildResult Build(Building building, Lot lot) => default;
                public System.Collections.Generic.IReadOnlyList<Lot> FindFreeLots() => null!;
                public System.Collections.Generic.IReadOnlyList<Building> GetBuildings() => null!;
                public void Log(string message) { }
                public void Log(object value) { }
            }

            public abstract class Building
            {
                public bool IsBuilt => false;
                public decimal Cost => 0m;
                public bool IsPowered => false;
            }

            public enum Insulation { Basic, Good, Excellent }

            public sealed class House : Building
            {
                public int Residents { get; set; } = 1;
                public Insulation Insulation { get; set; }
                public bool HasSolarPanels { get; set; }
                public double EnergyConsumption => 0;
            }

            public sealed class Shop : Building { public int Jobs => 4; }
            public sealed class PowerPlant : Building { public double CapacityKw => 0; }
            public sealed class Lot { public bool IsFree => true; public string Street => ""; }
            public sealed class Budget { public decimal Treasury => 0m; }
            public sealed class EnergyReport { public double BalanceKw => 0; }
            public readonly struct BuildResult { public bool Success => false; public string Message => ""; }
        }
        """;
}
