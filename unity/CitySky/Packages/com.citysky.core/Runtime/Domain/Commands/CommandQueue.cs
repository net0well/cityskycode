using System;
using System.Collections.Generic;

namespace CitySky.Domain
{
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
