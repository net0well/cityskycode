namespace CitySky.Contracts
{
    /// <summary>Nomes de tipo do envelope (campo <c>type</c>).</summary>
    public static class MessageType
    {
        public const string Hello = "Hello";
        public const string CompileAndRun = "CompileAndRun";
        public const string RunResult = "RunResult";
        public const string GetDiagnostics = "GetDiagnostics";
        public const string Diagnostics = "Diagnostics";
        public const string Cancel = "Cancel";
    }
}
