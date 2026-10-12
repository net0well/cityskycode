using System;

namespace CitySky.Domain
{
    /// <summary>Resultado da aplicação de um comando.</summary>
    public readonly struct CommandResult : IEquatable<CommandResult>
    {
        public readonly bool Success;
        public readonly CommandError Error;

        private CommandResult(bool success, CommandError error)
        {
            Success = success;
            Error = error;
        }

        public static readonly CommandResult Ok = new CommandResult(true, CommandError.None);
        public static CommandResult Fail(CommandError error) => new CommandResult(false, error);

        public bool Equals(CommandResult other) => Success == other.Success && Error == other.Error;
        public override bool Equals(object? obj) => obj is CommandResult o && Equals(o);
        public override int GetHashCode() => (Success ? 1 : 0) ^ ((int)Error << 1);
        public static bool operator ==(CommandResult a, CommandResult b) => a.Equals(b);
        public static bool operator !=(CommandResult a, CommandResult b) => !a.Equals(b);
        public override string ToString() => Success ? "Ok" : "Fail(" + Error + ")";
    }
}
