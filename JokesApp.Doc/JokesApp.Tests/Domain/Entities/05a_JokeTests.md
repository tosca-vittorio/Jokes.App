# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Entities/05a_JokeTests — Manuale didattico completo dei test per `Joke`

> **File di test:** `JokesApp.Tests/Domain/Entities/JokeTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.Entities.JokeTests`  
> **Soggetto:** Aggregate `Joke` (Domain Layer)

---

## 1️⃣ Obiettivo della suite

Questa documentazione è **manualistica e accademica**: descrive in modo esplicito *cosa* verificano i test e *perché* quelle verifiche sono essenziali per garantire le invarianti del dominio. L’obiettivo è rendere la suite comprensibile anche a chi studia la codebase per la prima volta, mantenendo un rigore coerente con Clean Architecture e DDD.

**Macro-obiettivi della suite `JokeTests`:**
- garantire **invarianti di creazione** (domanda/risposta valide, non degeneri, autore valido);
- garantire **autorizzazioni di aggiornamento** (solo l’autore può modificare);
- garantire **limiti del contatore Like** (minimo 0, massimo `int.MaxValue`);
- garantire **coerenza dell’autore** (`ApplicationUserId` deve combaciare con l’utente associato);
- garantire **emissione e consumo deterministico dei Domain Events**.

---

## 2️⃣ Strumenti e sintassi di base usati in tutti i test

### ✅ xUnit
- **`[Fact]`**: dichiara un test non parametrico, con scenario fisso.
- **`[Theory]` + `[InlineData]`**: dichiara un test parametrico, utile per validare più input mantenendo lo stesso comportamento atteso.

### ✅ FluentAssertions
- **`obj.Should()`**: abilita lo stile *fluent* delle asserzioni.
- **`.Be(...)` / `.BeFalse()` / `.BeNull()` / `.NotBeNull()`**: asserzioni dirette su valori e stato.
- **`.Throw<TException>()`**: verifica che l’azione generi un’eccezione del tipo atteso.
- **`.WithMessage(...)`**: vincola il test al messaggio previsto (utile quando i messaggi sono centralizzati nel dominio).
- **`.Which.MemberName`**: consente di verificare che l’errore sia associato al membro/parametro atteso.

### ✅ Domain Events
- **`PullDomainEvents()`**:
  - restituisce la lista degli eventi di dominio accodati dall’aggregate;
  - **svuota** la coda interna (side-effect intenzionale e utile nei test per isolare eventi tra operazioni successive).

---

## 3️⃣ Documentazione test-per-test (con sintassi punto per punto)

> Nota: gli snippet seguenti mostrano la struttura tipica *Arrange/Act/Assert* e i pattern ripetuti nella suite.

### 🧪 Test: `Create_ShouldInitializePropertiesAndEmitEvent`

**Scopo:** verificare che `Joke.Create(...)` inizializzi lo stato dell’aggregate e produca l’evento di creazione.

**Sintassi (estratto):**
```csharp
var joke = Joke.Create(Question, Answer, User);

joke.Id.IsEmpty.Should().BeFalse();
joke.Question.Should().Be(Question);
joke.Answer.Should().Be(Answer);
joke.ApplicationUserId.Should().Be(User);

joke.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
joke.UpdatedAt.Should().BeNull();

joke.Likes.Should().Be(0);

var events = joke.PullDomainEvents();
events.Should().ContainSingle().Which.Should().BeOfType<JokeWasCreated>();
```

**Spiegazione essenziale:**

* `Joke.Create(...)`: factory di dominio → crea l’aggregate applicando invarianti e validazioni.
* `Id.IsEmpty.Should().BeFalse()`: l’ID non deve rimanere “vuoto” (sentinella).
* `CreatedAt.Kind == Utc`: garantisce coerenza temporale (UTC) per confronti affidabili.
* `UpdatedAt == null`: subito dopo la creazione non esistono aggiornamenti.
* `Likes == 0`: valore iniziale deterministico.
* `PullDomainEvents()`: consente di verificare che l’aggregate abbia emesso **esattamente** un evento coerente con la creazione.

**Aspettativa di dominio:** la creazione produce uno stato coerente + 1 evento `JokeWasCreated`.

---

### 🧪 Test: `Create_ShouldThrow_WhenQuestionIsNull`

**Scopo:** rifiutare una domanda nulla con eccezione di validazione coerente.

**Sintassi (estratto):**

```csharp
var act = () => Joke.Create(question!, Answer, User);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.QuestionNullOrEmpty)
   .Which.MemberName.Should().Be("question");
```

