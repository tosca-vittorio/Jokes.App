using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Identificatore tipizzato dell'utente, conforme alle regole del dominio
    /// e ai vincoli di lunghezza di Identity Core.
    /// Immutabile, Auto-validante se creato tramite Create; supporta uno stato tecnico Empty per placeholder.
    /// </summary>
    public readonly record struct UserId
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita per un identificativo utente (Identity Core).
        /// </summary>
        public const int MaxLength = 450;

        #endregion

        #region Properties

        /// <summary>
        /// Valore stringa dell'identificativo utente.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il valore corrente è vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato: nel codice applicativo la creazione dovrebbe passare
        /// tramite <see cref="Create(string?)"/> oppure tramite <see cref="Empty"/>.
        /// Come per tutti gli struct in C#, esiste comunque un costruttore di default
        /// che produce uno stato equivalente a <see cref="Empty"/> (Value nullo o vuoto).
        /// </summary>

        private UserId(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un identificativo coerente con le regole del dominio.
        /// </summary>
        /// <param name="value">Valore stringa da utilizzare come identificativo utente.</param>
        /// <returns>Un'istanza valida di <see cref="UserId"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static UserId Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(UserId));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdTooLong,
                    nameof(UserId));
            }

            // At this point, the identifier is a valid domain value.
            return new UserId(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Rappresenta un identificativo vuoto o non inizializzato.
        /// Usato come placeholder per scenari tecnici di mapping/persistenza e scenari di default.
        /// </summary>
        public static UserId Empty { get; } = new UserId(string.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce la rappresentazione testuale dell'identificativo utente.
        /// </summary>
        public override string ToString() => Value ?? string.Empty;

        #endregion
    }
}
