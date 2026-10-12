namespace CitySky.Domain
{
    /// <summary>Emitido quando um novo dia de calendário começa (pelo <see cref="CalendarSystem"/>).</summary>
    public readonly struct DayStartedEvent : IDomainEvent
    {
        public readonly long DayOfGame;
        public DayStartedEvent(long dayOfGame) { DayOfGame = dayOfGame; }
    }
}
