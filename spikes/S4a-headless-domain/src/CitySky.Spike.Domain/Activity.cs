namespace CitySky.Spike.Domain
{
    /// <summary>Atividade do cidadão (CITIZEN_SYSTEM.md §3), sem "LeavingCity" no spike.</summary>
    public enum Activity : byte
    {
        Sleeping = 0,
        AtHome = 1,
        TravelingToWork = 2,
        Working = 3,
        TravelingToShop = 4,
        Shopping = 5,
        TravelingHome = 6,
    }
}
