using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JokesApp.Server.Domain.Entities;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Events;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using Xunit;

namespace JokesApp.Tests.Domain.Entities
{
    /// <summary>
    /// Unit Test per l'Aggregate <see cref="Joke"/> (Domain Layer).
    ///
    /// Obiettivo didattico:
    /// - verificare le invarianti di dominio in creazione e modifica (DomainValidationException / DomainOperationException),
    /// - verificare autorizzazioni di modifica (UnauthorizedDomainOperationException),
    /// - verificare emissione e consumo dei Domain Events (es. JokeWasCreated, JokeWasUpdated, JokeWasLiked, JokeWasUnliked),
    /// - comprendere la sintassi test con xUnit e asserzioni fluenti con FluentAssertions.
    ///
    /// Nota architetturale (Clean + DDD):
    /// - I test possono dipendere dal Domain (ok).
    /// - Il Domain non deve conoscere il progetto di test (dipendenze sempre “verso l’interno”).
    /// </summary>
    public class JokeTests
    {
        #region Test Data (Fixtures)

        /// <summary>
        /// Test fixture: domanda valida (Value Object).
        /// Usiamo una proprietà statica per riutilizzare un valore coerente in più test
        /// evitando duplicazioni (DRY) senza complicare l'implementazione (KISS).
        /// </summary>
        private static QuestionText Question => QuestionText.Create("Why did the chicken cross the road?");

        /// <summary>
        /// Test fixture: risposta valida (Value Object).
        /// </summary>
        private static AnswerText Answer => AnswerText.Create("To get to the other side.");

        /// <summary>
        /// Test fixture: autore valido (UserId Value Object).
        /// </summary>
        private static UserId User => UserId.Create("user-1");

        #endregion

        #region Create

