using System;
using System.Collections.Generic;

namespace CitySky.Domain
{
    /// <summary>Erros tipados de comando (falhas esperadas, não exceções). ARCHITECTURE §9.</summary>
    public enum CommandError
    {
        None = 0,
        InsufficientFunds = 1,
        InvalidTarget = 2,
        NotUnlocked = 3,
        Rejected = 4,
    }

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

    /// <summary>
    /// Toda mutação de estado é um comando validado (regra inviolável). Aplicado no tick alvo.
    /// </summary>
    public interface ICityCommand
    {
        CommandResult Apply(CityState state, in SimulationContext context);
    }

    /// <summary>
    /// Fila de comandos ordenada por <c>(targetTick, seq)</c> — determinística (F1-34).
    /// </summary>
    public sealed class CommandQueue
    {
        private readonly struct Entry
        {
            public readonly long TargetTick;
            public readonly long Seq;
            public readonly ICityCommand Command;

            public Entry(long targetTick, long seq, ICityCommand command)
            {
                TargetTick = targetTick;
                Seq = seq;
                Command = command;
            }
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private long _seq;

        public int Count => _entries.Count;

        /// <summary>Enfileira um comando para aplicar no tick alvo (ou no próximo, se já passou).</summary>
        public void Enqueue(ICityCommand command, long targetTick)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _entries.Add(new Entry(targetTick, _seq++, command));
        }

        /// <summary>
        /// Remove e devolve, em ordem <c>(targetTick, seq)</c>, os comandos cujo tick alvo já chegou (≤ tick).
        /// </summary>
        public List<ICityCommand> DrainDue(long tick)
        {
            var due = new List<Entry>();
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].TargetTick <= tick)
                {
                    due.Add(_entries[i]);
                    _entries.RemoveAt(i);
                }
            }

            due.Sort(static (a, b) =>
            {
                int c = a.TargetTick.CompareTo(b.TargetTick);
                return c != 0 ? c : a.Seq.CompareTo(b.Seq);
            });

            var result = new List<ICityCommand>(due.Count);
            for (int i = 0; i < due.Count; i++)
            {
                result.Add(due[i].Command);
            }

            return result;
        }
    }
}
