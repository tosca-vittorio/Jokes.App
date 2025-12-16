using System.Text.RegularExpressions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta l'indirizzo email di un utente.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record EmailAddress
    {
        /// <summary>
        /// Lunghezza massima consentita per l'indirizzo email.
        /// </summary>
        public const int MaxLength = 256;

        /// <summary>
        /// Espressione regolare per la validazione dell'indirizzo email.
        /// Allinea la logica del dominio a quella dell'attributo CustomEmailAttribute.
        /// </summary>
        private static readonly Regex EmailRegex = new(
            // Consente lettere, numeri, punti, trattini, underscore nella local part,
            // dominio con label separate da punto e TLD di almeno 2 caratteri.
            @"^[A-Za-z0-9._%+-]+@([A-Za-z0-9]+(-[A-Za-z0-9]+)*\.)+[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Valore testuale interno dell'indirizzo email.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il valore rappresenta uno stato vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Restituisce la lunghezza del testo interno.
        /// </summary>
        public int Length => Value.Length;

        /// <summary>
        /// Costruttore privato per garantire l'immutabilità
        /// e la validazione centralizzata tramite <see cref="Create"/>.
        /// </summary>
        /// <param name="value">Valore testuale già validato.</param>
        private EmailAddress(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Factory method che valida e crea un nuovo Value Object
        /// conforme alle regole del dominio ApplicationUser.
        /// </summary>
        /// <param name="value">Stringa contenente l'indirizzo email.</param>
        /// <returns>Un'istanza valida di <see cref="EmailAddress"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata se il valore è nullo, vuoto, troppo lungo o non rispetta il formato email.
        /// </exception>
        public static EmailAddress Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Email is required at domain level.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailRequired,
                    nameof(EmailAddress));
            }

            // Normalize input by trimming leading/trailing whitespace.
            string v = value.Trim();

            if (v.Length > MaxLength)
            {
                // Email exceeds maximum allowed length.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailTooLong,
                    nameof(EmailAddress));
            }

            if (!EmailRegex.IsMatch(v))
            {
                // Email format is invalid.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailInvalid,
                    nameof(EmailAddress));
            }

            return new EmailAddress(v);
        }

        /// <summary>
        /// Istanza vuota, utile per scenari di default, EF Core o binding iniziale.
        /// </summary>
        public static EmailAddress Empty { get; } = new EmailAddress(string.Empty);

        /// <summary>
        /// Restituisce il valore testuale dell'indirizzo email.
        /// </summary>
        public override string ToString() => Value;
    }
}
