namespace CitySky.Domain
{
    /// <summary>Emitido quando um novo dia de calendário começa.</summary>
    public readonly struct DayStartedEvent : IDomainEvent
    {
        public readonly long DayOfGame;
        public DayStartedEvent(long dayOfGame) { DayOfGame = dayOfGame; }
    }

    /// <summary>Incrementa um contador a cada tick. Sistema trivial para os testes de determinismo (F1-37).</summary>
    public sealed class HeartbeatSystem : ISimulationSystem
    {
        public int SystemId => 1;
        public TickSchedule Schedule => TickSchedule.EveryTick;

        public void Execute(CityState state, in SimulationContext context)
        {
            state.Heartbeat++;
        }
    }

    /// <summary>
    /// Primeiro sistema "real": conta os dias de calendário e emite <see cref="DayStartedEvent"/> na virada (F1-37).
    /// </summary>
    public sealed class CalendarSystem : ISimulationSystem
    {
        public int SystemId => 2;
        public TickSchedule Schedule => TickSchedule.EveryTick;

        public void Execute(CityState state, in SimulationContext context)
        {
            // Virada de dia: primeiro tick de cada dia, exceto o dia 1 (tick 0).
            if (context.Tick > 0 && context.Clock.TickOfDay == 0)
            {
                state.DayCounter++;
                context.Events.Emit(new DayStartedEvent(context.Clock.DayOfGame));
            }
        }
    }
}
