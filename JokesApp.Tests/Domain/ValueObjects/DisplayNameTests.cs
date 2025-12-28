using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit Test per il Value Object <see cref="DisplayName"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che la factory <see cref="DisplayName.Create(string)"/> normalizzi l'input (Trim),
    /// - verificare le invarianti: non nullo/non vuoto/non whitespace e lunghezza massima,
    /// - comprendere come testare Value Object in DDD con xUnit + FluentAssertions.
    ///
    /// Nota DDD:
    /// - Il Value Object è immutabile e valido per costruzione: se viene creato, rispetta le regole del dominio.
    /// - Le regole vivono tipicamente nella factory Create(...), che è il "guardiano" dell'invariante.
    /// </summary>
    public class DisplayNameTests
    {
        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni un DisplayName "trimmed" quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - Value venga normalizzato (rimozione spazi iniziali e finali),
        /// - Length sia coerente con il valore normalizzato,
        /// - IsEmpty sia false.
        /// </summary>
        [Fact]
        public void Create_ShouldReturnTrimmedDisplayName_WhenValueIsValid()
        {
            // Arrange
            // const:
            // - indica una costante (immutabile) deterministica, utile nei test per chiarezza e stabilità.
            const string rawValue = "  Ada Lovelace  ";

            // Act
            // La factory Create(...) normalizza e valida l'input, poi crea il Value Object.
            var displayName = DisplayName.Create(rawValue);

            // Assert
            // Value deve essere la forma canonicalizzata (trim).
            displayName.Value.Should().Be("Ada Lovelace");

            // Length deve riflettere la lunghezza del valore normalizzato (non dell'input grezzo).
            displayName.Length.Should().Be("Ada Lovelace".Length);

            // IsEmpty deve risultare false perché il valore è significativo.
            displayName.IsEmpty.Should().BeFalse();
        }

        #endregion

        #region Create - Validation

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando il valore è null, vuoto o composto solo da spazi.
        ///
        /// Nota xUnit:
        /// - [Theory] esegue lo stesso test con input diversi,
        /// - [InlineData] fornisce i casi limite (null, "", "   ").
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrow_WhenValueIsNullOrWhitespace(string? value)
        {
            // Act
            // Lambda: serve per catturare l'eccezione e farla verificare da FluentAssertions.
            var act = () => DisplayName.Create(value);

            // Assert
            // WithMessage(...) verifica il messaggio centralizzato.
            // Which.MemberName verifica che l'eccezione punti al "campo concettuale" del VO (qui: DisplayName).
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
                .Which.MemberName.Should().Be(nameof(DisplayName));
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando la lunghezza supera <see cref="DisplayName.MaxLength"/>.
        ///
        /// Nota C#:
        /// - <c>new('a', n)</c> crea una stringa di 'a' lunga n caratteri.
        /// - Usiamo MaxLength + 1 per generare un input sicuramente fuori limite.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenValueExceedsMaxLength()
        {
            // Arrange
            string tooLong = new('a', DisplayName.MaxLength + 1);

            // Act
            var act = () => DisplayName.Create(tooLong);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.DisplayNameMaxLength)
                .Which.MemberName.Should().Be(nameof(DisplayName));
        }

        #endregion
    }
}