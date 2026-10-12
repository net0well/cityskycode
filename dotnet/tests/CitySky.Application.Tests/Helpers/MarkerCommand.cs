using CitySky.Domain;

namespace CitySky.Application.Tests
{
    /// <summary>Comando de teste que registra quando (e se) foi aplicado.</summary>
    internal sealed class MarkerCommand : ICityCommand
    {
        public bool Applied { get; private set; }

        public long AppliedAtTick { get; private set; }

        public CommandResult Apply(CityState state, in SimulationContext context)
        {
            Applied = true;
            AppliedAtTick = context.Tick;
            return CommandResult.Ok;
        }
    }
}
