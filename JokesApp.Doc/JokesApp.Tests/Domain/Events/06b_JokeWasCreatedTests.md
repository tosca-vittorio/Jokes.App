# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Events/06b_JokeWasCreatedTests.md — Manuale didattico per `JokeWasCreated`

> **File di test:** `JokesApp.Tests/Domain/Events/JokeWasCreatedTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.Events.JokeWasCreatedTests`  
> **Soggetto:** Domain Event `JokeWasCreated` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Domain Event `JokeWasCreated` come “fatto” del dominio:

- **completezza dei dati**: l’evento deve riportare correttamente identificativo, autore, domanda, risposta e timestamp;
- **coerenza interna**: le proprietà esposte devono corrispondere ai parametri passati al costruttore;
- **invarianti minime**: l’evento deve rifiutare un `JokeId` non valido, in particolare `JokeId.Empty`.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo, non parametrico.

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto delle proprietà (asserzione semantica chiara).
- **`.Throw<DomainValidationException>()`**: verifica del tipo di eccezione di dominio.
- **`.WithMessage(...)`**: vincolo sul messaggio (centralizzato nel dominio).
- **`.Which.MemberName`**: vincolo sul nome del parametro/membro che ha causato l’errore.

### ✅ Lambda “act” (per testare eccezioni)
Nei test che devono fallire, l’azione viene incapsulata in una lambda:

```csharp
var act = () => /* operazione che deve lanciare */;
```

Questo consente a FluentAssertions di intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `JokeWasCreatedTests.cs`)

### 🧪 Test: `Constructor_ShouldPopulateProperties_WhenValuesAreValid`

**Scopo:** verificare che il costruttore dell’evento popoli tutte le proprietà quando i valori sono validi.

**Sintassi (estratto):**

```csharp
var jokeId = JokeId.New();
var authorId = UserId.Create("author-1");
var question = QuestionText.Create("Why did the chicken cross the road?");
var answer = AnswerText.Create("To get to the other side.");
var createdAt = DateTime.UtcNow;

var evt = new JokeWasCreated(jokeId, authorId, question, answer, createdAt);

evt.JokeId.Should().Be(jokeId);
evt.AuthorId.Should().Be(authorId);
evt.Question.Should().Be(question);
evt.Answer.Should().Be(answer);
evt.CreatedAt.Should().Be(createdAt);
```

**Spiegazione:**

* Il test conferma che `JokeWasCreated` funzioni come contenitore di dati: ogni proprietà deve essere **identica** al parametro ricevuto.
* `createdAt` è **deterministico** perché viene salvato in variabile e confrontato con la proprietà dell’evento (non si confrontano due `UtcNow` diversi).
* L’uso di `DateTime.UtcNow` segue una regola tipica di dominio: timestamps in **UTC** per evitare ambiguità di fuso.

---

### 🧪 Test: `Constructor_ShouldThrow_WhenJokeIdIsEmpty`

**Scopo:** verificare che il costruttore rifiuti un `JokeId` vuoto, mantenendo validi gli altri parametri per isolare la causa del fallimento.

**Sintassi (estratto):**

```csharp
var authorId = UserId.Create("author-1");
var question = QuestionText.Create("Q?");
var answer = AnswerText.Create("A!");

var act = () => new JokeWasCreated(JokeId.Empty, authorId, question, answer, DateTime.UtcNow);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.JokeIdEmpty)
   .Which.MemberName.Should().Be("jokeId");
```

**Aspettativa di dominio (vincolante):**

* `JokeId.Empty` è uno stato sentinella e non può identificare una Joke reale ⇒ l’evento è **invalid** e deve fallire.
* Il test vincola:

  * **tipo**: `DomainValidationException`;
  * **messaggio**: `JokeErrorMessages.JokeIdEmpty`;
  * **MemberName**: `"jokeId"` (nome del parametro non valido nel costruttore).

---

## 4️⃣ Conclusione

`JokeWasCreated` è un Domain Event “minimo ma completo”: trasporta i dati essenziali del fatto di creazione e applica l’invariante fondamentale di identificazione, respingendo `JokeId.Empty` con un errore di dominio preciso e tracciabile.

---