**Spiegazione sintattica:**

* `question!`: *null-forgiving operator* → permette di passare intenzionalmente `null` evitando warning del compilatore.
* `var act = () => ...`: lambda che ritarda l’esecuzione; necessaria per testare eccezioni.
* `Throw<DomainValidationException>()`: vincola il tipo di eccezione al dominio.
* `WithMessage(...)`: vincola il messaggio a quello centralizzato.
* `Which.MemberName`: vincola il parametro/membro che ha generato l’errore.

---

### 🧪 Test: `Create_ShouldThrow_WhenAnswerIsNull`

**Scopo:** rifiutare una risposta nulla con lo stesso pattern di validazione.

**Sintassi (estratto):**

```csharp
var act = () => Joke.Create(Question, null!, User);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.AnswerNullOrEmpty)
   .Which.MemberName.Should().Be("answer");
```

**Nota:** è un caso speculare del test precedente, applicato al parametro `answer`.

---

### 🧪 Test: `Create_ShouldThrow_WhenUserIdIsEmpty`

**Scopo:** impedire la creazione con autore vuoto/non inizializzato.

**Sintassi (estratto):**

```csharp
var act = () => Joke.Create(Question, Answer, UserId.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
   .Which.MemberName.Should().Be("userId");
```

**Spiegazione:** `UserId.Empty` simula un VO in stato sentinella → deve essere rifiutato.

---

### 🧪 Test: `Create_ShouldThrow_WhenQuestionEqualsAnswer`

**Scopo:** evitare una barzelletta degenerata con domanda e risposta equivalenti.

**Sintassi (estratto):**

```csharp
var same = QuestionText.Create("Same");
var act = () => Joke.Create(same, AnswerText.Create("same"), User);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.QuestionAndAnswerCannotMatch)
   .Which.MemberName.Should().Be("Question/Answer");
```

**Spiegazione:** il vincolo mira a bloccare casi “banali” (in genere gestiti come confronto case-insensitive a livello di dominio).

---

### 🧪 Test: `Update_ShouldChangeTextsAndEmitEvent_WhenRequestedByAuthor`

**Scopo:** l’autore può aggiornare i contenuti, valorizzare `UpdatedAt` ed emettere l’evento di aggiornamento.

**Sintassi (estratto):**

```csharp
var joke = Joke.Create(Question, Answer, User);
joke.PullDomainEvents();

var newQuestion = QuestionText.Create("How do you organize a space party?");
var newAnswer = AnswerText.Create("You planet.");

joke.Update(User, newQuestion, newAnswer);

joke.Question.Should().Be(newQuestion);
joke.Answer.Should().Be(newAnswer);
joke.UpdatedAt.Should().NotBeNull();

var events = joke.PullDomainEvents();
events.Should().ContainSingle().Which.Should().BeOfType<JokeWasUpdated>();
```

**Spiegazione:**

* `PullDomainEvents()` prima dell’update: svuota l’evento di creazione, isolando l’evento di update.
* `Update(User, ...)`: autorizzazione basata sull’identità dell’autore (il chiamante deve essere `User`).
* `UpdatedAt`: deve essere valorizzato solo a seguito di un aggiornamento.
* Eventi: dopo l’update ci si aspetta **solo** `JokeWasUpdated`.

---

### 🧪 Test: `Update_ShouldThrow_WhenUserIsNotAuthor`

**Scopo:** impedire l’update se il chiamante non è l’autore.

**Sintassi (estratto):**

```csharp
var act = () => joke.Update(UserId.Create("other"), Question, Answer);

act.Should()
   .Throw<UnauthorizedDomainOperationException>()
   .WithMessage(JokeErrorMessages.UpdateNotAllowed);
```

**Spiegazione:** l’eccezione di autorizzazione è una *domain operation exception* specializzata: blocca modifiche non autorizzate senza coinvolgere infrastruttura o database.

---

### 🧪 Test: `AddLike_ShouldIncrementLikesAndEmitEvent`

**Scopo:** un like incrementa il contatore e genera evento.

**Sintassi (estratto):**

```csharp
joke.PullDomainEvents();

joke.AddLike();

joke.Likes.Should().Be(1);
joke.PullDomainEvents().Single().Should().BeOfType<JokeWasLiked>();
```

**Spiegazione:**

* `Single()` garantisce che l’operazione produca **esattamente un** evento.
* Il contatore Like deve avanzare in modo deterministico.

---

### 🧪 Test: `AddLike_ShouldThrow_WhenMaximumReached`

**Scopo:** impedire overflow quando `Likes == int.MaxValue`.

