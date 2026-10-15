namespace CitySky.Contracts
{
    /// <summary>Envelope <c>{v, id, type, payload}</c>. Genérico para dar forma de golden JSON (F1-60).</summary>
    public sealed class Envelope<TPayload>
    {
        public int V { get; init; } = Protocol.Version;
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public TPayload? Payload { get; init; }
    }
}
