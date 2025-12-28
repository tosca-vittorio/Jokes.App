using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Test di unità per il Value Object <see cref="QuestionText"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che la factory <see cref="QuestionText.Create(string)"/> normalizzi l'input (Trim),
    /// - verificare invarianti: non nullo/non vuoto/non whitespace e lunghezza massima,
    /// - comprendere l'uso di xUnit e FluentAssertions per asserzioni e validazione di eccezioni.
    ///
    /// Nota DDD:
    /// - Il Value Object è immutabile e valido per costruzione: se esiste, rispetta le regole del dominio.
    /// - La normalizzazione (es. Trim) crea una "forma canonica" del testo.
    /// </summary>
    public class QuestionTextTests
    {
        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni un testo "trimmed" quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - Value venga normalizzato (rimozione spazi iniziali e finali),
        /// - IsEmpty sia false,
        /// - Length sia coerente con il valore normalizzato.
        /// </summary>
        [Fact]
        public void Create_ShouldReturnTrimmedValue_WhenInputIsValid()
        {
            // Arrange
            // const:
            // - rende esplicito che il valore non cambia ed è deterministico,
            // - utile nei test per chiarezza e per comunicare l’intento.
            const string raw = "  What do you call a fake noodle?  ";

            // Act
            // La factory Create(...) deve normalizzare (Trim) e validare l'input,
            // poi costruire il Value Object.
            var question = QuestionText.Create(raw);

            // Assert
            // Value deve essere la forma canonicalizzata (trim).
            question.Value.Should().Be("What do you call a fake noodle?");

            // IsEmpty:
            // - deve risultare false perché il testo è presente e significativo.
            question.IsEmpty.Should().BeFalse();

            // Length:
            // - deve riflettere la lunghezza del valore normalizzato, non dell’input grezzo.
            question.Length.Should().Be("What do you call a fake noodle?".Length);
        }

        #endregion

        #region Create - Validation

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando il valore è null, vuoto o composto solo da spazi.
        ///
        /// Nota xUnit:
        /// - [Theory] consente di testare la stessa regola con più input,
        /// - [InlineData] fornisce i casi limite.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrow_WhenValueIsNullOrWhitespace(string? value)
        {
            // Act
            // Lambda per catturare l'eccezione: FluentAssertions deve poter eseguire l'azione e intercettare il throw.
            var act = () => QuestionText.Create(value);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.QuestionNullOrEmpty)
                .Which.MemberName.Should().Be(nameof(QuestionText));
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando la lunghezza supera <see cref="QuestionText.MaxLength"/>.
        ///
        /// Nota C#:
        /// - <c>new('q', n)</c> crea una stringa di 'q' lunga n caratteri.
        /// - Usiamo MaxLength + 1 per generare un input sicuramente fuori limite.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenValueExceedsMaxLength()
        {
            // Arrange
            string tooLong = new('q', QuestionText.MaxLength + 1);

            // Act
            var act = () => QuestionText.Create(tooLong);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.QuestionTooLong)
                .Which.MemberName.Should().Be(nameof(QuestionText));
        }

        #endregion
    }
}