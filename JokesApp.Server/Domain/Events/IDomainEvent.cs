using System;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Contratto base per tutti gli eventi di dominio.
    /// Espone il timestamp di occorrenza dell'evento (atteso in UTC).
    /// </summary>
    public interface IDomainEvent
    {
        #region Properties

        /// <summary>
        /// Timestamp di occorrenza dell'evento (UTC).
        /// </summary>
        DateTime OccurredOn { get; }

        #endregion
    }
}
