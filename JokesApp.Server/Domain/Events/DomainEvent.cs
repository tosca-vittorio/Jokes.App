using System;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Implementazione base di un evento di dominio.
    /// Contiene la data/ora di occorrenza (UTC).
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        #region Properties

        /// <summary>
        /// Timestamp di occorrenza dell'evento (UTC).
        /// </summary>
        public DateTime OccurredOn { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Inizializza un evento di dominio impostando <see cref="OccurredOn"/> in UTC.
        /// </summary>
        protected DomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }

        #endregion
    }
}
