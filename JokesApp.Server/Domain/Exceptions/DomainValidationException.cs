using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione generata quando i dati di input non rispettano
    /// le regole di validazione definite nel dominio.
    /// </summary>
    public class DomainValidationException : DomainException
    {
        #region Properties

        /// <summary>
        /// Nome logico del membro (proprietà, Value Object o campo) che ha
        /// causato la violazione di validazione. Può essere nullo.
        /// </summary>
        public string? MemberName { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        public DomainValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> indicando anche
        /// il membro che ha causato la violazione di validazione.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="memberName">Nome del membro (es. "QuestionText", "AnswerText").</param>
        public DomainValidationException(string message, string? memberName)
            : base(message)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> con un messaggio descrittivo
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public DomainValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> indicando il membro coinvolto
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="memberName">Nome del membro (es. "QuestionText", "AnswerText").</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public DomainValidationException(string message, string? memberName, Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
        }

        #endregion
    }
}