        /// <summary>
        /// Verifica che <see cref="Joke.Create(QuestionText, AnswerText, UserId)"/>:
        /// - inizializzi correttamente tutte le proprietà dell'Aggregate,
        /// - imposti CreatedAt in UTC, UpdatedAt nullo,
        /// - inizializzi Likes a 0,
        /// - emetta un Domain Event di tipo <see cref="JokeWasCreated"/>.
        ///
        /// Nota didattica:
        /// - I Domain Events servono per segnalare “qualcosa di significativo” accaduto nel dominio
        ///   senza accoppiare direttamente il dominio alle reazioni esterne (es. notifiche, proiezioni, ecc.).
        /// </summary>
        [Fact]
        public void Create_ShouldInitializePropertiesAndEmitEvent()
        {
            // Act
            // Qui l’azione è la factory di dominio: Joke.Create(...).
            // In DDD è comune usare una factory statica per garantire invarianti in creazione.
            var joke = Joke.Create(Question, Answer, User);

            // Assert
            // joke.Id.IsEmpty:
            // - JokeId è un Value Object, spesso con un concetto di "Empty" per rappresentare un ID non inizializzato.
            // - Qui vogliamo essere certi che Create generi un nuovo ID valido.
            joke.Id.IsEmpty.Should().BeFalse();

            // Verifica proprietà di dominio impostate correttamente.
            joke.Question.Should().Be(Question);
            joke.Answer.Should().Be(Answer);
            joke.ApplicationUserId.Should().Be(User);

            // Timestamp:
            // - CreatedAt in UTC per evitare ambiguità con fusi orari,
            // - UpdatedAt nullo perché non ci sono update successivi alla creazione.
            joke.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
            joke.UpdatedAt.Should().BeNull();

            // Likes: in creazione l’Aggregate parte da 0.
            joke.Likes.Should().Be(0);

            // Domain Events:
            // PullDomainEvents() è tipicamente un metodo che "estrae" e svuota la coda interna degli eventi.
            // Quindi:
            // - lo usiamo per leggere gli eventi emessi,
            // - e contemporaneamente per "pulire" lo stato eventi dell'aggregate.
            var events = joke.PullDomainEvents();
            events.Should().ContainSingle().Which.Should().BeOfType<JokeWasCreated>();
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando la domanda è null.
        ///
        /// Nota sintattica (xUnit):
        /// - [Theory] indica un test parametrico (stesso test, dati diversi),
        /// - [InlineData(null)] passa esplicitamente il valore null al parametro.
        ///
        /// Nota C#:
        /// - question è QuestionText? (nullable),
        /// - question! usa il null-forgiving operator: “so che potrebbe essere null, ma lo passo comunque”.
        ///   Serve qui perché vogliamo testare la branch che gestisce il null a runtime.
        /// </summary>
        [Theory]
        [InlineData(null)]
        public void Create_ShouldThrow_WhenQuestionIsNull(QuestionText? question)
        {
            // Act
            // Lambda per catturare l’eccezione: l’esecuzione è differita e FluentAssertions può intercettarla.
            var act = () => Joke.Create(question!, Answer, User);

            // Assert
            // - WithMessage(...) verifica il messaggio (centralizzato in JokeErrorMessages),
            // - Which.MemberName verifica che l’eccezione indichi correttamente il parametro (“question”).
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.QuestionNullOrEmpty)
                .Which.MemberName.Should().Be("question");
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando la risposta è null.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenAnswerIsNull()
        {
            // Act
            var act = () => Joke.Create(Question, null!, User);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.AnswerNullOrEmpty)
                .Which.MemberName.Should().Be("answer");
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando lo UserId è vuoto.
        ///
        /// Nota:
        /// - Qui il messaggio viene da ApplicationUserErrorMessages: è una scelta progettuale che riusa un messaggio comune
        ///   per la regola “UserId non può essere vuoto”.
        /// - MemberName atteso: “userId” (nome del parametro della factory Create).
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenUserIdIsEmpty()
        {
            // Act
            var act = () => Joke.Create(Question, Answer, UserId.Empty);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
                .Which.MemberName.Should().Be("userId");
        }

        /// <summary>
        /// Verifica che Create lanci <see cref="DomainValidationException"/> quando domanda e risposta sono equivalenti.
        ///
        /// Importanza di dominio:
        /// - questa è un'invariante di business: evita "jokes" degeneri in cui Q e A coincidono.
        /// </summary>
        [Fact]
        public void Create_ShouldThrow_WhenQuestionEqualsAnswer()
        {
            // Arrange
            var same = QuestionText.Create("Same");

            // Act
            var act = () => Joke.Create(same, AnswerText.Create("same"), User);

            // Assert
            // Qui MemberName è nameof(Joke.Question) (quindi legato alla proprietà del dominio).
            // Usare nameof(...) rende il test più robusto ai refactor rispetto a stringhe hardcoded.
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.QuestionAndAnswerCannotMatch)
                .Which.MemberName.Should().Be(nameof(Joke.Question));
        }

        #endregion

        #region Update

        /// <summary>
        /// Verifica che Update:
        /// - consenta all’autore di aggiornare domanda e risposta,
        /// - imposti UpdatedAt,
        /// - emetta un Domain Event di tipo <see cref="JokeWasUpdated"/>.
        ///
        /// Nota didattica:
        /// - joke.PullDomainEvents() viene chiamato subito dopo Create per “svuotare” l’evento di creazione,
        ///   così in questo test possiamo concentrarci SOLO sull’evento dell’update.
        /// </summary>
        [Fact]
        public void Update_ShouldChangeTextsAndEmitEvent_WhenRequestedByAuthor()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // clear creation event
            // In italiano: consumiamo l’evento di creazione per evitare che interferisca con gli assert dell’update.
            joke.PullDomainEvents();

            var newQuestion = QuestionText.Create("How do you organize a space party?");
            var newAnswer = AnswerText.Create("You planet.");

            // Act
            // Aggiornamento richiesto dallo stesso autore (User).
            joke.Update(User, newQuestion, newAnswer);