**Sintassi (estratto):**

```csharp
SetPrivateProperty(joke, nameof(Joke.Likes), int.MaxValue);

var act = () => joke.AddLike();

act.Should()
   .Throw<DomainOperationException>()
   .WithMessage(JokeErrorMessages.MaximumLikeOfJokeReached);
```

**Spiegazione:** `SetPrivateProperty` (reflection) forza uno stato limite non raggiungibile via API pubblica; il dominio deve comunque proteggersi.

---

### 🧪 Test: `RemoveLike_ShouldDecrementLikesAndEmitEvent`

**Scopo:** un unlike decrementa il contatore e genera evento.

**Sintassi (estratto):**

```csharp
joke.AddLike();
joke.PullDomainEvents();

joke.RemoveLike();

joke.Likes.Should().Be(0);
joke.PullDomainEvents().Single().Should().BeOfType<JokeWasUnliked>();
```

**Spiegazione:** si porta lo stato a `Likes = 1` per poter verificare la rimozione.

---

### 🧪 Test: `RemoveLike_ShouldThrow_WhenLikesAreZero`

**Scopo:** evitare che il contatore Like diventi negativo.

**Sintassi (estratto):**

```csharp
var act = () => joke.RemoveLike();

act.Should()
   .Throw<DomainOperationException>()
   .WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached);
```

---

### 🧪 Test: `SetAuthor_ShouldAssociateAuthor_WhenIdsMatch`

**Scopo:** associare correttamente l’autore quando gli ID combaciano.

**Sintassi (estratto):**

```csharp
var author = new ApplicationUser(
    User,
    DisplayName.Create("Ada"),
    EmailAddress.Create("ada@example.com"),
    AvatarUrl.Empty);

joke.SetAuthor(author);

joke.Author.Should().Be(author);
```

**Spiegazione:** `SetAuthor` è un comportamento di dominio che valorizza la reference `Author` solo se coerente con `ApplicationUserId`.

---

### 🧪 Test: `SetAuthor_ShouldThrow_WhenAuthorIsNull`

**Scopo:** bloccare input nullo con errore di validazione.

**Sintassi (estratto):**

```csharp
var act = () => joke.SetAuthor(null!);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.AuthorNull)
   .Which.MemberName.Should().Be("author");
```

---

### 🧪 Test: `SetAuthor_ShouldThrow_WhenAuthorAlreadySet`

**Scopo:** impedire riassegnazione dell’autore (immutabilità logica della relazione).

**Sintassi (estratto):**

```csharp
joke.SetAuthor(author);

var act = () => joke.SetAuthor(author);

act.Should()
   .Throw<DomainOperationException>()
   .WithMessage(JokeErrorMessages.AuthorAlreadySet);
```

---

### 🧪 Test: `SetAuthor_ShouldThrow_WhenAuthorIdDoesNotMatch`

**Scopo:** impedire associazioni incoerenti tra `Author.Id` e `ApplicationUserId`.

**Sintassi (estratto):**

```csharp
var author = new ApplicationUser(
    UserId.Create("other"),
    DisplayName.Create("Ada"),
    EmailAddress.Create("ada@example.com"),
    AvatarUrl.Empty);

var act = () => joke.SetAuthor(author);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.AuthorIdMismatch)
   .Which.MemberName.Should().Be("Id");
```

---

## 4️⃣ Sezione di sintassi avanzata: helper reflection

La suite utilizza un helper per forzare stati limite (solo a fini di test):

```csharp
private static void SetPrivateProperty<T>(Joke target, string propertyName, T value)
{
    var property = typeof(Joke)
        .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

    property.SetValue(target, value);
}
```

**Spiegazione:**

* `BindingFlags.NonPublic`: consente l’accesso a proprietà non pubbliche (encapsulation preservata nel codice di produzione).
* `!`: evita warning nullability assumendo che la proprietà esista (assunzione corretta se `nameof(Joke.Likes)` è valido).
* `SetValue(...)`: imposta un valore non ottenibile via API pubblica, utile per testare “barriere” del dominio (overflow/underflow).

---

## 5️⃣ Conclusione

Questa suite rappresenta un riferimento completo e deterministico per:

* invarianti di dominio dell’aggregate `Joke`,
* autorizzazioni e operazioni consentite,
* gestione dei Domain Events,
* protezione da stati impossibili (overflow/underflow),
* coerenza della relazione con l’autore.

Ogni test è documentato con **sintassi esplicita** e **aspettative motivate**, rendendo studio, manutenzione e revisione del dominio immediati e solidi.

---