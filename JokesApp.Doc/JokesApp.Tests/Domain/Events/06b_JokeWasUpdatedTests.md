# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Events/06b_JokeWasUpdatedTests — Manuale didattico per `JokeWasUpdated`

> **File di test:** `JokesApp.Tests/Domain/Events/JokeWasUpdatedTests.cs`
> **Classe di test:** `JokesApp.Tests.Domain.Events.JokeWasUpdatedTests`
> **Soggetto:** Domain Event `JokeWasUpdated` (Domain Layer)

---

## 1️⃣ Scopo

Questa suite di test verifica il comportamento dell'evento `JokeWasUpdated`, che rappresenta l'operazione di aggiornamento di una barzelletta. Il test si concentra su due aspetti principali:

* **Completezza dei dati**: l'evento deve riportare correttamente l’identificativo della `Joke`, il nuovo testo della domanda (`NewQuestion`), il nuovo testo della risposta (`NewAnswer`) e il timestamp dell'aggiornamento (`UpdatedAt`).
* **Coerenza interna**: le proprietà esposte devono riflettere esattamente i parametri passati al costruttore al momento della creazione dell'evento.
* **Validazione dei parametri**: l'evento deve rifiutare valori invalidi, come un timestamp `UpdatedAt` impostato a `default(DateTime)`.

L'evento `JokeWasUpdated` è un esempio di "fatto" significativo all'interno del dominio, che rappresenta una modifica al contenuto di una barzelletta, specificamente al testo della domanda e della risposta, con un timestamp associato a tale operazione.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit

* **`[Fact]`**: Test singolo, non parametrico. Ogni metodo di test viene eseguito in modo indipendente, garantendo l'isolamento dei test.

### ✅ FluentAssertions

* **`.Should().Be(...)`**: Confronto diretto delle proprietà, utilizzato per verificare che il valore di una proprietà sia uguale a quello atteso.
* **`.Throw<DomainValidationException>()`**: Verifica che venga sollevata una specifica eccezione quando il valore del parametro non è valido.
* **`.WithMessage(...)`**: Assicura che l’eccezione sollevata contenga il messaggio di errore previsto, centralizzato nel dominio per garantire uniformità.
* **`.Which.MemberName`**: Verifica che l'errore sia associato al parametro specifico che ha causato l'errore (in questo caso, `updatedAt`).

### ✅ Lambda “act” (per testare eccezioni)

Nei test che devono verificare la corretta gestione delle eccezioni, l’azione viene incapsulata in una lambda. Questo approccio consente a FluentAssertions di intercettare e gestire in modo deterministico l'eccezione sollevata durante l'esecuzione del test.

```csharp
var act = () => /* operazione che deve lanciare */;
```

---

## 3️⃣ Test documentati (1:1 con `JokeWasUpdatedTests.cs`)

### 🧪 Test: `Constructor_ShouldPopulateProperties_WhenValuesAreValid`

**Scopo:** verificare che il costruttore dell'evento popoli correttamente le proprietà quando i valori sono validi. Questo test conferma che l'evento `JokeWasUpdated` funzioni come un contenitore di dati, trasportando correttamente l'ID della barzelletta, il nuovo testo della domanda (`NewQuestion`), il nuovo testo della risposta (`NewAnswer`) e il timestamp (`UpdatedAt`).

**Sintassi (estratto):**

```csharp
var jokeId = JokeId.New();
var question = QuestionText.Create("New question");
var answer = AnswerText.Create("New answer");
var updatedAt = DateTime.UtcNow;

var evt = new JokeWasUpdated(jokeId, question, answer, updatedAt);

evt.JokeId.Should().Be(jokeId);
evt.NewQuestion.Should().Be(question);
evt.NewAnswer.Should().Be(answer);
evt.UpdatedAt.Should().Be(updatedAt);
```

**Spiegazione:**

* **`JokeId.New()`**: genera un nuovo ID valido per una barzelletta.
* **`NewQuestion`** e **`NewAnswer`**: i nuovi testi della domanda e della risposta che vengono aggiornati nell'evento.
* **`UpdatedAt`**: il timestamp dell'operazione di aggiornamento che deve essere correttamente valorizzato con un valore UTC.

---

### 🧪 Test: `Constructor_ShouldThrow_WhenUpdatedAtIsDefault`

**Scopo:** verificare che il costruttore lanci un'eccezione di tipo `DomainValidationException` quando il timestamp (`UpdatedAt`) è impostato a `default(DateTime)`, che equivale a `DateTime.MinValue`. Questo valore non è valido per un timestamp di dominio, poiché non rappresenta un'operazione di aggiornamento reale.

**Sintassi (estratto):**

```csharp
var act = () => new JokeWasUpdated(jokeId, question, answer, default);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.JokeUpdatedAtInvalid)
   .Which.MemberName.Should().Be("updatedAt");
```

**Aspettativa di dominio:**

* Se **`UpdatedAt`** è impostato a `default(DateTime)`, l'operazione di aggiornamento deve essere respinta.
* La validazione deve sollevare un'eccezione `DomainValidationException` con il messaggio di errore centralizzato `JokeErrorMessages.JokeUpdatedAtInvalid`.
* **`MemberName`**: L'errore deve essere associato correttamente al parametro che ha causato l'errore, in questo caso `updatedAt`.

---

## 4️⃣ Conclusione

L'evento `JokeWasUpdated` garantisce che ogni operazione di aggiornamento del testo della barzelletta rispetti le regole del dominio e che il timestamp sia valido. La suite di test verifica che:

* L'evento contenga correttamente i dati necessari, inclusi l'ID della barzelletta, i nuovi testi della domanda e della risposta, e il timestamp.
* Il valore di `UpdatedAt` non possa essere impostato su `default(DateTime)`, per evitare che eventi non validi vengano processati.
* Le eccezioni vengano sollevate quando i parametri non sono validi, proteggendo il dominio da stati incoerenti.

---