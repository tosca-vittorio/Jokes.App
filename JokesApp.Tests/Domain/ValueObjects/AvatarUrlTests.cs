using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit Test per il Value Object <see cref="AvatarUrl"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare il comportamento "Empty Object": input null/whitespace produce <see cref="AvatarUrl.Empty"/>,
    /// - verificare le invarianti: lunghezza massima e validità dell'URL,
    /// - verificare la normalizzazione (Trim) in caso di input valido,
    /// - comprendere l'uso di xUnit [Theory]/[InlineData] e FluentAssertions.
    ///
    /// Nota DDD:
    /// - Questo Value Object sembra adottare una strategia "tollerante" per input assente:
    ///   invece di lanciare eccezione su null/whitespace, ritorna un oggetto speciale (Empty),
    ///   evitando di gestire null a valle.
    /// </summary>
    public class AvatarUrlTests
    {
        #region Create - Empty behavior

        /// <summary>
        /// Verifica che Create ritorni <see cref="AvatarUrl.Empty"/> quando il valore è null, vuoto o solo spazi.
        ///
        /// Nota didattica:
        /// - Qui NON ci aspettiamo un'eccezione: l'assenza dell'URL avatar è considerata lecita.
        /// - Con BeSameAs verifichiamo che sia proprio la stessa istanza (identità), tipico pattern "singleton Empty".
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldReturnEmpty_WhenValueIsNullOrWhitespace(string? value)
        {
            // Act
            // Factory "tollerante": normalizza l'input assente in AvatarUrl.Empty.
            var avatarUrl = AvatarUrl.Create(value);

            // Assert
            // BeSameAs:
            // - verifica che avatarUrl e AvatarUrl.Empty siano la stessa istanza in memoria (reference equality),
            // - utile se Empty è implementato come singleton per ridurre allocazioni e standardizzare confronti.
            avatarUrl.Should().BeSameAs(AvatarUrl.Empty);

            // IsEmpty:
            // - proprietà di convenienza che rende esplicito lo stato "assenza valore".
            avatarUrl.IsEmpty.Should().BeTrue();
        }

        #endregion

        #region Create - Validation

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando la lunghezza supera il massimo consentito.
        ///
        /// Nota:
        /// - Costruiamo una stringa volutamente troppo lunga per attivare l'invariante MaxLength.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenValueExceedsMaxLength()
        {
            // Arrange
            // Creiamo una URL con prefisso fisso + una coda di 'a' lunga AvatarUrl.MaxLength.
            // L'intento è superare il limite massimo ammesso dal Value Object.
            string tooLong = "https://example.com/" + new string('a', AvatarUrl.MaxLength);

            // Act
            var act = () => AvatarUrl.Create(tooLong);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.AvatarUrlMaxLength)
                .Which.MemberName.Should().Be(nameof(AvatarUrl));
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando l'URL non è valido.
        ///
        /// Esempi:
        /// - schema non consentito (ftp),
        /// - stringa che non è una URL.
        /// </summary>
        [Theory]
        [InlineData("ftp://example.com/avatar.png")]
        [InlineData("not-a-url")]
        public void Create_ShouldThrow_WhenUrlIsInvalid(string value)
        {
            // Act
            var act = () => AvatarUrl.Create(value);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.AvatarUrlInvalid)
                .Which.MemberName.Should().Be(nameof(AvatarUrl));
        }

        #endregion

        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni un AvatarUrl valido quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - normalizzazione (Trim) del valore,
        /// - IsEmpty deve risultare false.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/avatar.png")]
        [InlineData("  http://example.com/avatar.png  ")]
        public void Create_ShouldReturnUrl_WhenValueIsValid(string value)
        {
            // Act
            // Se l'input è valido, Create deve produrre un Value Object non vuoto con valore canonicalizzato.
            var avatarUrl = AvatarUrl.Create(value);

            // Assert
            // Value deve essere il valore normalizzato (trim).
            avatarUrl.Value.Should().Be(value.Trim());
            avatarUrl.IsEmpty.Should().BeFalse();
        }

        #endregion
    }
}