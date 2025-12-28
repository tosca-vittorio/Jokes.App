using FluentAssertions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.ValueObjects
{
    /// <summary>
    /// Unit Test per il Value Object <see cref="EmailAddress"/>.
    ///
    /// Obiettivo didattico:
    /// - verificare che la factory <see cref="EmailAddress.Create(string)"/> normalizzi l'input (Trim),
    /// - verificare invarianti: non nullo/non vuoto/non whitespace, formato valido, lunghezza massima,
    /// - comprendere l'uso di xUnit ([Theory]/[InlineData]) e FluentAssertions per asserzioni e eccezioni.
    ///
    /// Nota DDD:
    /// - Un Value Object è immutabile e valido per costruzione: se viene creato, rispetta sempre le regole del dominio.
    /// - Le regole e la normalizzazione vivono nella factory Create(...).
    /// </summary>
    public class EmailAddressTests
    {
        #region Create - Happy path

        /// <summary>
        /// Verifica che Create ritorni una email normalizzata quando l'input è valido.
        ///
        /// Aspetti controllati:
        /// - Value sia l'input "canonicalizzato" (qui: Trim),
        /// - Length sia coerente con Value,
        /// - IsEmpty sia false.
        ///
        /// Nota xUnit:
        /// - [Theory] permette di testare più input validi con lo stesso metodo di test.
        /// </summary>
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("USER.NAME+tag@sub.domain.co.uk")]
        [InlineData("  user_name-123@example.io  ")]
        public void Create_ShouldReturnNormalizedEmail_WhenValueIsValid(string value)
        {
            // Act
            // Creiamo il Value Object: se l'input è valido, Create deve restituire un oggetto non vuoto e coerente.
            var email = EmailAddress.Create(value);

            // Assert
            // Value deve corrispondere alla forma normalizzata. Qui la normalizzazione verificata è il Trim().
            email.Value.Should().Be(value.Trim());

            // Length deve essere coerente con la stringa interna (Value).
            email.Length.Should().Be(email.Value.Length);

            // Se l'email è valida e presente, IsEmpty deve risultare false.
            email.IsEmpty.Should().BeFalse();
        }

        #endregion

        #region Create - Validation (null/whitespace)

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando l'email è null, vuota o composta solo da spazi.
        ///
        /// Nota:
        /// - La lambda <c>() => ...</c> serve a differire l'esecuzione per permettere a FluentAssertions
        ///   di catturare l'eccezione.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_ShouldThrow_WhenEmailIsNullOrWhitespace(string? value)
        {
            // Act
            var act = () => EmailAddress.Create(value);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailRequired)
                .Which.MemberName.Should().Be(nameof(EmailAddress));
        }

        #endregion

        #region Create - Validation (max length)

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/>
        /// quando l'email supera la lunghezza massima consentita.
        ///
        /// Nota didattica:
        /// - Generiamo una stringa volutamente molto lunga usando <c>new string('a', n)</c>.
        /// - L'obiettivo è violare l'invariante di lunghezza e ottenere l'errore previsto.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenEmailExceedsMaxLength()
        {
            // Arrange
            // Costruiamo un local-part lunghissimo e poi aggiungiamo "@example.com".
            // Questo è un modo semplice per superare certamente un limite di lunghezza complessiva.
            var localPart = new string('a', EmailAddress.MaxLength) + "@example.com";

            // Act
            var act = () => EmailAddress.Create(localPart);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailTooLong)
                .Which.MemberName.Should().Be(nameof(EmailAddress));
        }

        #endregion

        #region Create - Validation (invalid format)

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando il formato dell'email è invalido.
        ///
        /// Esempi coperti:
        /// - assenza di '@' (plainaddress),
        /// - dominio incompleto (missing@domain),
        /// - doppio punto nel dominio (user@domain..com),
        /// - dominio che inizia con '-' (user@-domain.com),
        /// - TLD troppo corto (user@domain.c),
        /// - caratteri non consentiti/inaspettati (màrio@example.com).
        ///
        /// Nota:
        /// - L'insieme di casi serve a evitare validazioni “troppo permissive”.
        /// </summary>
        [Theory]
        [InlineData("plainaddress")]
        [InlineData("missing@domain")]
        [InlineData("user@domain..com")]
        [InlineData("user@-domain.com")]
        [InlineData("user@domain.c")]
        [InlineData("màrio@example.com")]
        public void Create_ShouldThrow_WhenEmailIsInvalid(string value)
        {
            // Act
            var act = () => EmailAddress.Create(value);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailInvalid)
                .Which.MemberName.Should().Be(nameof(EmailAddress));
        }

        #endregion
    }
}