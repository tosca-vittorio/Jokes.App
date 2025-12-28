using System;
using System.Reflection;
using FluentAssertions;
using JokesApp.Server.Domain.Entities;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.Entities
{
    /// <summary>
    /// Unit Test per l'Entity di dominio <see cref="ApplicationUser"/>.
    ///
    /// Obiettivo didattico:
    /// - mostrare come validare invarianti del dominio tramite eccezioni (qui: <see cref="DomainValidationException"/>),
    /// - verificare che il costruttore e i metodi di modifica (UpdateProfile / ChangeEmail / SetAvatar)
    ///   producano uno stato coerente,
    /// - capire la sintassi dei test con xUnit ([Fact]) e delle asserzioni fluenti con FluentAssertions (.Should()).
    ///
    /// Nota architetturale (DDD/Clean):
    /// - Questi test vivono nel progetto di test e dipendono dal Domain (ok).
    /// - Il Domain non deve dipendere dai test (clean direction of dependencies).
    /// </summary>
    public class ApplicationUserTests
    {
        #region Constructor

        /// <summary>
        /// Verifica che il costruttore:
        /// - assegni correttamente tutte le proprietà ricevute,
        /// - imposti <c>CreatedAt</c> in UTC,
        /// - lasci <c>UpdatedAt</c> nullo (perché non ci sono state modifiche post-creazione).
        /// </summary>
        [Fact]
        public void Constructor_ShouldSetProperties_WhenValuesAreValid()
        {
            // Arrange
            // In questa sezione prepariamo i dati in ingresso per il test.
            // Qui stiamo creando Value Object (UserId, DisplayName, EmailAddress, AvatarUrl) tramite factory Create(...).
            // Questo è tipico nel Domain Layer: i Value Object validano e incapsulano regole (es. formato email).
            var id = UserId.Create("user-1");
            var name = DisplayName.Create("Ada");
            var email = EmailAddress.Create("ada@example.com");
            var avatar = AvatarUrl.Create("https://example.com/avatar.png");

            // Act
            // Creiamo l'entity: qui stiamo testando il comportamento del costruttore, quindi l'azione è l'istanziazione.
            var user = new ApplicationUser(id, name, email, avatar);

            // Assert
            // FluentAssertions: l'approccio "fluent" usa .Should() per rendere la frase di test leggibile.
            // Esempio: user.Id.Should().Be(id) si legge come "Id dovrebbe essere uguale a id".
            user.Id.Should().Be(id);
            user.DisplayName.Should().Be(name);
            user.Email.Should().Be(email);
            user.AvatarUrl.Should().Be(avatar);

            // Qui verifichiamo anche un dettaglio temporale:
            // - CreatedAt.Kind == Utc: il dominio salva i timestamp in UTC per coerenza globale.
            user.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);

            // UpdatedAt nullo: non avendo ancora eseguito operazioni di aggiornamento,
            // l'entity deve risultare "mai modificata" dopo la creazione.
            user.UpdatedAt.Should().BeNull();
        }

        /// <summary>
        /// Verifica che il costruttore lanci una <see cref="DomainValidationException"/>
        /// quando l'ID è vuoto.
        ///
        /// Didattica:
        /// - uso di una lambda <c>() => ...</c> per catturare l'azione che deve lanciare eccezione,
        /// - catena FluentAssertions: Throw&lt;T&gt;() + WithMessage(...) + Which.MemberName.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenIdIsEmpty()
        {
            // Act
            // Invece di creare direttamente l'oggetto, incapsuliamo la creazione in una lambda.
            // Questo è necessario perché vogliamo "testare un'eccezione": la lambda ritarda l'esecuzione
            // e permette a FluentAssertions di intercettare l'eccezione.
            var act = () => new ApplicationUser(
                UserId.Empty,
                DisplayName.Create("A"),
                EmailAddress.Create("a@example.com"),
                AvatarUrl.Empty);

            // Assert
            // act.Should().Throw<DomainValidationException>():
            // - "act" è l'azione,
            // - Throw<T> afferma che quell'azione DEVE lanciare l'eccezione T.
            //
            // WithMessage(...) verifica anche il messaggio (qui centralizzato in ApplicationUserErrorMessages),
            // così mantieni consistenza e testi “parlanti”.
            //
            // Which.MemberName:
            // - FluentAssertions espone l'eccezione catturata tramite Which,
            // - e controlliamo MemberName per verificare che l'eccezione identifichi correttamente il parametro.
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
                .Which.MemberName.Should().Be("id");
        }

        /// <summary>
        /// Verifica che il costruttore lanci <see cref="DomainValidationException"/>
        /// quando il DisplayName è vuoto.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenDisplayNameIsEmpty()
        {
            // Act
            var act = () => new ApplicationUser(
                UserId.Create("user"),
                DisplayName.Empty,
                EmailAddress.Create("a@example.com"),
                AvatarUrl.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
                .Which.MemberName.Should().Be("displayName");
        }

        /// <summary>
        /// Verifica che il costruttore lanci <see cref="DomainValidationException"/>
        /// quando l'Email è vuota.
        /// </summary>
        [Fact]
        public void Constructor_ShouldThrow_WhenEmailIsEmpty()
        {
            // Act
            var act = () => new ApplicationUser(
                UserId.Create("user"),
                DisplayName.Create("A"),
                EmailAddress.Empty,
                AvatarUrl.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailRequired)
                .Which.MemberName.Should().Be("email");
        }

        #endregion

        #region UpdateProfile

        /// <summary>
        /// Verifica che UpdateProfile aggiorni i campi (nome/avatar/email) e valorizzi UpdatedAt.
        ///
        /// Nota:
        /// - In DDD questa è una tipica "operazione di dominio" che modifica lo stato in modo controllato,
        ///   applicando invarianti e regole di business.
        /// </summary>
        [Fact]
        public void UpdateProfile_ShouldUpdateFieldsAndTimestamp()
        {
            // Arrange
            // Creiamo un ApplicationUser valido (stato iniziale coerente).
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Prepariamo i nuovi valori da applicare.
            var newName = DisplayName.Create("Ada Lovelace");
            var newAvatar = AvatarUrl.Create("https://example.com/new.png");
            var newEmail = EmailAddress.Create("ada.lovelace@example.com");

            // Act
            // Eseguiamo la mutation controllata tramite metodo di dominio.
            // Qui stai testando che la "transizione di stato" avvenga correttamente.
            user.UpdateProfile(newName, newAvatar, newEmail);

            // Assert
            // Controlliamo che lo stato interno abbia effettivamente preso i nuovi valori.
            user.DisplayName.Should().Be(newName);
            user.AvatarUrl.Should().Be(newAvatar);
            user.Email.Should().Be(newEmail);

            // UpdatedAt deve essere impostato perché abbiamo effettuato una modifica.
            user.UpdatedAt.Should().NotBeNull();
        }

        /// <summary>
        /// Verifica che UpdateProfile lanci <see cref="DomainValidationException"/>
        /// quando viene passato un DisplayName vuoto.
        /// </summary>
        [Fact]
        public void UpdateProfile_ShouldThrow_WhenDisplayNameIsEmpty()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Act
            // Anche qui usiamo una lambda perché la chiamata deve lanciare eccezione.
            var act = () => user.UpdateProfile(DisplayName.Empty, AvatarUrl.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
                .Which.MemberName.Should().Be("displayName");
        }

        /// <summary>
        /// Verifica che UpdateProfile lanci <see cref="DomainValidationException"/>
        /// quando l'email viene "fornita" ma è vuota.
        ///
        /// Questo test è importante perché intercetta una regola:
        /// - se il chiamante decide di cambiare email, allora deve fornire un valore valido (non vuoto).
        /// </summary>
        [Fact]
        public void UpdateProfile_ShouldThrow_WhenEmailIsProvidedAndEmpty()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Act
            var act = () => user.UpdateProfile(
                DisplayName.Create("New"),
                AvatarUrl.Empty,
                EmailAddress.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailRequired)
                .Which.MemberName.Should().Be("email");
        }

        #endregion

        #region ChangeEmail

        /// <summary>
        /// Verifica che ChangeEmail aggiorni l'email e imposti UpdatedAt.
        ///
        /// Didattica:
        /// - Questo test dimostra un caso "single responsibility": un metodo focalizzato su una modifica specifica.
        /// </summary>
        [Fact]
        public void ChangeEmail_ShouldUpdateEmailAndTimestamp()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            var newEmail = EmailAddress.Create("ada@new.com");

            // Act
            user.ChangeEmail(newEmail);

            // Assert
            user.Email.Should().Be(newEmail);
            user.UpdatedAt.Should().NotBeNull();
        }

        /// <summary>
        /// Verifica che ChangeEmail lanci <see cref="DomainValidationException"/> se l'email è vuota.
        ///
        /// Nota sintattica:
        /// - MemberName qui è "newEmail" (cioè il nome del parametro del metodo ChangeEmail),
        ///   non "email" (che invece è la proprietà dell'entity).
        /// </summary>
        [Fact]
        public void ChangeEmail_ShouldThrow_WhenEmailIsEmpty()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Act
            var act = () => user.ChangeEmail(EmailAddress.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.EmailRequired)
                .Which.MemberName.Should().Be("newEmail");
        }

        #endregion

        #region SetAvatar

        /// <summary>
        /// Verifica che SetAvatar aggiorni l'avatar e imposti UpdatedAt.
        /// </summary>
        [Fact]
        public void SetAvatar_ShouldUpdateAvatarAndTimestamp()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            var newAvatar = AvatarUrl.Create("https://example.com/avatar.png");

            // Act
            user.SetAvatar(newAvatar);

            // Assert
            user.AvatarUrl.Should().Be(newAvatar);
            user.UpdatedAt.Should().NotBeNull();
        }

        #endregion

        #region Integrity

        /// <summary>
        /// Verifica che ValidateIntegrity intercetti uno stato internamente invalido.
        ///
        /// Concetto chiave:
        /// - Normalmente, l'API pubblica dell'entity dovrebbe impedire stati impossibili.
        /// - Qui "buciamo" intenzionalmente l'incapsulamento usando Reflection per simulare corruzione dello stato
        ///   (es. materializzazione da persistence malformata, bug, manipolazione esterna, ecc.).
        /// - ValidateIntegrity funge da rete di sicurezza: se lo stato è invalido, deve esplodere.
        /// </summary>
        [Fact]
        public void ValidateIntegrity_ShouldThrow_WhenStateIsInvalid()
        {
            // Arrange
            var user = new ApplicationUser(
                UserId.Create("user-1"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Force an invalid state
            // Spiegazione (IT):
            // - Settiamo la proprietà Id a UserId.Empty via reflection, creando uno stato che l'API pubblica
            //   probabilmente non consentirebbe.
            // - Questo ci permette di verificare che ValidateIntegrity effettui controlli reali e non sia "vuoto".
            SetPrivateProperty(user, nameof(ApplicationUser.Id), UserId.Empty);

            // Act
            var act = () => user.ValidateIntegrity();

            // Assert
            // Qui MemberName è nameof(ApplicationUser.Id), quindi è più robusto ai refactor del nome proprietà
            // rispetto a una stringa hardcoded.
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
                .Which.MemberName.Should().Be(nameof(ApplicationUser.Id));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper di test: imposta una proprietà (anche non pubblica) tramite Reflection.
        ///
        /// Perché esiste:
        /// - serve a forzare stati invalidi "artificiali" per testare le difese del dominio (es. ValidateIntegrity).
        ///
        /// Nota di sintassi:
        /// - <see cref="Type.GetProperty(string, BindingFlags)"/> può restituire null se la proprietà non esiste.
        /// - Qui usiamo l'operatore null-forgiving <c>!</c> per dire al compilatore: "so che non è null".
        ///   Questo è accettabile nei test quando la proprietà è nota e controllata dal team,
        ///   ma è comunque una scelta consapevole (se rinomini la proprietà, il test può esplodere a runtime).
        /// </summary>
        private static void SetPrivateProperty<T>(ApplicationUser target, string propertyName, T value)
        {
            // Recuperiamo la PropertyInfo anche se la proprietà è non pubblica:
            // - BindingFlags.Public | NonPublic: include getter/setter privati,
            // - BindingFlags.Instance: proprietà di istanza (non statiche).
            var property = typeof(ApplicationUser)
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            // SetValue assegna il valore anche se il setter non è pubblico (a seconda della runtime/access).
            // In pratica stiamo alterando lo stato dell'oggetto bypassando l'API pubblica: SOLO per test.
            property.SetValue(target, value);
        }

        #endregion
    }
}