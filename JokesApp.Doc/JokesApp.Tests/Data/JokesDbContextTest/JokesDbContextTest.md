Perfetto! 🚀 Procediamo passo passo. Ti preparo **la lista dei test In-Memory**, organizzata per categoria, con **nome suggerito**, descrizione e scopo. Poi li potremo implementare uno per uno nel tuo `JokesDbContextTests.cs`.

---

## **Lista Test In-Memory – JokesDbContext**

### 1️⃣ CRUD Base

| Nome Test                                   | Scopo                                                                              |
| ------------------------------------------- | ---------------------------------------------------------------------------------- |
| `SaveJoke_ShouldPersistToDatabase`          | Verifica che una Joke venga salvata correttamente e sia leggibile dal DB.          |
| `GetJoke_ShouldReturnCorrectData`           | Recupera una Joke per ID e controlla che tutte le proprietà siano corrette.        |
| `UpdateJoke_ShouldModifyFieldsAndUpdatedAt` | Modifica question/answer e verifica che il campo `UpdatedAt` venga aggiornato.     |
| `DeleteJoke_ShouldRemoveFromDatabase`       | Cancella una Joke singola e verifica che sparisca dal DbSet.                       |
| `SaveUser_ShouldPersistToDatabase`          | Salva un ApplicationUser e verifica che sia leggibile dal DB.                      |
| `UpdateUser_ShouldModifyFieldsAndUpdatedAt` | Modifica DisplayName, Email o AvatarUrl e controlla l’aggiornamento del timestamp. |
| `DeleteUser_ShouldRemoveFromDatabase`       | Cancella un utente senza barzellette e verifica che sparisca dal DbSet.            |

---

### 2️⃣ Relazioni & Cascade

| Nome Test                                           | Scopo                                                                                          |
| --------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| `CascadeDelete_ShouldDeleteJokes_WhenUserIsDeleted` | Se elimini un utente, tutte le barzellette collegate devono essere cancellate automaticamente. |
| `UserWithMultipleJokes_ShouldHaveCorrectNavigation` | Controlla che `User.Jokes` contenga tutte le barzellette associate.                            |
| `Joke_ShouldHaveCorrectAuthorNavigation`            | Verifica che `Joke.Author` punti all’utente corretto.                                          |

---

### 3️⃣ Validazioni & Constraint a livello di modello

| Nome Test                                           | Scopo                                                                            |
| --------------------------------------------------- | -------------------------------------------------------------------------------- |
| `SaveJoke_WithNullQuestion_ShouldThrowException`    | La creazione di una Joke con question null o vuota deve fallire.                 |
| `SaveJoke_WithTooLongQuestion_ShouldThrowException` | Verifica che question > 200 caratteri lanci eccezione.                           |
| `SaveJoke_WithNullAnswer_ShouldThrowException`      | Verifica che answer null o vuota lanci eccezione.                                |
| `SaveJoke_WithTooLongAnswer_ShouldThrowException`   | Verifica che answer > 500 caratteri lanci eccezione.                             |
| `SetAuthor_ShouldSetAuthorCorrectly`                | Verifica che `SetAuthor()` imposti correttamente `Author` e `ApplicationUserId`. |
| `SetAuthor_ShouldThrowIfAlreadySet`                 | Se Author è già impostato, deve lanciare eccezione.                              |
| `UpdateJoke_ShouldThrowOnInvalidData`               | Verifica che `Update()` non accetti valori null, vuoti o troppo lunghi.          |

---

### 4️⃣ Metodi Custom & Logica di dominio

| Nome Test                                            | Scopo                                                                 |
| ---------------------------------------------------- | --------------------------------------------------------------------- |
| `IsAuthoredBy_ShouldReturnTrueForCorrectUserId`      | Controlla che il metodo ritorni true se userId coincide con l’autore. |
| `IsAuthoredBy_ShouldReturnFalseForWrongOrNullUserId` | Ritorna false se userId non coincide o è null/bianco.                 |

---

### 5️⃣ Timestamp e integrità dei dati

| Nome Test                          | Scopo                                                                                          |
| ---------------------------------- | ---------------------------------------------------------------------------------------------- |
| `CreatedAt_ShouldBeSetOnCreation`  | Controlla che `CreatedAt` sia valorizzato automaticamente.                                     |
| `UpdatedAt_ShouldBeNullOnCreation` | Controlla che `UpdatedAt` sia null all’inizio.                                                 |
| `UpdatedAt_ShouldChangeOnUpdate`   | Verifica che `UpdatedAt` cambi quando viene eseguito `Update()` o modifiche di proprietà User. |

---

💡 **Strategia di implementazione:**

* Ogni test deve usare **un nuovo DbContext in-memory** con `Guid.NewGuid().ToString()` come nome del DB.
* Test indipendenti tra loro (nessuna contaminazione).
* FluentAssertions per verifiche: `Should().Be()`, `Should().NotBeNull()`, `Should().Throw<Exception>()`.

---

Se vuoi, il passo successivo è **iniziare a implementare i test nel file `JokesDbContextTests.cs`**, cominciando dalla prima categoria CRUD Base, uno per uno, in modo strutturato e commentato.

Vuoi che iniziamo subito con il **primo test `SaveJoke_ShouldPersistToDatabase`**?
