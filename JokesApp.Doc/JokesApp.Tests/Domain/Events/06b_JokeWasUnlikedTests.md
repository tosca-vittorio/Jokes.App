# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Events/06b_JokeWasUnlikedTests — Manuale didattico per `JokeWasUnliked`

> **File di test:** `JokesApp.Tests/Domain/Events/JokeWasUnlikedTests.cs`
> **Classe di test:** `JokesApp.Tests.Domain.Events.JokeWasUnlikedTests`
> **Soggetto:** Domain Event `JokeWasUnliked` (Domain Layer)

---

## 1️⃣ Scopo

Questa suite di test verifica il comportamento dell’evento di dominio `JokeWasUnliked`, che rappresenta l'operazione di rimozione di un like associato a una barzelletta. Il test si concentra su due aspetti principali:

* **Completezza dei dati**: l'evento deve riportare correttamente l'`JokeId` e il numero di like dopo l'operazione (`LikesAfterChange`).
* **Coerenza interna**: le proprietà esposte devono riflettere esattamente i parametri passati al costruttore al momento della creazione dell'evento.
* **Validazione dei parametri**: l'evento deve rifiutare valori invalidi, come un numero di like negativo, per evitare stati incoerenti nel dominio.

L’evento `JokeWasUnliked` è un esempio di un "fatto" significativo all'interno del dominio, che rappresenta una modifica allo stato di una barzelletta, specificamente alla sua interazione con gli utenti attraverso i "like". Il suo scopo è assicurarsi che ogni operazione di "unlike" sia gestita correttamente, rispettando le regole del dominio.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit

* **`[Fact]`**: Test singolo, non parametrico. Ogni metodo di test viene eseguito in modo indipendente, garantendo l'isolamento dei test.

### ✅ FluentAssertions

* **`.Should().Be(...)`**: Confronto diretto delle proprietà, utilizzato per verificare che il valore di una proprietà sia uguale a quello atteso.
* **`.Throw<DomainValidationException>()`**: Verifica che venga sollevata una specifica eccezione quando il valore del parametro non è valido.
* **`.WithMessage(...)`**: Assicura che l’eccezione sollevata contenga il messaggio di errore previsto, centralizzato nel dominio per garantire uniformità.
* **`.Which.MemberName`**: Verifica che l'errore sia associato al parametro specifico che ha causato l'eccezione (in questo caso, `likesAfterChange`).

### ✅ Lambda “act” (per testare eccezioni)

Nei test che devono verificare la corretta gestione delle eccezioni, l’azione viene incapsulata in una lambda. Questo approccio consente a FluentAssertions di intercettare e gestire in modo deterministico l'eccezione sollevata durante l'esecuzione del test.

```csharp
var act = () => /* operazione che deve lanciare */;
```

---

## 3️⃣ Test documentati (1:1 con `JokeWasUnlikedTests.cs`)

### 🧪 Test: `Constructor_ShouldPopulateProperties_WhenValuesAreValid`

**Scopo:** verificare che il costruttore dell'evento popoli correttamente le proprietà quando i valori sono validi. Questo test conferma che l'evento `JokeWasUnliked` funzioni come un contenitore di dati, trasportando correttamente l'ID della barzelletta e il numero di like dopo l'operazione di rimozione.

**Sintassi (estratto):**

```csharp
var jokeId = JokeId.New();

var evt = new JokeWasUnliked(jokeId, 0);

evt.JokeId.Should().Be(jokeId);
evt.LikesAfterChange.Should().Be(0);
```

**Spiegazione:**

* **`JokeId.New()`**: genera un nuovo ID valido per una barzelletta.
* **`LikesAfterChange`**: rappresenta il numero finale di like dopo che l'operazione di "unlike" è stata eseguita. In questo caso, il valore finale è 0, indicando che non ci sono più like.
* L'evento deve essere **immutabile**: una volta creato, non deve cambiare, quindi il confronto tra le proprietà dell'evento e i valori iniziali deve essere esatto.

---

### 🧪 Test: `Constructor_ShouldThrow_WhenLikesAreNegative`

**Scopo:** verificare che il costruttore lanci un'eccezione di tipo `DomainValidationException` quando il numero di like (`LikesAfterChange`) è negativo. Questo test è fondamentale per garantire che il dominio non accetti valori non validi, come un numero negativo di like, che non ha senso nel contesto di questo evento.

**Sintassi (estratto):**

```csharp
var act = () => new JokeWasUnliked(JokeId.New(), -1);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.MinimumLikeOfJokeReached)
   .Which.MemberName.Should().Be("likesAfterChange");
```

**Aspettativa di dominio:**

* **Numero negativo di like**: Se viene passato un valore negativo per `LikesAfterChange`, l'operazione deve essere rifiutata. La validazione interna del dominio deve sollevare un'eccezione di tipo `DomainValidationException` con il messaggio centralizzato `JokeErrorMessages.MinimumLikeOfJokeReached`.
* **`MemberName`**: L'errore deve essere associato correttamente al parametro che ha causato l'errore, che in questo caso è `likesAfterChange`.

---

## 4️⃣ Conclusione

`JokeWasUnliked` è un Domain Event che garantisce la coerenza del conteggio dei like dopo un'operazione di "unlike". La suite di test verifica che:

* L'evento contenga correttamente i dati necessari (ID della barzelletta e il numero finale di like).
* I valori passati al costruttore vengano assegnati correttamente.
* Le eccezioni vengano sollevate quando si cerca di impostare un numero negativo di like, proteggendo il dominio da stati inconsistenti.

---