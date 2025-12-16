using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta l'URL dell'avatar di un utente.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record AvatarUrl
    {
        /// <summary>
        /// Lunghezza massima consentita per l'URL dell'avatar.
        /// </summary>
        public const int MaxLength = 2048;

        /// <summary>
        /// Valore testuale interno dell'URL dell'avatar.
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
        /// e la validazione centralizzata tramite Create().
        /// </summary>
        /// <param name="value">Valore testuale già validato.</param>
        private AvatarUrl(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Factory method che valida e crea un nuovo Value Object
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">
        /// Stringa contenente l'URL dell'avatar. Può essere nulla o vuota
        /// per indicare l'assenza di avatar.
        /// </param>
        /// <returns>Un'istanza valida di <see cref="AvatarUrl"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata se l'URL non è valido o eccede la lunghezza massima consentita.
        /// </exception>
        public static AvatarUrl Create(string? value)
        {
            // If no avatar is provided, return the Empty instance.
            if (string.IsNullOrWhiteSpace(value))
            {
                return Empty;
            }

            // Normalize input by trimming leading/trailing whitespace.
            string v = value.Trim();

            if (v.Length > MaxLength)
            {
                // Avatar URL exceeds maximum allowed length.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.AvatarUrlMaxLength,
                    nameof(AvatarUrl));
            }

            // Validate URL format (absolute HTTP/HTTPS URL).
            if (!Uri.TryCreate(v, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                // Avatar URL is not a valid HTTP/HTTPS URL.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.AvatarUrlInvalid,
                    nameof(AvatarUrl));
            }

            return new AvatarUrl(v);
        }

        /// <summary>
        /// Istanza vuota, utile per scenari di default, EF Core o binding iniziale,
        /// e per rappresentare l'assenza di un avatar impostato.
        /// </summary>
        public static AvatarUrl Empty { get; } = new AvatarUrl(string.Empty);

        /// <summary>
        /// Restituisce il valore testuale dell'URL dell'avatar.
        /// </summary>
        public override string ToString() => Value;
    }
}
