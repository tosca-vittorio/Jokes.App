using System;
using FluentAssertions;
using JokesApp.Server.Domain.Entities;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.Events;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.Events
{
    /// <summary>
    /// Unit Test per il Domain Event <see cref="JokeWasCreated"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che l'evento sia un "contenitore di fatti" (fact) del dominio: dati completi e coerenti,
    /// - verificare le invarianti minime richieste dal dominio (es. JokeId non vuoto),
    /// - comprendere la sintassi Arrange/Act/Assert e le asserzioni fluenti con FluentAssertions.
    ///
    /// Nota architetturale (DDD):
    /// - Un Domain Event rappresenta qualcosa di significativo accaduto nel dominio.
    /// - L'evento dovrebbe essere immutabile: una volta creato, i suoi dati non devono cambiare.
    /// </summary>
    public class JokeWasCreatedTests
    {
        #region Constructor - Happy path

        /// <summary>
        /// Verifica che il costruttore dell'evento popoli tutte le proprietà quando i valori sono validi.
        ///
        /// In pratica: stiamo controllando che l'evento riporti correttamente il "fatto" accaduto:
        /// - quale Joke è stata creata,
        /// - da quale autore,
        /// - con quale domanda/risposta,
        /// - e quando (CreatedAt).
        /// </summary>
        [Fact]
        public void Constructor_ShouldPopulateProperties_WhenValuesAreValid()
        {
            // Arrange
            // Prepariamo input validi: in questi test usiamo i Value Object del dominio,
            // così restiamo allineati al comportamento reale dell'applicazione.
            var jokeId = JokeId.New();
            var authorId = UserId.Create("author-1");
            var question = QuestionText.Create("Why did the chicken cross the road?");
            var answer = AnswerText.Create("To get to the other side.");

            // DateTime.UtcNow:
            // - usiamo UTC per coerenza con le regole del dominio (timestamp non dipendono dal fuso orario locale).
            var createdAt = DateTime.UtcNow;

            // Act
            // Creiamo l'evento: un Domain Event è tipicamente un oggetto semplice che trasporta dati.
            var evt = new JokeWasCreated(jokeId, authorId, question, answer, createdAt);

            // Assert
            // Verifichiamo che ogni proprietà dell'evento corrisponda al valore passato al costruttore.
            evt.JokeId.Should().Be(jokeId);
            evt.AuthorId.Should().Be(authorId);
            evt.Question.Should().Be(question);
            evt.Answer.Should().Be(answer);
            evt.CreatedAt.Should().Be(createdAt);
        }

        #endregion

        #region Constructor - Validation

        /// <summary>
        /// Verifica che il costruttore lanci <see cref="DomainValidationException"/> quando <c>jokeId</c> è vuoto.
        ///
        /// Nota didattica:
        /// - Incapsuliamo la creazione dell'evento in una lambda <c>() => ...</c> per intercettare l'eccezione.
        /// - Con FluentAssertions controlliamo:
        ///   1) tipo eccezione,
        ///   2) messaggio (centralizzato in <see cref="JokeErrorMessages"/>),
        ///   3) MemberName (il nome del parametro che ha causato l'errore).
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenJokeIdIsEmpty()
        {
            // Arrange
            // Qui JokeId sarà vuoto, mentre gli altri parametri restano validi per isolare la causa dell'errore.
            var authorId = UserId.Create("author-1");
            var question = QuestionText.Create("Q?");
            var answer = AnswerText.Create("A!");

            // Act
            // Lambda per catturare l'eccezione: l'espressione non viene eseguita subito.
            var act = () => new JokeWasCreated(JokeId.Empty, authorId, question, answer, DateTime.UtcNow);

            // Assert
            // Throw<DomainValidationException>:
            // - l'azione deve fallire per violazione di regole/invarianti del dominio.
            //
            // WithMessage(...) verifica il messaggio atteso.
            // Which.MemberName verifica quale parametro è stato considerato non valido ("jokeId").
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.JokeIdEmpty)
                .Which.MemberName.Should().Be("jokeId");
        }

        #endregion
    }
}