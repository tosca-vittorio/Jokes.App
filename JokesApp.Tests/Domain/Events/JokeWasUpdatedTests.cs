using System;
using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.Events;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.Events
{
    /// <summary>
    /// Unit Test per il Domain Event <see cref="JokeWasUpdated"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che l'evento riporti correttamente i nuovi testi (domanda/risposta) e il timestamp di aggiornamento,
    /// - verificare la validazione dei parametri (UpdatedAt non può essere default),
    /// - comprendere la sintassi Arrange/Act/Assert e l'uso di FluentAssertions per testare eccezioni.
    ///
    /// Nota DDD:
    /// - Un Domain Event è un "fatto" accaduto nel dominio.
    /// - Qui: “una Joke è stata aggiornata” e l'evento descrive il nuovo stato testuale e il momento dell'aggiornamento.
    /// </summary>
    public class JokeWasUpdatedTests
    {
        #region Constructor - Happy path

        /// <summary>
        /// Verifica che il costruttore popoli correttamente tutte le proprietà quando i valori sono validi.
        ///
        /// Nota:
        /// - <c>UpdatedAt</c> è un timestamp in UTC (scelta tipica per coerenza tra sistemi e fusi orari).
        /// </summary>
        [Fact]
        public void Constructor_ShouldPopulateProperties_WhenValuesAreValid()
        {
            // Arrange
            // Prepariamo input validi (Value Object del dominio).
            var jokeId = JokeId.New();
            var question = QuestionText.Create("New question");
            var answer = AnswerText.Create("New answer");

            // DateTime.UtcNow:
            // - usiamo UTC per non dipendere dal fuso orario della macchina.
            var updatedAt = DateTime.UtcNow;

            // Act
            // Creiamo l'evento: oggetto “immutabile” che trasporta i dati dell’aggiornamento.
            var evt = new JokeWasUpdated(jokeId, question, answer, updatedAt);

            // Assert
            // Controlliamo che le proprietà dell'evento corrispondano ai valori passati.
            evt.JokeId.Should().Be(jokeId);
            evt.NewQuestion.Should().Be(question);
            evt.NewAnswer.Should().Be(answer);
            evt.UpdatedAt.Should().Be(updatedAt);
        }

        #endregion

        #region Constructor - Validation

        /// <summary>
        /// Verifica che il costruttore lanci <see cref="DomainValidationException"/> quando <c>updatedAt</c> è default.
        ///
        /// Spiegazione:
        /// - <c>default(DateTime)</c> equivale a <c>DateTime.MinValue</c>,
        ///   ed è tipicamente un valore "non valido" per un timestamp di dominio.
        ///
        /// Nota sintattica:
        /// - Usiamo una lambda <c>() => ...</c> per consentire a FluentAssertions di intercettare l'eccezione.
        /// - Con Which.MemberName verifichiamo quale parametro ha causato l'errore ("updatedAt").
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenUpdatedAtIsDefault()
        {
            // Arrange
            // Manteniamo validi gli altri parametri per isolare la causa dell'errore: updatedAt = default.
            var jokeId = JokeId.New();
            var question = QuestionText.Create("Q");
            var answer = AnswerText.Create("A");

            // Act
            var act = () => new JokeWasUpdated(jokeId, question, answer, default);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.JokeUpdatedAtInvalid)
                .Which.MemberName.Should().Be("updatedAt");
        }

        #endregion
    }
}