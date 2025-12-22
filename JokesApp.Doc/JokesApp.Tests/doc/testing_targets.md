## 🔎🧪 1. Analisi del progetto

### Folder principali lato server:

| Folder                                  | Contenuto                                        | Tipo di componente          | Note                                                                                      |
| --------------------------------------- | ------------------------------------------------ | --------------------------- | ----------------------------------------------------------------------------------------- |
| `Models`                                | `Joke.cs`, `ApplicationUser.cs`                  | Entità del dominio          | Contiene i dati principali; da testare logica interna, validazioni e mapping DTO          |
| `DTOs`                                  | `JokeDto.cs`, `UserDto.cs`, `RegisterUserDto.cs` | Data Transfer Objects       | Test di validazione, mapping, eventuali regole di input                                   |
| `Data`                                  | `JokesDbContext.cs`                              | DbContext EF Core           | Test di configurazione DbContext, CRUD operations (integration test)                      |
| Root                                    | `Program.cs`                                     | Configurazione DI e servizi | Test di bootstrap solo marginalmente con unit test; più interessante per integration test |
| (Eventuali) `Services` / `Repositories` | Non ancora menzionati                            | Logica di business          | Qui si concentrano test unitari più complessi (metodi che manipolano dati)                |

---

## 📂🧪 2. Tipologia di test per cartella/componente

### 2.1 Models (`Models`)

* **Obiettivo:** verificare integrità della logica interna e consistenza delle entità.
* **Unit test consigliati:**

  * `Joke.cs`
    * Creazione di una nuova `Joke` con dati validi → proprietà impostate correttamente
    * Creazione con dati mancanti o non validi (es. titolo nullo o troppo lungo) → verifica che l’eccezione venga sollevata o proprietà validate correttamente
  
  * `ApplicationUser.cs`
    * Validazione campi obbligatori (Email, Password hash)
    * Regole di business interne se presenti (es. ruolo di default, ecc.)

---

### 2.2 DTOs (`DTOs`)

* **Obiettivo:** testare validazioni e mapping.
* **Unit test consigliati:**

  * `RegisterUserDto`

    * Testare validazioni `[Required]`, `[EmailAddress]` → input corretto/fallito
  * `UserDto` / `JokeDto`

    * Verifica mapping da `Model` a `DTO` e viceversa (eventuale conversione tramite AutoMapper se lo userai)
  * Eventuali metodi di helper o validazione interna ai DTO

---

### 2.3 DbContext (`Data/JokesDbContext.cs`)

* **Obiettivo:** test CRUD e configurazione DB
* **Unit test** (con *in-memory database* per EF Core):

  * Creazione di un `Joke` → verifica salvataggio
  * Lettura di una `Joke` esistente
  * Update di una `Joke`
  * Delete di una `Joke`
* **Integration test** (DB reale o container PostgreSQL):

  * Migrazioni applicate correttamente
  * Verifica vincoli DB (chiavi primarie, FK se ci sono relazioni future)
  * Test completo di repository (es. creazione user → scrittura DB → lettura)

---

### 2.4 Program.cs / Startup / DI

* **Obiettivo:** garantire che i servizi siano registrati correttamente
* **Test consigliati:**

  * Unit test raramente necessario → più utile integration test:

    * Verifica che `JokesDbContext` sia correttamente iniettato
    * Verifica che i servizi siano risolvibili dal container DI
    * Test di avvio API minimale (WebApplicationFactory in test ASP.NET)

---

### 2.5 Servizi / Repository (quando li aggiungerai)

* **Obiettivo:** test della logica di business
* **Unit test consigliati:**

  * Metodi che creano, aggiornano, cancellano joke o utenti
  * Gestione errori (es. creazione di joke con titolo duplicato)
  * Mapping tra DTO e Model
  * Eventuali regole complesse di business (es. limitazioni o calcoli interni)

---

## 📋🧪 3. Lista completa dei test suggeriti (per ora)

| Componente            | Tipo Test   | Test specifico                              |
| --------------------- | ----------- | ------------------------------------------- |
| `Joke.cs`             | Unit        | Creazione oggetto valido                    |
| `Joke.cs`             | Unit        | Creazione oggetto con dati invalidi         |
| `ApplicationUser.cs`  | Unit        | Creazione oggetto valido                    |
| `ApplicationUser.cs`  | Unit        | Validazione campi obbligatori               |
| `RegisterUserDto`     | Unit        | Validazione Email obbligatoria              |
| `RegisterUserDto`     | Unit        | Validazione formato email                   |
| `JokeDto` / `UserDto` | Unit        | Mapping corretta da Model a DTO             |
| `JokesDbContext`      | Unit        | Inserimento joke in memory db               |
| `JokesDbContext`      | Unit        | Lettura joke da memory db                   |
| `JokesDbContext`      | Unit        | Update/Delete joke in memory db             |
| `JokesDbContext`      | Integration | CRUD reale con PostgreSQL                   |
| `Program.cs`          | Integration | Verifica DI container (DbContext + servizi) |
| (futuri servizi)      | Unit        | Logica business CRUD e regole specifiche    |

---

## ✅🧪 4. Best practice generali per i test

1. **Testare il comportamento, non l’implementazione**

   * Es. testare che `RegisterUserDto` fallisca con email non valida, non come viene fatto internamente.

2. **Separazione chiara unit / integration**

   * Unit test → *in-memory* o *mock*
   * Integration test → DB reale, API reale

3. **Usare nomi chiari**

   * Esempio: `RegisterUserDto_ShouldFail_WhenEmailIsInvalid`

4. **Coprire validazioni e regole di business**

   * Non solo CRUD “positivo”

5. **Test indipendenti**

   * Ogni test deve poter girare da solo

6. **Testare anche casi negativi**

   * Input invalidi, eccezioni attese, limiti

---

In .NET, i test del software si possono fare principalmente in due modalità: **unit testing** e **integration testing**, anche se esistono altri tipi come **functional testing** o **UI testing**. Ti spiego tutto con dettaglio, partendo dal più comune.

---