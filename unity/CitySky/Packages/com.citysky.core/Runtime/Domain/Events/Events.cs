using System;
using System.Collections.Generic;

namespace CitySky.Domain
{
    /// <summary>Marcador de evento de domínio. Implementações são <c>readonly struct</c>.</summary>
    public interface IDomainEvent
    {
    }

    /// <summary>
    /// Acumula eventos durante o tick e os publica <b>depois</b> dele, em ordem determinística (F1-35).
    /// Proibido callback do domínio direto para a Unity: assinantes são read models/sistemas.
    /// (Boxing de structs aqui é aceito no esqueleto; buffers tipados por evento virão na escala — O8.)
    /// </summary>
    public sealed class DomainEventBuffer
    {
        private readonly List<IDomainEvent> _pending = new List<IDomainEvent>();
        private readonly List<Action<IDomainEvent>> _subscribers = new List<Action<IDomainEvent>>();

        public int PendingCount => _pending.Count;

        public void Subscribe(Action<IDomainEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _subscribers.Add(handler);
        }

        /// <summary>Emite durante o tick; só é despachado no <see cref="Flush"/>.</summary>
        public void Emit(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            _pending.Add(domainEvent);
        }

        /// <summary>Despacha os eventos pendentes na ordem de emissão e limpa o buffer.</summary>
        public void Flush()
        {
            // Índice explícito: a ordem de emissão é a ordem de despacho.
            for (int i = 0; i < _pending.Count; i++)
            {
                IDomainEvent e = _pending[i];
                for (int s = 0; s < _subscribers.Count; s++)
                {
                    _subscribers[s](e);
                }
            }

            _pending.Clear();
        }
    }
}
