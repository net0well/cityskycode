namespace CitySky.Spike.CodeHost;

/// <summary>Stub da Player API (PLAYER_API.md §2) contra o qual o código do aluno compila.</summary>
public static class PlayerApiStub
{
    public const string Source = """
        namespace CitySky
        {
            public static class CityGame { public static City Current => Live; private static readonly City Live = new City(); }

            public sealed class City
            {
                public string Name => "Nova Esperança";
                public int Population => 48;
                public Budget Budget { get; } = new Budget();
                public BuildResult Build(Building building) => new BuildResult(true, "Casa construída");
                public BuildResult Build(Building building, Lot lot) => new BuildResult(true, "Casa construída no lote");
                public System.Collections.Generic.IReadOnlyList<Lot> FindFreeLots() => new System.Collections.Generic.List<Lot>();
                public System.Collections.Generic.IReadOnlyList<Building> GetBuildings() => new System.Collections.Generic.List<Building>();
                public void Log(string message) => System.Console.WriteLine(message);
                public void Log(object value) => System.Console.WriteLine(value);
            }

            public abstract class Building
            {
                public bool IsBuilt => false;
                public decimal Cost => 12400m;
                public bool IsPowered => true;
            }

            public enum Insulation { Basic, Good, Excellent }

            public sealed class House : Building
            {
                public int Residents { get; set; } = 1;
                public Insulation Insulation { get; set; }
                public bool HasSolarPanels { get; set; }
                public double EnergyConsumption => 1.9;
            }

            public sealed class Shop : Building { public int Jobs => 4; }
            public sealed class PowerPlant : Building { public double CapacityKw => 80; }
            public sealed class Lot { public bool IsFree => true; public string Street => "Rua A"; }
            public sealed class Budget { public decimal Treasury => 87600m; }
            public readonly struct BuildResult
            {
                public BuildResult(bool success, string message) { Success = success; Message = message; }
                public bool Success { get; }
                public string Message { get; }
            }
        }
        """;
}
