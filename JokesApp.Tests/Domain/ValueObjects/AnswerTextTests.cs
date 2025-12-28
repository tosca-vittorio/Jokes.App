using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit Test per il Value Object <see cref="AnswerText"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che la factory <see cref="AnswerText.Create(string)"/> normalizzi l'input (trim),
    /// - verificare le invarianti: non nullo/non vuoto/non whitespace, lunghezza massima,
    /// - comprendere come testare Value Object in DDD con xUnit + FluentAssertions.
    ///
    /// Nota DDD:
    /// - Un Value Object è immutabile e valido per costruzione: se esiste, rispetta le regole del dominio.
    /// - La factory Create(...) è il punto in cui si applicano normalizzazione e validazione.
    /// </summary>
    public class AnswerTextTests
    {
        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni un Value Object con valore "trimmed" quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - il testo venga normalizzato (rimozione spazi iniziali/finali),
        /// - il Value Object non risulti vuoto,
        /// - la lunghezza esposta dal VO sia coerente con il valore normalizzato.
        /// </summary>
        [Fact]
        public void Create_ShouldReturnTrimmedValue_WhenInputIsValid()
        {
            // Arrange
            // const:
            // - rende esplicito che la stringa non cambia e il compilatore la tratta come costante.
            // - utile nei test per comunicare intenti e mantenere i dati deterministici.
            const string raw = "  An impasta!  ";

            // Act
            // Creiamo il Value Object tramite factory: qui deve avvenire la normalizzazione (Trim).
            var answer = AnswerText.Create(raw);

            // Assert
            // Verifichiamo il valore normalizzato (spazi rimossi ai bordi).
            answer.Value.Should().Be("An impasta!");

            // Invarianti “derivate”:
            // - IsEmpty deve essere false perché il testo è significativo,
            // - Length deve riflettere la lunghezza del valore normalizzato, non dell'input grezzo.
            answer.IsEmpty.Should().BeFalse();
            answer.Length.Should().Be("An impasta!".Length);
        }

        #endregion

        #region Create - Validation

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando il valore è null, vuoto o solo spazi.
        ///
        /// Nota sintattica (xUnit):
        /// - [Theory] permette di eseguire lo stesso test con input diversi,
        /// - [InlineData(...)] fornisce i casi (null, stringa vuota, whitespace).
        ///
        /// Nota didattica:
        /// - in questi casi il Value Object non può essere creato perché violerebbe l'invariante "Answer non può essere vuota".
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrow_WhenValueIsNullOrWhitespace(string? value)
        {
            // Act
            // Lambda per catturare l'eccezione: FluentAssertions deve poter invocare l'azione e intercettare il throw.
            var act = () => AnswerText.Create(value);

            // Assert
            // Con Which.MemberName verifichiamo che l'eccezione punti al "campo concettuale" del Value Object.
            // Qui usi nameof(AnswerText), quindi il test resta robusto ai refactor del nome tipo.
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.AnswerNullOrEmpty)
                .Which.MemberName.Should().Be(nameof(AnswerText));
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando la lunghezza supera il massimo consentito.
        ///
        /// Nota C#:
        /// - <c>new('a', n)</c> crea una stringa composta dal carattere 'a' ripetuto n volte.
        /// - Qui usiamo MaxLength + 1 per generare un input sicuramente fuori limite.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenValueExceedsMaxLength()
        {
            // Arrange
            // Costruiamo un testo di lunghezza MaxLength + 1 per violare l'invariante di dominio.
            string tooLong = new('a', AnswerText.MaxLength + 1);

            // Act
            var act = () => AnswerText.Create(tooLong);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.AnswerTooLong)
                .Which.MemberName.Should().Be(nameof(AnswerText));
        }

        #endregion
    }
}