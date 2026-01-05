# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Events/06b_JokeWasLikedTests.md — Manuale didattico per `JokeWasLiked`

> **File di test:** `JokesApp.Tests/Domain/Events/JokeWasLikedTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.Events.JokeWasLikedTests`
>
> **Soggetto:** Domain Event `JokeWasLiked` (Domain Layer)

---

## 1️⃣ Scopo

Questa suite verifica il Domain Event `JokeWasLiked` come “fatto” del dominio:

- **completezza dei dati**: l’evento deve riportare correttamente l’identificativo della `Joke` e il numero di like dopo la modifica (`LikesAfterChange`);
- **coerenza interna**: le proprietà esposte devono corrispondere ai parametri passati al costruttore;
- **validazione dei parametri**: l’evento deve rifiutare valori negativi per `LikesAfterChange`.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo, non parametrico.

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto delle proprietà.
- **`.Throw<DomainValidationException>()`**: verifica del tipo di eccezione.
- **`.WithMessage(...)`**: vincolo sul messaggio (centralizzato nel dominio).
- **`.Which.MemberName`**: vincolo sul parametro/membro che ha causato l’errore.

### ✅ Lambda “act” (per testare eccezioni)
Nei test che devono fallire, l’azione viene incapsulata in una lambda:

```csharp
var act = () => /* operazione che deve lanciare */;
```

Questo consente a FluentAssertions di intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `JokeWasLikedTests.cs`)

### 🧪 Test: `Constructor_ShouldPopulateProperties_WhenValuesAreValid`

**Scopo:** verificare che il costruttore dell’evento popoli correttamente le proprietà quando i valori sono validi.

**Sintassi (estratto):**

```csharp
var jokeId = JokeId.New();

var evt = new JokeWasLiked(jokeId, 3);

evt.JokeId.Should().Be(jokeId);
evt.LikesAfterChange.Should().Be(3);
```

**Spiegazione:**

* Il test conferma che l'evento `JokeWasLiked` sia un "contenitore di dati" che trasporta i valori forniti al costruttore.
* `LikesAfterChange` rappresenta il conteggio finale dei like, non il delta dell’operazione.

---

### 🧪 Test: `Constructor_ShouldThrow_WhenLikesAreNegative`

**Scopo:** verificare che il costruttore lanci un’eccezione quando il numero di like è negativo.

**Sintassi (estratto):**

```csharp
var act = () => new JokeWasLiked(JokeId.New(), -1);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached)
   .Which.MemberName.Should().Be("likesAfterChange");
```

**Aspettativa di dominio:**

* `LikesAfterChange` non può essere negativo. Se viene passato un valore negativo, deve essere sollevata una `DomainValidationException` con il messaggio di errore centralizzato.

---

## 4️⃣ Conclusione

`JokeWasLiked` è un Domain Event che mantiene la coerenza sul conteggio dei like post‑operazione e valida i parametri di ingresso, respingendo i like negativi.

---
