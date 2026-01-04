using System;
using System.Collections.Generic;
using JokesApp.Server.Domain.Events;

namespace JokesApp.Server.Domain.Primitives
{
    /// <summary>
    /// Classe base per un Aggregate Root DDD.
    /// Gestisce la collezione di Domain Events generati durante operazioni di dominio valide.
    /// </summary>
    public abstract class AggregateRoot
    {
        #region Fields

        private readonly List<IDomainEvent> _domainEvents = new();
        private readonly IReadOnlyCollection<IDomainEvent> _domainEventsReadOnly;

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza un nuovo aggregate root predisponendo la coda eventi.
        /// </summary>
        protected AggregateRoot()
        {
            _domainEventsReadOnly = _domainEvents.AsReadOnly();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Collezione read-only degli eventi di dominio generati dall'aggregate.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEventsReadOnly;

        #endregion

        #region Protected API

        /// <summary>
        /// Aggiunge un evento di dominio alla coda dell'aggregate.
        /// </summary>
        /// <param name="domainEvent">Evento di dominio da accodare.</param>
        /// <exception cref="ArgumentNullException">Se <paramref name="domainEvent"/> null.</exception>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent is null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// Rimuove tutti gli eventi di dominio attualmente accodati.
        /// </summary>
        protected void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Estrae e rimuove gli eventi di dominio dalla coda dell'aggregate.
        /// Tipicamente usato dall'Application Layer dopo la persistenza.
        /// </summary>
        /// <returns>
        /// Snapshot read-only degli eventi presenti al momento della chiamata.
        /// </returns>
        public IReadOnlyCollection<IDomainEvent> PullDomainEvents()
        {
            if (_domainEvents.Count == 0)
            {
                return Array.Empty<IDomainEvent>();
            }

            // Copy then clear to avoid exposing internal list.
            var events = new List<IDomainEvent>(_domainEvents);
            ClearDomainEvents();

            return events.AsReadOnly();
        }

        #endregion
    }
}
