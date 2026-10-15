namespace CitySky.Contracts
{
    /// <summary>Handshake inicial.</summary>
    public sealed class Hello
    {
        public int ProtocolVersion { get; init; } = Protocol.Version;
        public string GameVersion { get; init; } = string.Empty;
    }
}
