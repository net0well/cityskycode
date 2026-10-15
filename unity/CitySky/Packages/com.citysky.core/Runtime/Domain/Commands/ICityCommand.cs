namespace CitySky.Domain
{
    /// <summary>
    /// Toda mutação de estado é um comando validado (regra inviolável) — padrão Command. Aplicado no tick alvo.
    /// </summary>
    public interface ICityCommand
    {
        CommandResult Apply(CityState state, in SimulationContext context);
    }
}
