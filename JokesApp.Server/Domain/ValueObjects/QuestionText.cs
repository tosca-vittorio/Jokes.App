using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta il testo della domanda di una barzelletta.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record QuestionText
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita per la domanda.
        /// </summary>
        public const int MaxLength = 200;

        #endregion

        #region Properties

        /// <summary>
        /// Valore testuale della domanda.
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
        /// Costruttore privato: l'unica via per creare il VO è tramite <see cref="Create(string?)"/>.
        /// </summary>
        /// <param name="value">Testo della domanda già validato.</param>
        private QuestionText(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un nuovo <see cref="QuestionText"/>
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">Testo della domanda da validare.</param>
        /// <returns>Un'istanza valida di <see cref="QuestionText"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static QuestionText Create(string? value)
        {
            if (value is null)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(QuestionText));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length == 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(QuestionText));
            }

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionTooLong,
                    nameof(QuestionText));
            }

            // At this point the value is valid according to the domain rules.
            return new QuestionText(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Rappresenta un valore vuoto, utile come placeholder per scenari tecnici di mapping/persistenza, test o binding.
        /// </summary>
        public static QuestionText Empty { get; } = new QuestionText(string.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce il valore testuale interno.
        /// </summary>
        public override string ToString() => Value;

        #endregion
    }
}