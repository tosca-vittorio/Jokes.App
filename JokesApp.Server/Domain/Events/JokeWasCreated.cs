using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Evento di dominio che indica la creazione di una nuova barzelletta.
    /// Registra i principali dati di stato iniziale dell'entità.
    /// </summary>
    public sealed class JokeWasCreated : DomainEvent
    {
        #region Properties

        /// <summary>
        /// Identificatore tipizzato della barzelletta appena creata.
        /// Nel dominio attuale è sempre valorizzato (non <see cref="JokeId.Empty"/>).
        /// </summary>
        public JokeId JokeId { get; }

        /// <summary>
        /// Identificativo dell'autore che ha creato la barzelletta.
        /// </summary>
        public UserId AuthorId { get; }

        /// <summary>
        /// Testo della domanda fornita al momento della creazione.
        /// </summary>
        public QuestionText Question { get; }

        /// <summary>
        /// Testo della risposta fornita al momento della creazione.
        /// </summary>
        public AnswerText Answer { get; }

        /// <summary>
        /// Timestamp UTC in cui la barzelletta è stata creata.
        /// </summary>
        public DateTime CreatedAt { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Crea un evento di creazione completamente valido assicurando
        /// coerenza e correttezza dei dati di dominio.
        /// </summary>
        /// <param name="jokeId">Identificatore tipizzato della barzelletta appena creata (non vuoto).</param>
        /// <param name="authorId">Identificativo dell'autore.</param>
        /// <param name="question">Testo della domanda.</param>
        /// <param name="answer">Testo della risposta.</param>
        /// <param name="createdAt">Istante di creazione della barzelletta (UTC).</param>
        /// <exception cref="DomainValidationException">Generata quando uno dei valori di dominio risulta non valido.</exception>
        public JokeWasCreated(
            JokeId jokeId,
            UserId authorId,
            QuestionText question,
            AnswerText answer,
            DateTime createdAt)
        {
            if (jokeId.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(jokeId));
            }

            if (authorId.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(authorId));
            }

            if (question is null || question.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(question));
            }

            if (answer is null || answer.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(answer));
            }

            if (createdAt == default)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeCreatedAtInvalid,
                    nameof(createdAt));
            }

            JokeId = jokeId;
            AuthorId = authorId;
            Question = question;
            Answer = answer;

            CreatedAt = createdAt.Kind == DateTimeKind.Utc
                ? createdAt
                : createdAt.ToUniversalTime();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Fornisce una rappresentazione stringa leggibile dell'evento.
        /// </summary>
        public override string ToString()
            => $"[JokeWasCreated] JokeId={JokeId}, AuthorId={AuthorId}, " +
               $"CreatedAt={CreatedAt:O}, OccurredOn={OccurredOn:O}";

        #endregion
    }
}
