namespace CitySky.Application
{
    /// <summary>Orçamento sem limite de tempo (Null Object): usado em testes e na simulação headless.</summary>
    public sealed class UnlimitedSimulationBudget : ISimulationBudget
    {
        public static readonly UnlimitedSimulationBudget Instance = new UnlimitedSimulationBudget();

        public void BeginFrame()
        {
        }

        public bool HasTimeLeft() => true;
    }
}
