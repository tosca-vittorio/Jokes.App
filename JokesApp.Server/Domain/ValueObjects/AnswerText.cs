using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta il testo della risposta di una barzelletta.
    /// È immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record AnswerText
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita dal dominio per la risposta.
        /// </summary>
        public const int MaxLength = 500;

        #endregion

        #region Properties

        /// <summary>
        /// Valore testuale interno della risposta.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il testo rappresenta un valore vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Restituisce la lunghezza del testo interno.
        /// </summary>
        public int Length => Value.Length;

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato per garantire l’immutabilità
        /// e la validazione centralizzata tramite <see cref="Create(string?)"/>.
        /// </summary>
        /// <param name="value">Testo della risposta già validato.</param>
        private AnswerText(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un nuovo <see cref="AnswerText"/>
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">Testo della risposta da validare.</param>
        /// <returns>Un'istanza valida di <see cref="AnswerText"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static AnswerText Create(string? value)
        {
            if (value is null)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(AnswerText));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length == 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(AnswerText));
            }

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerTooLong,
                    nameof(AnswerText));
            }

            // At this point the value is valid according to the domain rules.
            return new AnswerText(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Istanza vuota, utile per scenari tecnici di mapping/persistenza, test o binding iniziale.
        /// </summary>
        public static AnswerText Empty { get; } = new AnswerText(string.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce il valore testuale interno.
        /// </summary>
        public override string ToString() => Value;

        #endregion
    }
}