            // Assert
            joke.Question.Should().Be(newQuestion);
            joke.Answer.Should().Be(newAnswer);
            joke.UpdatedAt.Should().NotBeNull();

            // Verifica emissione evento update.
            var events = joke.PullDomainEvents();
            events.Should().ContainSingle().Which.Should().BeOfType<JokeWasUpdated>();
        }

        /// <summary>
        /// Verifica che Update sia vietato a un utente diverso dall’autore.
        ///
        /// In DDD questo è un controllo di autorizzazione a livello di dominio:
        /// - il dominio non deve permettere un’operazione “illegal” anche se chiamata dall’esterno.
        /// </summary>
        [Fact]
        public void Update_ShouldThrow_WhenUserIsNotAuthor()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Act
            var act = () => joke.Update(UserId.Create("other"), Question, Answer);

            // Assert
            act.Should()
                .Throw<UnauthorizedDomainOperationException>()
                .WithMessage(JokeErrorMessages.UpdateNotAllowed);
        }

        #endregion

        #region Likes

        /// <summary>
        /// Verifica che AddLike:
        /// - incrementi Likes,
        /// - emetta un Domain Event di tipo <see cref="JokeWasLiked"/>.
        ///
        /// Nota:
        /// - Puliamo prima gli eventi per non confondere l’evento di creazione con quello del like.
        /// </summary>
        [Fact]
        public void AddLike_ShouldIncrementLikesAndEmitEvent()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Pulizia eventi pregressi (es. JokeWasCreated).
            joke.PullDomainEvents();

            // Act
            joke.AddLike();

            // Assert
            joke.Likes.Should().Be(1);

            // Single():
            // - richiede esattamente un elemento nella sequenza,
            // - in un test è utile perché se ci sono 0 o >1 eventi, fallisce evidenziando un problema.
            joke.PullDomainEvents().Single().Should().BeOfType<JokeWasLiked>();
        }

        /// <summary>
        /// Verifica che AddLike lanci <see cref="DomainOperationException"/> quando il numero massimo di like è raggiunto.
        ///
        /// Nota didattica:
        /// - Qui forziamo un valore “impossibile” o estremo (int.MaxValue) tramite Reflection
        ///   per testare la protezione contro overflow o superamento del limite.
        /// </summary>
        [Fact]
        public void AddLike_ShouldThrow_WhenMaximumReached()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Impostiamo Likes = int.MaxValue bypassando l'API pubblica.
            // Questo serve a verificare che AddLike non vada oltre e lanci l’eccezione prevista.
            SetPrivateProperty(joke, nameof(Joke.Likes), int.MaxValue);

            // Act
            var act = () => joke.AddLike();

            // Assert
            act.Should()
                .Throw<DomainOperationException>()
                .WithMessage(JokeErrorMessages.MaximumLikeOfJokeReached);
        }

        /// <summary>
        /// Verifica che RemoveLike:
        /// - decrementi Likes,
        /// - emetta un Domain Event di tipo <see cref="JokeWasUnliked"/>.
        ///
        /// Nota:
        /// - Per poter rimuovere un like, prima aggiungiamo un like (stato coerente).
        /// - Poi puliamo gli eventi per isolare l’evento del remove.
        /// </summary>
        [Fact]
        public void RemoveLike_ShouldDecrementLikesAndEmitEvent()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);
            joke.PullDomainEvents();

            // Portiamo l’aggregate in uno stato “Like=1” in modo controllato.
            joke.AddLike();
            joke.PullDomainEvents();

            // Act
            joke.RemoveLike();

            // Assert
            joke.Likes.Should().Be(0);
            joke.PullDomainEvents().Single().Should().BeOfType<JokeWasUnliked>();
        }

        /// <summary>
        /// Verifica che RemoveLike lanci <see cref="DomainOperationException"/> quando Likes è già 0.
        ///
        /// Questa è una regola di dominio “operazionale”:
        /// - non puoi decrementare sotto zero (limite minimo).
        /// </summary>
        [Fact]
        public void RemoveLike_ShouldThrow_WhenLikesAreZero()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Act
            var act = () => joke.RemoveLike();

            // Assert
            act.Should()
                .Throw<DomainOperationException>()
                .WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached);
        }

        #endregion

        #region Author association

        /// <summary>
        /// Verifica che SetAuthor associ correttamente l’autore quando gli ID coincidono.
        ///
        /// Nota:
        /// - Joke contiene ApplicationUserId (ID dell'autore),
        /// - SetAuthor “aggancia” l’oggetto ApplicationUser (navigational reference) solo se coerente.
        /// </summary>
        [Fact]
        public void SetAuthor_ShouldAssociateAuthor_WhenIdsMatch()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Creiamo un ApplicationUser con lo stesso UserId della Joke.
            var author = new ApplicationUser(
                User,
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Act
            joke.SetAuthor(author);

            // Assert
            joke.Author.Should().Be(author);
        }

        /// <summary>
        /// Verifica che SetAuthor lanci <see cref="DomainValidationException"/> se l'autore è null.
        /// </summary>
        [Fact]
        public void SetAuthor_ShouldThrow_WhenAuthorIsNull()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            // Act
            var act = () => joke.SetAuthor(null!);

            // Assert
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.AuthorNull)
                .Which.MemberName.Should().Be("author");
        }

        /// <summary>
        /// Verifica che SetAuthor lanci <see cref="DomainOperationException"/> se l’autore è già stato impostato.
        ///
        /// Questa è una regola di dominio sul “lifecycle” dell’aggregate:
        /// - una volta assegnato l’autore, non è lecito riassegnarlo (o almeno non con questa API).
        /// </summary>
        [Fact]
        public void SetAuthor_ShouldThrow_WhenAuthorAlreadySet()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);
            var author = new ApplicationUser(
                User,
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            joke.SetAuthor(author);

            // Act
            var act = () => joke.SetAuthor(author);

            // Assert
            act.Should()
                .Throw<DomainOperationException>()
                .WithMessage(JokeErrorMessages.AuthorAlreadySet);
        }

        /// <summary>
        /// Verifica che SetAuthor lanci <see cref="DomainValidationException"/> se l’ID dell’autore non coincide con ApplicationUserId.
        ///
        /// Nota:
        /// - Questo impedisce inconsistenze: Joke.ApplicationUserId deve corrispondere a author.Id.
        /// </summary>
        [Fact]
        public void SetAuthor_ShouldThrow_WhenAuthorIdDoesNotMatch()
        {
            // Arrange
            var joke = Joke.Create(Question, Answer, User);

            var author = new ApplicationUser(
                UserId.Create("other"),
                DisplayName.Create("Ada"),
                EmailAddress.Create("ada@example.com"),
                AvatarUrl.Empty);

            // Act
            var act = () => joke.SetAuthor(author);

            // Assert
            // Qui MemberName atteso è "Id" (coerente con l’implementazione dell’eccezione nel dominio).
            act.Should()
                .Throw<DomainValidationException>()
                .WithMessage(JokeErrorMessages.AuthorIdMismatch)
                .Which.MemberName.Should().Be("Id");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Helper di test per impostare una proprietà tramite Reflection.
        ///
        /// Perché è utile:
        /// - in produzione l’Aggregate dovrebbe proteggere il proprio stato (incapsulamento),
        /// - nei test può essere necessario forzare stati estremi/impossibili per verificare difese (es. limiti di likes).
        ///
        /// Nota tecnica:
        /// - usiamo BindingFlags per includere proprietà non pubbliche,
        /// - l’operatore <c>!</c> (null-forgiving) dice al compilatore che assumiamo la proprietà esista.
        ///   È accettabile nei test quando il nome della proprietà è controllato dal codice stesso.
        /// </summary>
        private static void SetPrivateProperty<T>(Joke target, string propertyName, T value)
        {
            var property = typeof(Joke)
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            property.SetValue(target, value);
        }

        #endregion
    }
}