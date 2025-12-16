# 📘 **05a_Joke.md**

### *Entità di dominio e Aggregate Root del sottodominio “Joke”*

---

## 5.1 Ruolo nel modello di dominio

`Joke` è l’**entità di dominio principale** (aggregate root) che rappresenta una barzelletta
creata da un utente.

All’interno del modello:

- coordina diversi **Value Object** del dominio:
  - `JokeId` → identificatore tipizzato della barzelletta,
  - `QuestionText` → testo della domanda,
  - `AnswerText` → testo della risposta,
  - `UserId` → identificatore tipizzato dell’autore;
- applica **regole di business** legate alla joke, tra cui:
  - la relazione tra domanda e risposta,
  - l’associazione autore ↔ barzelletta,
  - le operazioni di aggiornamento,
  - il sistema di like/unlike;
- genera e accumula **eventi di dominio** (`JokeWasCreated`, `JokeWasUpdated`,
  `JokeWasLiked`, `JokeWasUnliked`), che verranno poi pubblicati dall’Application Layer.

Per i dettagli sui principali Value Object utilizzati da `Joke` si vedano:

- `04a_QuestionText.md`
- `04a_AnswerText.md`
- `04a_JokeId.md`
- `04a_UserId.md`

`Joke` appartiene al **Domain Layer** e non dipende da Entity Framework/HTTP/JSON/DTO.

Tali elementi sono delegati ad Application/Data layer, che si limitano a **mappare**
lo stato della `Joke` verso l’esterno.

---

## 5.2 Struttura e dipendenze

La classe è definita in:

```csharp
namespace JokesApp.Server.Domain.Entities
{
    public sealed class Joke : AggregateRoot
    {
        // ...
    }
}
```

Dipende esclusivamente da:

* **BCL**: `System`, `System.Collections.Generic`, `System.Linq`;
* **Domain Layer**:

  * `JokesApp.Server.Domain.ValueObjects` → `JokeId`, `QuestionText`, `AnswerText`, `UserId`;
  * `JokesApp.Server.Domain.Events` → `IDomainEvent`, `JokeWasCreated`, `JokeWasUpdated`,
    `JokeWasLiked`, `JokeWasUnliked`;
  * `JokesApp.Server.Domain.Errors` → `JokeErrorMessages` (cfr. `03_JokeErrorMessages.md`);
  * `JokesApp.Server.Domain.Exceptions` → `DomainValidationException`,
    `DomainOperationException`, `UnauthorizedDomainOperationException`
    (cfr. `01_DomainValidationException.md`, `01_DomainOperationException.md`,
    `01_UnauthorizedDomainOperationException.md`).

Sono stati volutamente rimossi:

* attributi infrastrutturali (`[NotMapped]`, `[JsonIgnore]`, ecc.),
* riferimenti a `Models` o ad altri livelli applicativi.

Questo rende `Joke` una vera entità di **dominio puro**, adatta a Clean Architecture.

---

## 5.3 Proprietà principali e invarianti

Le proprietà principali sono:

```csharp
public JokeId Id { get; private set; }
public QuestionText Question { get; private set; }
public AnswerText Answer { get; private set; }
public UserId ApplicationUserId { get; private set; }
public ApplicationUser? Author { get; private set; }
public DateTime CreatedAt { get; private set; }
public DateTime? UpdatedAt { get; private set; }
public int Likes { get; private set; }
```

Gli **invarianti** garantiti dal dominio sono:

1. **Question e Answer sono sempre Value Object validi**

   * `Question` e `Answer` vengono passati come `QuestionText` e `AnswerText`.
   * Le loro regole (non null, non vuoti, lunghezza massima, ecc.) sono già verificate
     a monte dai rispettivi VO (vedi `04a_QuestionText.md` e `04a_AnswerText.md`).

2. **Question e Answer non possono essere identiche**

   Implementato nel metodo privato:

   ```csharp
   private static void EnsureQuestionAndAnswerAreDifferent(QuestionText q, AnswerText a)
   {
       if (string.Equals(q.Value, a.Value, StringComparison.OrdinalIgnoreCase))
       {
           throw new DomainValidationException(
              JokeErrorMessages.QuestionAndAnswerCannotMatch,
              nameof(Question));
       }
   }
   ```

   Questo vincolo è applicato:

   * nel costruttore di dominio,
   * nel metodo `Update`,
   * in `ValidateIntegrity()` (per verifiche interne/test).

