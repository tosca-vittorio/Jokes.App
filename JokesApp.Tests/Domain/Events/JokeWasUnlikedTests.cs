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
    /// Unit Test per il Domain Event <see cref="JokeWasUnliked"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che l'evento trasporti correttamente i dati (JokeId e likes dopo la modifica),
    /// - verificare la validazione dei parametri (likes non può essere negativo),
    /// - comprendere la sintassi Arrange/Act/Assert e l'uso di FluentAssertions per le eccezioni.
    ///
    /// Nota DDD:
    /// - Un Domain Event rappresenta un "fatto" del dominio.
    /// - Qui: “è stato rimosso un like”, e l'evento contiene il conteggio risultante dopo l'operazione.
    /// </summary>
    public class JokeWasUnlikedTests
	{
		#region Constructor - Happy path

		/// <summary>
		/// Verifica che il costruttore popoli correttamente le proprietà quando i valori sono validi.
		///
		/// Nota:
		/// - <c>LikesAfterChange</c> rappresenta lo stato finale dopo l'operazione (conteggio totale),
		///   non il delta (“-1”).
		/// </summary>
		[Fact]
		public void Constructor_ShouldPopulateProperties_WhenValuesAreValid()
		{
			// Arrange
			// JokeId.New() genera un ID valido per una Joke.
			var jokeId = JokeId.New();

			// Act
			// Creiamo l'evento con un conteggio finale pari a 0.
			var evt = new JokeWasUnliked(jokeId, 0);

			// Assert
			// Controlliamo che l'evento contenga esattamente i dati passati al costruttore.
			evt.JokeId.Should().Be(jokeId);
			evt.LikesAfterChange.Should().Be(0);
		}

		#endregion

		#region Constructor - Validation

		/// <summary>
		/// Verifica che il costruttore lanci <see cref="DomainValidationException"/>
		/// quando <c>likesAfterChange</c> è negativo.
		///
		/// Nota didattica:
		/// - Incapsuliamo la creazione dell'evento in una lambda <c>() => ...</c>
		///   per consentire a FluentAssertions di catturare l'eccezione.
		/// - Verifichiamo anche:
		///   - il messaggio (centralizzato in <see cref="JokeErrorMessages"/>),
		///   - il MemberName (parametro che ha causato l'errore).
		/// </summary>
		[Fact]
		public void Constructor_ShouldThrow_WhenLikesAreNegative()
		{
			// Act
			var act = () => new JokeWasUnliked(JokeId.New(), -1);

			// Assert
			act.Should()
				.Throw<DomainValidationException>()
				.WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached)
				.Which.MemberName.Should().Be("likesAfterChange");
		}

		#endregion
	}
}