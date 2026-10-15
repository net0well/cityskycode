namespace CitySky.Domain
{
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
}