3. **ApplicationUserId è un Value Object valido**

   * `ApplicationUserId` è di tipo `UserId`.
   * Le regole di validazione (non nullo, non vuoto, lunghezza massima, ecc.) sono incapsulate
     nel VO `UserId` (vedi `04a_UserId.md`), non nell’entità.

4. **Coerenza autore ↔ ApplicationUserId (quando l’autore è presente)**

   * La proprietà di navigazione `Author` è opzionale e rappresenta un **enrichment**:
     l’entità `Joke` può essere perfettamente valida anche con `Author == null`,
     purché `ApplicationUserId` sia coerente e valido.

   * Quando `Author` viene valorizzato, `SetAuthor` garantisce che:
     * l’istanza non sia nulla;
     * l’`Id` dell’autore non sia vuoto (`author.Id.IsEmpty == false`);
     * l’autore non sia già stato impostato;
     * `author.Id` sia coerente con `ApplicationUserId` (confronto tra due `UserId` tipizzati).

   In altre parole, l’invariante “forte” del dominio riguarda **sempre** `ApplicationUserId`;
   `Author` è un riferimento aggiuntivo che deve essere coerente *se e solo se* è valorizzato.

5. **Likes non vanno mai sotto 0 né oltre `int.MaxValue`**

   * `AddLike()` evita overflow.
   * `RemoveLike()` impedisce di scendere sotto 0.
   * Qualsiasi violazione produce una `DomainOperationException` con i messaggi
     definiti in `JokeErrorMessages` (vedi `03_JokeErrorMessages.md`).

---

## 5.4 Costruttori e ciclo di vita

La classe espone due costruttori:

```csharp
/// <summary>
/// Costruttore protetto richiesto dagli strumenti di persistenza (es. ORM/strumenti di persistenza).
/// Non deve essere utilizzato manualmente nel codice di dominio.
/// </summary>
protected Joke()
{
}

/// <summary>
/// Costruttore principale del dominio.
/// Esegue validazioni, assegna i Value Objects e genera un evento di creazione.
/// </summary>
/// <param name="question">Value Object contenente la domanda.</param>
/// <param name="answer">Value Object contenente la risposta.</param>
/// <param name="userId">Identificatore tipizzato dell'autore.</param>
public Joke(QuestionText question, AnswerText answer, UserId userId)
{
    if (question is null || question.IsEmpty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.QuestionNullOrEmpty,
            nameof(question));
    }

    if (answer is null || answer.IsEmpty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.AnswerNullOrEmpty,
            nameof(answer));
    }

    if (userId.IsEmpty)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.UserIdNullOrEmpty,
            nameof(userId));
    }

    EnsureQuestionAndAnswerAreDifferent(question, answer);

    Id = JokeId.New();
    Question = question;
    Answer = answer;
    ApplicationUserId = userId;
    CreatedAt = DateTime.UtcNow;

    // Con Id domain-generated, l'evento "Created" deve nascere già con un identificatore reale.
    AddDomainEvent(new JokeWasCreated(
        Id,
        ApplicationUserId,
        Question,
        Answer,
        CreatedAt));
}
```

* Il **costruttore protetto** è pensato per gli ORM (es. ORM / strumenti di persistenza) o altri strumenti di persistenza
  e non dovrebbe essere usato nell’Application Layer.
* Il **costruttore di dominio**:

  * richiede Value Object già validi (`QuestionText`, `AnswerText`, `UserId`);
  * applica la regola “question e answer sono diverse”;
  * inizializza `CreatedAt` in UTC;
  * registra un evento `JokeWasCreated` con l’`Id` **reale** generato nel dominio tramite `JokeId.New()`, così da averlo disponibile subito (es. per Domain Events)..

Per i dettagli su `JokeWasCreated` e sugli altri eventi di dominio, si veda la documentazione
del sottosistema eventi (`Domain/Events`).

---

## 5.5 Gestione autore (`SetAuthor` e `IsAuthoredBy`)

L’entità espone:

