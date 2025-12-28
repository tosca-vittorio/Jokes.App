using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Test di unità per il Value Object <see cref="UserId"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che la factory <see cref="UserId.Create(string)"/> normalizzi l'input (Trim),
    /// - verificare invarianti: non nullo/non vuoto/non whitespace e lunghezza massima,
    /// - comprendere l'uso di xUnit ([Fact]/[Theory]) e FluentAssertions per asserzioni ed eccezioni.
    ///
    /// Nota DDD:
    /// - Un Value Object è immutabile e valido per costruzione: se viene creato, rispetta le regole del dominio.
    /// - Usare un tipo dedicato (UserId) evita di passare stringhe “anonime” nel dominio.
    /// </summary>
    public class UserIdTests
    {
        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni un UserId con valore "trimmed" quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - Value venga normalizzato (rimozione spazi iniziali e finali),
        /// - IsEmpty sia false.
        /// </summary>
        [Fact]
        public void Create_ShouldReturnTrimmedValue_WhenInputIsValid()
        {
            // Arrange
            // const:
            // - indica una costante (immutabile) deterministica, utile per test ripetibili e leggibili.
            const string raw = "  user-123  ";

            // Act
            // Create(...) applica normalizzazione e validazione, poi crea il Value Object.
            var userId = UserId.Create(raw);

            // Assert
            // Verifichiamo che il valore interno sia canonicalizzato (Trim).
            userId.Value.Should().Be("user-123");

            // Se l'ID è valido e presente, non deve essere considerato "Empty".
            userId.IsEmpty.Should().BeFalse();
        }

        #endregion

        #region Create - Validation (null/whitespace)

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando il valore è null, vuoto o composto solo da spazi.
        ///
        /// Nota xUnit:
        /// - [Theory] consente di eseguire lo stesso test con input diversi,
        /// - [InlineData] fornisce i casi limite.
        ///
        /// Nota sintattica:
        /// - Usiamo una lambda <c>() => ...</c> per catturare l'eccezione con FluentAssertions.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrow_WhenValueIsNullOrWhitespace(string? value)
        {
            // Act
            var act = () => UserId.Create(value);

            // Assert
            // WithMessage(...) verifica il messaggio di errore centralizzato.
            // Which.MemberName verifica il "campo concettuale" associato alla validazione (qui: UserId).
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
                .Which.MemberName.Should().Be(nameof(UserId));
        }

        #endregion

        #region Create - Validation (max length)

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando la lunghezza supera <see cref="UserId.MaxLength"/>.
        ///
        /// Nota C#:
        /// - <c>new('u', n)</c> crea una stringa di 'u' lunga n caratteri,
        /// - usiamo MaxLength + 1 per garantire che l'input sia fuori limite.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenValueExceedsMaxLength()
        {
            // Arrange
            string tooLong = new('u', UserId.MaxLength + 1);

            // Act
            var act = () => UserId.Create(tooLong);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.UserIdTooLong)
                .Which.MemberName.Should().Be(nameof(UserId));
        }

        #endregion
    }
}