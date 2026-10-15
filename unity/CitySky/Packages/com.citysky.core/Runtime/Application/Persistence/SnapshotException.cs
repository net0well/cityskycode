using System;

namespace CitySky.Application
{
    /// <summary>
    /// Falha ao restaurar um save: esquema incompatível ou hash divergente (arquivo corrompido ou
    /// adulterado). Lançada por <see cref="SnapshotService.Restore"/> (F1-101).
    /// </summary>
    public sealed class SnapshotException : Exception
    {
        public SnapshotException(string message)
            : base(message)
        {
        }
    }
}
