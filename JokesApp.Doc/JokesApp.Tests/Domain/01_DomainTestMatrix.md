# 📘 **01_DomainTestMatrix.md — Matrice dei test unitari per Value Object, Aggregate e AggregateRoot**

## *Copertura minima garantita per il dominio (`ValueObjects`, `Joke`, `ApplicationUser`, `AggregateRoot`)*

---

## 1️⃣ Scopo e riferimenti

Questa matrice definisce **cosa testare** a livello di dominio puro, indicando per ogni componente:

* categorie di verifica e casi limite;
* messaggi d’errore e invarianti da controllare;
* file di test previsti in `JokesApp.Tests/Domain/...`;
* documentazione di dettaglio già presente:
  * `JokesApp.Doc/JokesApp.Tests/Model/03_JokeTest.md`;
  * `JokesApp.Doc/JokesApp.Tests/Model/04_ApplicationUserTest.md`;
  * `JokesApp.Doc/JokesApp.Tests/Domain/Attributes/04_CustomEmailAttributeTest.md`.

---

## 2️⃣ Value Object — matrice di verifica

| Value Object            | Casi positivi                                                                                                                                                    | Casi negativi / edge                                                                                                                                                                             | Note di implementazione test                                                                 |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `QuestionText`          | *Create* con testo valido (trim applicato), lunghezza = `MaxLength`, `ToString` restituisce il valore; `Length` coerente.                                         | `null`, `""`, whitespace → `DomainValidationException` (`QuestionNullOrEmpty`); lunghezza > `MaxLength` → `QuestionTooLong`.                                                                    | Fixture: usa stringhe boundary (`MaxLength`, `MaxLength + 1`), controlla `IsEmpty` su `Empty`. |
| `AnswerText`            | *Create* con testo valido, trim, supporto lunghezza limite (`MaxLength`); `ToString`.                                                                            | `null` / vuoto / whitespace → `AnswerNullOrEmpty`; oltre `MaxLength` → `AnswerTooLong`.                                                                                                         | Verifica anche `IsEmpty` su `Empty`.                                                            |
| `DisplayName`           | Trim automatico, lunghezza = `MaxLength`, `IsEmpty` false per valore valido.                                                                                     | `null` / vuoto / whitespace → `DisplayNameRequired`; lunghezza > `MaxLength` → `DisplayNameMaxLength`.                                                                                          | Usa stringhe con spazi laterali per verificare il trim.                                         |
| `AvatarUrl`             | `Create` con `http`/`https` valido, ritorno `Empty` per `null`/vuoto/whitespace; `IsEmpty` true per `Empty`.                                                     | Lunghezza > `MaxLength` → `AvatarUrlMaxLength`; URI non valido o schema diverso da `http/https` → `AvatarUrlInvalid`.                                                                           | Copri anche URL con spazi esterni (trim).                                                       |
| `EmailAddress`          | Formati validi (domini con trattini, sottodomini), lunghezza = `MaxLength`, trim.                                                                                | `null` / vuoto → `EmailRequired`; oltre `MaxLength` → `EmailTooLong`; regex fallita (doppia `@@`, domini invalidi, Unicode) → `EmailInvalid`.                                                    | Allinea i dati di test alla regex interna.                                                      |
| `UserId`                | `Create` con id valido (trim), lunghezza = `MaxLength`; `ToString` restituisce il valore; confronto di uguaglianza strutturale.                                  | `null` / vuoto / whitespace → `UserIdNullOrEmpty`; lunghezza > `MaxLength` → `UserIdTooLong`.                                                                                                    | Verifica `IsEmpty` su `Empty` e sul default struct.                                             |
| `JokeId`                | `New()` genera Guid != `Empty`, `Create(Guid)` accetta Guid non vuoto, `ToString` restituisce il Guid.                                                           | `Create(Guid.Empty)` → `JokeIdEmpty`; `IsEmpty` true per `Empty` e per struct di default.                                                                                                       | Controlla che `New()` produca valori diversi in chiamate successive.                            |

---

## 3️⃣ Aggregates — `Joke` e `ApplicationUser`

### 🟦 Joke (Aggregate Root)

