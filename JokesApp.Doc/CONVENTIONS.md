## Convenzioni di Codice C# adottate nel progetto

### 1. Convenzioni generali (linee guida Microsoft)

Il progetto segue le **C# Coding Conventions** raccomandate da Microsoft, che rappresentano lo standard de facto per la maggior parte dei progetti .NET.

#### 1.1 Nomi di classi, interfacce, enum e struct

* Utilizzo di **PascalCase**.
* Nomi descrittivi e chiari, che riflettano il ruolo della classe.

```csharp
public class JokesDbContext { }       // ✔
public interface IJokeService { }     // ✔
public enum JokeCategory { }          // ✔
```

#### 1.2 Nomi di metodi

* **PascalCase**.
* Nomi verbali che esprimono un’azione o un comportamento.

```csharp
public void AddUser(User user) { }        // ✔
public Joke GetRandomJoke() { }           // ✔
public async Task SaveChangesAsync() { }  // ✔
```

#### 1.3 Nomi di proprietà

* **PascalCase**.
* Nessun underscore `_` nei membri pubblici.
* Nomi che descrivono il dato esposto.

```csharp
public string DisplayName { get; set; }   // ✔
public int LikesCount { get; set; }       // ✔
public DateTime CreatedAt { get; set; }   // ✔
```

#### 1.4 Variabili locali, parametri e campi privati

* **camelCase** per variabili locali e parametri.
* Campi privati spesso prefissati con underscore `_` (convenzionale e molto diffuso nel mondo .NET).

```csharp
// Variabili locali e parametri
void SendMessage(string message)
{
    var jokeList = new List<Joke>();
    var messageLength = message.Length;
}

// Campo privato
private readonly IJokeService _jokeService;
```

Questa convenzione rende immediata la distinzione tra:

* parametri/metodi/proprietà pubbliche,
* campi interni alla classe,
* variabili locali.

---

### 2. Struttura delle cartelle e namespace

La struttura delle cartelle e dei namespace è pensata per essere:

* **coerente** tra progetto principale (`JokesApp.Server`) e test (`JokesApp.Tests`),
* **auto-esplicativa**, così da permettere di individuare rapidamente dove si trova una funzionalità o il relativo test.

Esempio di struttura logica del progetto server:

```text
JokesApp.Server/
 ├─ Controllers/              // API endpoints (Web API)
 ├─ DTOs/                     // Contratti API (request/response)
 ├─ Domain/                   // Dominio (DDD)
 │    ├─ Entities/
 │    ├─ ValueObjects/
 │    ├─ Events/
 │    ├─ Exceptions/
 │    ├─ Errors/
 │    └─ Primitives/
 ├─ Data/                     // Persistenza (EF Core)
 ├─ Migrations/               // Migrazioni EF Core
 ├─ Validation/               // Validazione a supporto del boundary HTTP
 └─ Program.cs                // Bootstrap/DI/pipeline
```

Il progetto di test segue una struttura a **mirror**, in modo che ogni componente del codice di produzione abbia idealmente una controparte di test in una posizione prevedibile:

```text
JokesApp.Tests/
 ├─ Data/                     // Setup e test su DbContext
 ├─ Domain/                   // Test su componenti di dominio
 ├─ Model/                    // Test su entità/model
 ├─ doc/                      // Strategia/target di test
 ├─ README.md
 └─ toDo.md
```

#### Namespace

I namespace seguono la gerarchia delle cartelle e iniziano con il nome del progetto radice.

Esempi:

```csharp
namespace JokesApp.Server.Domain.ValueObjects
{
    public sealed record EmailAddress { }
}

namespace JokesApp.Tests.Domain.ValueObjects
{
    public sealed class EmailAddressTests { }
}
```

Questo approccio:

* facilita il **mapping mentale** tra codice e test,
* rende semplice ritrovare la classe di test corrispondente a una determinata classe di produzione.

---

### 3. Convenzioni per il naming dei test

Per i metodi di test si adotta uno stile descrittivo che indichi chiaramente:

* **UnitOfWork** → cosa stiamo testando (metodo/funzione/comportamento),
* **StateUnderTest** → in quali condizioni (input, precondizioni),
* **ExpectedBehavior** → cosa ci aspettiamo che accada.

Formato consigliato:

```text
UnitOfWork_StateUnderTest_ExpectedBehavior
```

Esempi:

```csharp
public void AddJoke_WithValidUser_ShouldStoreInDatabase() { }

public void AddJoke_WithNullContent_ShouldThrowArgumentNullException() { }

public void RemoveUser_WithExistingJokes_ShouldCascadeDelete() { }

public void GetJoke_ById_ShouldReturnCorrectJoke() { }
```

In alternativa, è accettabile anche una variante in stile **Given/When/Then**, purché lo stile rimanga coerente all’interno del progetto:

```csharp
public void GivenValidUser_WhenAddingJoke_ThenJokeIsStored() { }
```

#### Linee guida pratiche

* Evitare abbreviazioni criptiche nei nomi dei test.
* Includere sempre il **comportamento atteso** nel nome, così da capire immediatamente cosa deve fallire se il test non passa.
* Raggruppare test affini nella stessa classe (es. `JokeValidationTests`, `ApplicationUserTests`, `JokesDbContextTests`).

---

### 4. Sintesi delle convenzioni adottate

1. **PascalCase** per classi, interfacce, enum, struct, metodi e proprietà.
2. **camelCase** per variabili locali e parametri; `_camelCase` per campi privati.
3. Struttura di cartelle e namespace **coerente e speculare** tra progetto principale e progetto di test.
4. Naming dei test secondo il pattern
   `UnitOfWork_StateUnderTest_ExpectedBehavior` (o variante Given/When/Then), in modo chiaro e leggibile.
5. Favorire l’uso di **commenti XML** (`///`) per documentare classi e metodi pubblici più importanti, così da supportare IntelliSense e la leggibilità del codice.

---