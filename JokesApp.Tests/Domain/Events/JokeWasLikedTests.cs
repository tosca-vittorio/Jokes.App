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
    /// Unit Test per il Domain Event <see cref="JokeWasLiked"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che l'evento trasporti correttamente i dati (JokeId e likes dopo la modifica),
    /// - verificare la validazione dei parametri (likes non può essere negativo),
    /// - comprendere la sintassi Arrange/Act/Assert e l'uso di FluentAssertions per testare eccezioni.
    ///
    /// Nota DDD:
    /// - Un Domain Event rappresenta un "fatto" accaduto nel dominio.
    /// - In questo caso: “è stato aggiunto un like”, e l'evento include il conteggio risultante.
    /// </summary>
    public class JokeWasLikedTests
    {
        #region Constructor - Happy path

        /// <summary>
        /// Verifica che il costruttore popoli correttamente le proprietà quando i valori sono validi.
        ///
        /// Nota:
        /// - <c>LikesAfterChange</c> rappresenta lo stato finale (conteggio like) dopo l'operazione,
        ///   non "il delta" (non è “+1”), ma il valore risultante.
        /// </summary>
        [Fact]
        public void Constructor_ShouldPopulateProperties_WhenValuesAreValid()
        {
            // Arrange
            // JokeId.New() genera un ID valido per una Joke.
            var jokeId = JokeId.New();

            // Act
            // Creiamo l'evento con un conteggio likes "dopo il cambiamento" pari a 3.
            var evt = new JokeWasLiked(jokeId, 3);

            // Assert
            // Controlliamo che l'evento contenga esattamente i dati passati al costruttore.
            evt.JokeId.Should().Be(jokeId);
            evt.LikesAfterChange.Should().Be(3);
        }

        #endregion

        #region Constructor - Validation

        /// <summary>
        /// Verifica che il costruttore lanci <see cref="DomainValidationException"/>
        /// quando <c>likesAfterChange</c> è negativo.
        ///
        /// Nota didattica:
        /// - Incapsuliamo la creazione dell'evento in una lambda <c>() => ...</c>
        ///   per permettere a FluentAssertions di catturare l'eccezione.
        /// - Verifichiamo anche:
        ///   - il messaggio (centralizzato in <see cref="JokeErrorMessages"/>),
        ///   - il MemberName (parametro che ha causato l'errore).
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenLikesAreNegative()
        {
            // Act
            var act = () => new JokeWasLiked(JokeId.New(), -1);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached)
                .Which.MemberName.Should().Be("likesAfterChange");
        }

        #endregion
    }
}