```csharp
public ApplicationUser? Author { get; private set; }

public void SetAuthor(ApplicationUser author)
{
    if (author is null)
    {
        throw new DomainValidationException(
            JokeErrorMessages.AuthorNull,
            nameof(author));
    }

    // L'utente associato deve avere un identificativo valido (non vuoto).
    if (author.Id.IsEmpty)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.UserIdNullOrEmpty,
            nameof(author));
    }

    if (Author is not null)
    {
        throw new DomainOperationException(JokeErrorMessages.AuthorAlreadySet);
    }

    // Confronto tra due UserId tipizzati
    if (!author.Id.Equals(ApplicationUserId))
    {
        throw new DomainValidationException(
            JokeErrorMessages.AuthorIdMismatch,
            nameof(author));
    }

    Author = author;
}

public bool IsAuthoredBy(UserId userId)
    => ApplicationUserId.Equals(userId);
```

Regole applicate da `SetAuthor`:

1. **Autore non nullo**  
   → `DomainValidationException(AuthorNull)` se `author` è `null`.

2. **UserId dell’autore non vuoto**  
   → `DomainValidationException(UserIdNullOrEmpty)` se `author.Id.IsEmpty` è `true`.

3. **Autore assegnato una sola volta**  
   → `DomainOperationException(AuthorAlreadySet)` se `Author` è già valorizzato.

4. **Coerenza tra Author.Id e ApplicationUserId**  
   → `DomainValidationException(AuthorIdMismatch)` se `!author.Id.Equals(ApplicationUserId)`.


`IsAuthoredBy(UserId)` fornisce un modo chiaro per verificare la proprietà della joke
rispetto a un utente ed è utilizzato anche in altre operazioni (es. `Update`).

> **Nota architetturale**
> Questa configurazione presuppone l’esistenza di una **entità di dominio** `ApplicationUser`
> nel namespace `JokesApp.Server.Domain.Entities`, separata da eventuali modelli infrastrutturali
> (ad esempio `Models/ApplicationUser` utilizzato da Identity/EF).
> Il Domain Layer lavora con la versione “pura” di `ApplicationUser`; la conversione da/verso
> i modelli concreti di Identity è responsabilità dei layer superiori (Application/Data).

---

## 5.6 Comportamenti di dominio: Update, Like, Unlike

### 5.6.1 Aggiornamento del contenuto (`Update`)

```csharp
/// <summary>
/// Aggiorna la barzelletta sostituendo domanda e risposta dopo le opportune validazioni.
/// Genera un evento di aggiornamento.
/// </summary>
/// <param name="userId">Identificatore dell'utente che richiede l'aggiornamento.</param>
/// <param name="question">Nuovo testo della domanda.</param>
/// <param name="answer">Nuovo testo della risposta.</param>
public void Update(UserId userId, QuestionText question, AnswerText answer)
{
    EnsureIdIsInitialized();

    if (question is null || question.IsEmpty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.QuestionNullOrEmpty,
            nameof(question));
    }

    if (answer is null || answer.IsEmpty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.AnswerNullOrEmpty,
            nameof(answer));
    }

    if (userId.IsEmpty)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.UserIdNullOrEmpty,
            nameof(userId));
    }

    if (!IsAuthoredBy(userId))
    {
        throw new UnauthorizedDomainOperationException(JokeErrorMessages.UpdateNotAllowed);
    }

    EnsureQuestionAndAnswerAreDifferent(question, answer);

    Question = question;
    Answer = answer;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new JokeWasUpdated(
        Id,
        Question,
        Answer,
        UpdatedAt.Value));
}
```

Regole:

* solo l’autore può aggiornare la joke → in caso contrario,
  `UnauthorizedDomainOperationException(UpdateNotAllowed)`;
* question e answer devono continuare a rispettare la regola “non identiche”;
* `UpdatedAt` viene aggiornato in UTC;
* viene generato un evento di dominio `JokeWasUpdated`.

Qui confluiscono:

* le regole locali dei VO (`QuestionText`, `AnswerText`),
* le regole di autorizzazione (`IsAuthoredBy` + `UpdateNotAllowed` in `JokeErrorMessages`),
* la pubblicazione di un evento coerente con il pattern Domain Events.