| Area                     | Casi da coprire                                                                                                                                                                                      | Note sui messaggi/eventi                                                                                                  |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| Creazione (`Create`)     | Costruzione con VO validi → proprietà impostate, `Likes = 0`, `UpdatedAt = null`; generazione evento `JokeWasCreated` con `Id`, `ApplicationUserId`, `Question`, `Answer`, `CreatedAt`.               | Verifica `DomainEvents` contiene un solo evento e `PullDomainEvents()` lo svuota.                                         |
| Invarianti               | `QuestionText`/`AnswerText` null/empty → `QuestionNullOrEmpty` / `AnswerNullOrEmpty`; `UserId.IsEmpty` → `UserIdNullOrEmpty`; domanda = risposta (case-insensitive) → `QuestionAndAnswerCannotMatch`. | Usa `ValidateIntegrity()` per scenari già creati.                                                                         |
| Aggiornamento (`Update`) | Aggiornamento con autore corretto → `Question`/`Answer` aggiornati, `UpdatedAt` valorizzato, evento `JokeWasUpdated`; mismatch `userId` → `UnauthorizedDomainOperationException` (`UpdateNotAllowed`).  | Controlla che gli eventi vengano accodati e poi svuotati via `PullDomainEvents()`.                                        |
| Author management        | `SetAuthor` con `null` → `AuthorNull`; `author.Id` vuoto → `UserIdNullOrEmpty`; autore già presente → `AuthorAlreadySet`; `author.Id != ApplicationUserId` → `AuthorIdMismatch`; assegnazione valida.  | Dopo `SetAuthor` la proprietà `Author` è popolata e non cambia su `Update`.                                              |
| Like / Unlike            | `AddLike` incrementa e genera `JokeWasLiked`; overflow `int.MaxValue` → `MaximumLikeOfJokeReached`; `RemoveLike` decrementa e genera `JokeWasUnliked`; se `Likes = 0` → `MinimumLikeOfJokeReached`.    | Testare accumulo di eventi multipli prima di `PullDomainEvents()`.                                                        |
| Guardie su Id            | Metodi pubblici (`Update`, `SetAuthor`, `AddLike`, `RemoveLike`, `ValidateIntegrity`) su `Id.IsEmpty` → `JokeIdEmpty`.                                                                               | Coprire anche l’istanza tecnica costruita via costruttore EF (uso `JokeId.Empty`).                                        |

### 🟩 ApplicationUser (Aggregate)

| Area                        | Casi da coprire                                                                                                                                                                           | Note                                                                                       |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| Creazione dominio           | Costruttore con VO validi → proprietà impostate, `Jokes` inizializzato, `UpdatedAt = null`; `CreatedAt` in UTC.                                                                          | Nessun Domain Event previsto.                                                              |
| Invarianti in ingresso      | `UserId.Empty` → `UserIdNullOrEmpty`; `DisplayName.Empty` → `DisplayNameRequired`; `Email.Empty` → `EmailRequired`; `AvatarUrl.Empty` è ammesso.                                         | Usa messaggi da `ApplicationUserErrorMessages`.                                           |
| `ValidateIntegrity()`       | Stato valido non lancia; `Id.Empty` / `DisplayName.Empty` / `Email.Empty` producono le rispettive `DomainValidationException`.                                                            | Utile per istanze reidratate o default.                                                    |
| `UpdateProfile`             | Aggiornamento con `displayName` e `avatarUrl` validi → proprietà aggiornate, `UpdatedAt` valorizzato; email `null` mantiene valore precedente; email `Empty` → `EmailRequired`.            | Verifica che il trim/validazione resti delegato ai VO.                                     |
| `ChangeEmail`               | Email valida aggiorna `Email` + `UpdatedAt`; `EmailAddress.Empty` → `EmailRequired`.                                                                                                      | Coprire timestamp monotonicamente crescente rispetto a `CreatedAt`.                        |
| `SetAvatar`                 | Aggiornamento con `AvatarUrl` valido (anche `Empty`) → proprietà aggiornata, `UpdatedAt` impostato.                                                                                       | Controlla che `UpdatedAt` cambi rispetto al precedente valore.                             |
| Collezione `Jokes`          | Inizializzazione vuota; aggiunta/rimozione di `Joke` mantiene consistenza; collezioni di utenti diversi sono indipendenti.                                                                | Per ora test puri di dominio (senza EF).                                                   |

---

## 4️⃣ AggregateRoot — gestione eventi (`AddDomainEvent`, `PullDomainEvents`, `ClearDomainEvents`)

| Comportamento         | Test richiesti                                                                                                                                                                                                                                 |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AddDomainEvent`      | Lancia `ArgumentNullException` su `null`; accoda un evento concreto; preserva l’ordine di inserimento.                                                                                                                                         |
| `PullDomainEvents()`  | Con eventi presenti → restituisce snapshot read-only, svuota la coda (`DomainEvents` torna vuoto); chiamata senza eventi → ritorna `Array.Empty<IDomainEvent>()`; chiamate successive non duplicano eventi già estratti.                         |
| `ClearDomainEvents()` | Svuota la coda anche senza `Pull`, è idempotente; dopo `Clear` seguito da `Pull` viene restituito `Array.Empty<IDomainEvent>()`.                                                                                                                |
| Integrazione          | Simulare un aggregate fittizio derivato da `AggregateRoot` per verificare l’interazione tra `AddDomainEvent` e `PullDomainEvents`, evitando di usare le entità reali quando non serve un contesto di dominio completo.                         |

---

## 5️⃣ Struttura file di test consigliata (`JokesApp.Tests`)

```
JokesApp.Tests/
 └─ Domain/
     ├─ ValueObjects/
     │   ├─ QuestionTextTests.cs
     │   ├─ AnswerTextTests.cs
     │   ├─ DisplayNameTests.cs
     │   ├─ AvatarUrlTests.cs
     │   ├─ EmailAddressTests.cs
     │   ├─ UserIdTests.cs
     │   └─ JokeIdTests.cs
     ├─ Aggregates/
     │   ├─ JokeTests.cs          (focalizzato su eventi, invarianti, like/update)
     │   └─ ApplicationUserTests.cs
     └─ Primitives/
         └─ AggregateRootTests.cs (fake aggregate per pull/clear eventi)
```

*La matrice è allineata con le convenzioni esistenti: test granulari, naming AAA, uso di `FluentAssertions` per le asserzioni semantiche e controllo esplicito dei messaggi di dominio.*

---