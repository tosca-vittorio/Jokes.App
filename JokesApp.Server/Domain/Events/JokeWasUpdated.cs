using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Evento di dominio che rappresenta l’avvenuto aggiornamento di una barzelletta.
    /// Registra i nuovi valori e il timestamp dell'operazione.
    /// </summary>
    public sealed class JokeWasUpdated : DomainEvent
    {
        #region Properties

        /// <summary>
        /// Identificatore tipizzato della barzelletta aggiornata.
        /// </summary>
        public JokeId JokeId { get; }

        /// <summary>
        /// Nuova domanda della barzelletta.
        /// </summary>
        public QuestionText NewQuestion { get; }

        /// <summary>
        /// Nuova risposta della barzelletta.
        /// </summary>
        public AnswerText NewAnswer { get; }

        /// <summary>
        /// Timestamp UTC dell'aggiornamento dell'entità.
        /// </summary>
        public DateTime UpdatedAt { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Costruisce un evento di aggiornamento coerente secondo le principali regole di dominio.
        /// </summary>
        /// <param name="jokeId">Identificatore della barzelletta aggiornata.</param>
        /// <param name="newQuestion">Nuova domanda.</param>
        /// <param name="newAnswer">Nuova risposta.</param>
        /// <param name="updatedAt">Istante di aggiornamento dell'entità (UTC).</param>
        /// <exception cref="DomainValidationException">
        /// Generata quando uno dei valori di dominio risulta non valido.
        /// </exception>
        public JokeWasUpdated(
            JokeId jokeId,
            QuestionText newQuestion,
            AnswerText newAnswer,
            DateTime updatedAt)
        {
            if (jokeId.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(jokeId));
            }

            JokeId = jokeId;

            NewQuestion = newQuestion
                ?? throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(newQuestion));

            NewAnswer = newAnswer
                ?? throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(newAnswer));

            if (NewQuestion.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(newQuestion));
            }

            if (NewAnswer.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(newAnswer));
            }

            if (updatedAt == default)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeUpdatedAtInvalid,
                    nameof(updatedAt));
            }

            if (updatedAt.Kind != DateTimeKind.Utc)
            {
                updatedAt = updatedAt.ToUniversalTime();
            }

            UpdatedAt = updatedAt;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Rappresentazione leggibile per log e debugging.
        /// </summary>
        public override string ToString()
            => $"[JokeWasUpdated] JokeId={JokeId}, UpdatedAt={UpdatedAt:O}, OccurredOn={OccurredOn:O}";

        #endregion
    }
}