### 5.6.2 Gestione like (`AddLike` e `RemoveLike`)

```csharp
public void AddLike()
{
    EnsureIdIsInitialized();

    if (Likes == int.MaxValue)
    {
        throw new DomainOperationException(JokeErrorMessages.MaximumLikeOfJokeReached);
    }

    Likes++;

    AddDomainEvent(new JokeWasLiked(
        Id,
        Likes));
}

public void RemoveLike()
{
    EnsureIdIsInitialized();

    if (Likes == 0)
    {
        throw new DomainOperationException(JokeErrorMessages.MinimumLikeOfJokeReached);
    }

    Likes--;

    AddDomainEvent(new JokeWasUnliked(
        Id,
        Likes));
}
```

Regole:

* `Likes` non può superare `int.MaxValue` → se è già al massimo,
  `DomainOperationException(MaximumLikeOfJokeReached)`;
* `Likes` non può scendere sotto 0 → se è 0,
  `DomainOperationException(MinimumLikeOfJokeReached)`;
* ogni modifica al numero di like genera un evento corrispondente:

  * `JokeWasLiked` dopo l’incremento,
  * `JokeWasUnliked` dopo il decremento.

---

## 5.7 Gestione degli eventi di dominio

La gestione della coda degli eventi di dominio non è implementata direttamente dentro `Joke`,
ma è demandata alla base class `AggregateRoot` (Domain/Primitives).

L’aggregate:
- genera eventi significativi (`JokeWasCreated`, `JokeWasUpdated`, `JokeWasLiked`, `JokeWasUnliked`)
  tramite `AddDomainEvent(...)`;
- espone la coda tramite `DomainEvents` (read-only);
- consente all’Application Layer di estrarre e svuotare la coda tramite `PullDomainEvents()` dopo la persistenza.

Pattern adottato:
- il Domain Layer **accumula** gli eventi;
- l’Application Layer **pubblica/dispatcha** gli eventi e poi “ripulisce” la coda (pull/clear).

---

## 5.8 Validazioni di integrità (`ValidateIntegrity`)

```csharp
public void ValidateIntegrity()
{
    EnsureIdIsInitialized();
    EnsureQuestionAndAnswerAreDifferent(Question, Answer);
}
```

`ValidateIntegrity` offre un entry point esplicito per:

* test,
* procedure di import,
* controlli diagnostici,

per verificare che lo stato interno dell’entità continui a rispettare gli invarianti
di dominio (in questo caso: `Id` inizializzato e `Question`/`Answer` non identiche).

In futuro, se gli invarianti aumentano, è il posto naturale dove centralizzare
i controlli ad alto livello.

---

## 5.9 Rappresentazione testuale e logging

```csharp
public override string ToString()
    => $"Joke(Id={Id}, UserId={ApplicationUserId}, CreatedAt={CreatedAt:O})";
```

La stringa restituita da `ToString()` fornisce una rappresentazione sintetica:

* ID della joke (come `JokeId`),
* ID dell’utente autore (come `UserId`),
* data di creazione (ISO 8601, specifica `:O`).

È pensata per:

* log tecnici,
* debugging,
* messaggi diagnostici.

---

## 5.10 Coerenza con DDD, Clean Architecture, SOLID

`Joke` è progettata per rispettare i principi fondanti dell’architettura:

* **DDD**

  * aggregate root del sottodominio “Joke”;
  * coordina Value Object, invarianti e domain events;
  * incapsula completamente le regole di business legate alla vita di una barzelletta.

* **Clean Architecture**

  * vive nel Domain Layer, senza riferimenti a EF, JSON, HTTP o DTO;
  * gli altri layer si limitano a mappare lo stato della `Joke` verso database o API.

* **SOLID (SRP)**

  * responsabilità unica: rappresentare e gestire il ciclo di vita di una barzelletta
    nel dominio, con le sue regole e i suoi eventi;
  * non gestisce processi esterni (logging, persistenza, serializzazione, UI).

In sintesi, `Joke` è il punto di riferimento centrale per tutte le logiche di business
legate alle barzellette: chiunque voglia creare, aggiornare, validare o reagire ai cambiamenti
di una joke, lo fa passando da questa entità di dominio.

---
