using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione base astratta per tutti gli errori del dominio.
    /// Utilizzata per rappresentare violazioni delle regole di business
    /// e degli invarianti definiti nel Domain Layer.
    /// </summary>
    public abstract class DomainException : Exception
    {
        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/>.
        /// </summary>
        protected DomainException()
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio di errore descrittivo.</param>
        protected DomainException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/> con un messaggio descrittivo
        /// e un'eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio di errore descrittivo.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        protected DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Inner exception is preserved to keep the original stack trace.
        }

        #endregion
    }
}
