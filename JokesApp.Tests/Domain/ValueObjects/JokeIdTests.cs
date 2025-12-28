using System;
using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit Test per il Value Object <see cref="JokeId"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che <see cref="JokeId.New"/> generi un identificatore valido (non vuoto),
    /// - verificare che <see cref="JokeId.Create(Guid)"/> crei correttamente l'ID a partire da un Guid valido,
    /// - verificare l'invariante principale: <see cref="Guid.Empty"/> non è ammesso come identificatore.
    ///
    /// Nota DDD:
    /// - Un ID come Value Object serve per tipizzare e proteggere il dominio:
    ///   invece di passare Guid ovunque, passiamo un tipo specifico (JokeId) con regole proprie.
    /// - "Valido per costruzione": se esiste un JokeId, allora non è vuoto.
    /// </summary>
    public class JokeIdTests
    {
        #region New

        /// <summary>
        /// Verifica che <see cref="JokeId.New"/> generi un ID non vuoto.
        ///
        /// Nota didattica:
        /// - <see cref="Guid.Empty"/> è il valore “zero” del Guid (tutti zeri),
        ///   tipicamente usato come sentinella per indicare "non inizializzato".
        /// - Qui vogliamo essere certi che New() non ritorni mai un valore sentinella.
        /// </summary>
        [Fact]
        public void New_ShouldCreateNonEmptyId()
        {
            // Act
            // New() dovrebbe generare internamente un Guid random (o equivalente) e incapsularlo nel Value Object.
            var id = JokeId.New();

            // Assert
            // NotBe(Guid.Empty):
            // - verifica che il Guid sia diverso dal valore sentinella “vuoto”.
            id.Value.Should().NotBe(Guid.Empty);

            // IsEmpty:
            // - proprietà di convenienza sul Value Object per esprimere lo stato “vuoto/non vuoto”.
            id.IsEmpty.Should().BeFalse();
        }

        #endregion

        #region Create - Happy path

        /// <summary>
        /// Verifica che <see cref="JokeId.Create(Guid)"/> ritorni un ID valido quando il Guid è valido.
        ///
        /// Nota didattica:
        /// - <see cref="Guid.NewGuid"/> genera un nuovo Guid pseudo-casuale.
        /// - In questo test ci interessa che Create(guid) preservi esattamente quel valore.
        /// </summary>
        [Fact]
        public void Create_ShouldReturnId_WhenGuidIsValid()
        {
            // Arrange
            // Generiamo un Guid valido (non vuoto).
            Guid guid = Guid.NewGuid();

            // Act
            // Create(...) costruisce il Value Object attorno al Guid fornito (dopo eventuali validazioni).
            var id = JokeId.Create(guid);

            // Assert
            // Il Value Object deve contenere lo stesso Guid passato in input.
            id.Value.Should().Be(guid);
            id.IsEmpty.Should().BeFalse();
        }

        #endregion

        #region Create - Validation

        /// <summary>
        /// Verifica che <see cref="JokeId.Create(Guid)"/> lanci <see cref="DomainValidationException"/>
        /// quando il Guid è <see cref="Guid.Empty"/>.
        ///
        /// Nota sintattica:
        /// - Usiamo una lambda <c>() => ...</c> per “ritardare” l'esecuzione e permettere a FluentAssertions
        ///   di catturare l'eccezione.
        /// - Verifichiamo sia il messaggio sia il MemberName per garantire diagnosi coerente.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenGuidIsEmpty()
        {
            // Act
            // Lambda necessaria perché l'eccezione deve essere intercettata dal framework di asserzione.
            var act = () => JokeId.Create(Guid.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.JokeIdEmpty)
                .Which.MemberName.Should().Be(nameof(JokeId));
        }

        #endregion
    }
}