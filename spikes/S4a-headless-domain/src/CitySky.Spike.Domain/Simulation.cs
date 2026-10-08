namespace CitySky.Spike.Domain
{
    /// <summary>Ordem fixa dos sistemas (SIMULATION_ARCHITECTURE.md §2.2), só os do spike.</summary>
    public sealed class Simulation
    {
        public Simulation(CityState state)
        {
            State = state;
        }

        public CityState State { get; }

        public long Tick { get; private set; }

        public void RunTick()
        {
            long tick = Tick;
            ScheduleSystem.Execute(State, tick);
            TripSystem.Execute(State, tick);
            NeedsSystem.Execute(State, tick);
            HappinessSystem.Execute(State, tick);
            Tick = tick + 1;
        }
    }
}
