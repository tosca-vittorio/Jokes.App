# 📘 Architettura del Progetto - **React Frontend + ASP.NET Core Web API (Clean Architecture + DDD + Hexagonal)**

## 1. Introduzione

Il progetto **JokesApp** adotta un’architettura moderna, modulare e scalabile, basata su:

* **Frontend React** sviluppato come **Single Page Application (SPA)**
* **Backend ASP.NET Core Web API** progettato secondo i paradigmi:

  * **Clean Architecture**
  * **Domain-Driven Design (DDD)**
  * **Hexagonal Architecture (Ports & Adapters)**
  * **Principi SOLID, DRY, KISS, YAGNI**
  * **CQRS leggero**

Questa combinazione offre:

* una **separazione netta** tra logica di presentazione e logica applicativa,
* alta **manutenibilità**, **testabilità** e **scalabilità**,
* possibilità di evolvere frontend e backend in modo indipendente,
* struttura robusta e adatta a scenari enterprise.

---

## 2. Visione Architetturale Complessiva

L’applicazione segue un modello **API-Driven**, con due componenti chiaramente distinte:

```
React SPA (Client)  ←→  ASP.NET Core Web API (Server)
```

Il frontend:

* non conosce il dominio,
* non contiene logica sensibile,
* non accede al database.

Il backend:

* implementa logica applicativa e di dominio,
* fornisce esclusivamente API REST in formato JSON,
* è strutturato secondo Clean Architecture.

---

## 3. Architettura Logica del Frontend (React SPA)

### 🔵 Frontend: React (SPA)

Il frontend vive interamente lato client nel browser e si occupa della:

* **presentazione**,
* **gestione stato UI** (local state + possibilità future di context/store),
* **routing client-side**,
* **comunicazione con il backend tramite HTTP/JSON**.

Struttura logica:

```
Frontend (React SPA)
├── Componenti React (UI)
├── Pages
├── Custom Hooks
├── Services (HTTP client)
└── Routing e Stato
```

Principi chiave:

* UI completamente dinamica.
* Nessuna logica di dominio.
* Nessun accesso diretto a database o regole sensibili.
* Interazione unica via API del backend.

---

## 4. Architettura Logica del Backend (ASP.NET Core Web API)

### 🔴 Backend moderno (Clean + DDD + Hexagonal)

Il backend è progettato secondo una stratificazione rigorosa:

```
API / Presentation Layer
↓
Application Layer
↓
Domain Layer
↑
Infrastructure Layer (implementa Ports)
```

---

## 5. Layer del Backend 

### 🟦 5.1 Domain Layer — Il nucleo del sistema

* Entità, Value Object, Aggregate Root
* Regole di business
* Invarianti
* Domain Events
* Domain Services
* Domain Exceptions

**Non può contenere:**

❌ EF Core
❌ HTTP
❌ Controller
❌ Logging
❌ Repository concreti

📌 È il layer più stabile e indipendente dalle tecnologie.

(Esempi reali: `JokeId`, `QuestionText`, `JokeWasCreated`, `DomainValidationException`, ecc.)

---

### 🟩 5.2 Application Layer — Casi d’uso e orchestrazione

* Implementa i **Use Case**
* Gestisce **Command** / **Query** (CQRS leggero)
* Espone **Ports** (interfacce)
* Coordina dominio e infrastruttura
* Effettua validazioni superficiali
* Dispatch finale dei Domain Events

**Non contiene:**

❌ EF Core
❌ SQL
❌ Logica di dominio profonda
❌ Controller / HTTP

---

### 🟧 5.3 Infrastructure Layer — Adapters e implementazioni tecniche

* Repository concreti (EF Core)
* `JokesDbContext`
* Conversioni e mapping
* Accesso a risorse esterne
* Logging, FileSystem, SMTP, ecc.

**Non contiene:**

❌ Regole del dominio
❌ Use Case applicativi

È il layer più “volatile”, isolato tramite le Ports dell’Application.

---

### 🟥 5.4 API Layer / Presentation Layer

* Controller ASP.NET Core
* Routing HTTP
* Request/Response DTO
* Validazione input
* Mapping API → Application (Command/Query)
* Autenticazione / Autorizzazione
* Serializzazione / Output HTTP

**Non contiene:**

❌ Logica di business
❌ Accesso diretto ai dati
❌ Regole di dominio

---

### 🟪 5.5 Cross-Cutting Layer

Funzionalità trasversali:

* middleware globali
* logging
* dependency injection
* configurazioni
* sicurezza
* rate limiting

---

### 🟫 5.6 Testing Layer

* Unit Test di Dominio
* Unit Test di Application
* Integration Test (EF Core, API)
* Test End-to-End

Coerenti con l’isolamento dei layer.

---

## 6. Confronto: API + SPA vs MVC tradizionale

*(Manteniamo questa sezione, già corretta nel README originale )*

### ✔️ API + SPA

* UI client-side
* Backend solo REST
* Scalabilità alta
* Separazione totale frontend/backend
* Perfetto per React e architetture moderne

### ❌ MVC classico

* Rendering HTML sul server
* Meno flessibile
* Architettura monolitica
* Ideale solo per admin panel o gestionali semplici

---

## 7. Struttura della Solution (Aggiornata)

Struttura logica generale:

```
JokesApp/
├── JokesApp.Client/      → React SPA
├── JokesApp.Server/      → ASP.NET Core Web API (Clean + DDD + Hexagonal)
├── JokesApp.Test/        → Test (Unit, Integration, E2E)
└── JokesApp.Doc/         → Documentazione tecnica
```

Struttura reale del backend aggiornata:

```
JokesApp.Server/
├── Controllers/                → API Layer
├── Application/ (futuro)       → UseCases, Ports, Dispatcher
├── Domain/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Errors/
│   ├── Exceptions/
│   └── Attributes/
├── Data/                       → DbContext + EF Converters
├── DTOs/                       → Request/Response DTO
├── Models/                     → (temporaneo) Identity/User/Joke (prima del refactoring completo)
├── Migrations/                 → EF Core migrations
├── Program.cs                  → Bootstrap e DI
└── appsettings.json            → Configurazioni
```

📌 Questo riflette la struttura reale del tuo progetto.

---


## 8. Frontend – `JokesApp.Client`

### 8.1 Obiettivi del frontend

`JokesApp.Client` implementa l’interfaccia utente dell’applicazione sotto forma di **Single Page Application (SPA) React**.
I suoi obiettivi principali sono:

* fornire una **UI reattiva e moderna** per la gestione delle “jokes” (barzellette / contenuti),
* orchestrare il **routing client-side**,
* consumare le API esposte da `JokesApp.Server`,
* gestire lo **stato dell’applicazione lato client**, senza contenere logica di dominio o di persistenza.

Il frontend è progettato per essere:

* **disaccoppiato** dal backend (comunica solo via HTTP/JSON),
* **testabile** a livello di componenti e logica di presentazione,
* **estensibile**, grazie a una struttura di cartelle chiara e pensata per crescere.

---

### 8.2 Struttura delle cartelle principali

> Nota: la struttura può leggermente variare in base al template e alle personalizzazioni; questa sezione descrive l’**organizzazione logica** prevista.

```text
JokesApp.Client/
├── src/
│   ├── components/        // Componenti UI riutilizzabili (bottoni, form, layout, ecc.)
│   ├── pages/             // Pagine principali collegate al router (es. Home, Login, JokesList)
│   ├── hooks/             // Custom hooks (es. useAuth, useFetchJokes, ecc.)
│   ├── services/          // Accesso alle API (client HTTP, wrapper fetch/axios)
│   ├── utils/             // Funzioni di utilità lato client (helper, formatter, ecc.)
│   ├── assets/            // Immagini, icone, risorse statiche
│   └── main.jsx           // Entry point dell’app React
├── public/                // File statici serviti direttamente (index.html, favicon, manifest, ecc.)
├── package.json           // Dipendenze npm e script di build/esecuzione
├── vite.config.* /        // Configurazione tool di build (Vite o equivalente)
└── README.md (opzionale)  // Documentazione specifica del solo frontend
```

#### Cartelle chiave

* **`src/components/`**
  Contiene i **componenti presentazionali** e i componenti UI riutilizzabili.
  Esempi:

  * `JokeCard`, `JokesList`, `Navbar`, `Layout`, `FormInput`, ecc.

  Obiettivo: favorire **riuso** e **consistenza visiva** nell’interfaccia.

* **`src/pages/`**
  Contiene le **pagine principali**, mappate sulle route dell’applicazione (es. `/`, `/login`, `/register`, `/jokes`, ecc.).
  Ogni pagina:

  * compone più componenti UI,
  * gestisce la logica di **interazione ad alto livello** (es. caricare una lista di jokes, gestire il submit di un form, ecc.).

* **`src/hooks/`**
  Contiene i **custom hooks** che incapsulano logica di stato e side-effect lato client.
  Esempi:

  * `useAuth()` per stato di autenticazione,
  * `useJokes()` per il caricamento e caching delle jokes,
  * `useApi()` per standardizzare chiamate HTTP e gestione errori.

  Vantaggi:

  * riduzione della duplicazione,
  * componenti più puliti,
  * separazione tra **logica di presentazione** e **logica di interazione**.

* **`src/services/`**
  Modulo dedicato all’**accesso al backend**.
  Qui vengono definiti:

  * client HTTP (es. wrapper di `fetch` o `axios`),
  * funzioni di alto livello come `getJokes()`, `createJoke()`, `registerUser()`, ecc.

  È il punto unico in cui è noto il **base URL** delle API, i path (`/api/jokes`, `/api/auth`, …) e le eventuali intestazioni (token, auth, ecc.).

* **`src/utils/`**
  Funzioni di utilità lato UI, come:

  * formatter di date,
  * normalizzazione degli errori,
  * mapping da DTO di backend a modelli UI, se necessario.

* **`src/assets/`**
  Contiene risorse statiche **non servite direttamente** ma importate nei componenti (loghi, immagini decorative, icone).

---

### 8.3 Comunicazione con il backend

Il frontend comunica con `JokesApp.Server` tramite:

* chiamate HTTP verso endpoint REST (es. `GET /api/jokes`, `POST /api/auth/register`, ecc.),
* scambio dati in formato **JSON**,
* eventuale gestione di **token di autenticazione** (es. JWT) tramite header HTTP.

Tutta la logica di comunicazione è **centralizzata** in `src/services/` per:

* evitare la dispersione di URL e gestione errori nei componenti,
* permettere test mirati sulle funzioni di accesso dati,
* semplificare una futura migrazione o refactoring dell’API.

---

## 9. Backend – `JokesApp.Server`

### 9.1 Obiettivi del backend

`JokesApp.Server` è un’applicazione **ASP.NET Core Web API** che fornisce:

* endpoint REST per la gestione delle jokes,
* gestione utenti e autenticazione/registrazione,
* validazione dei dati in ingresso,
* accesso al database PostgreSQL tramite Entity Framework Core.

L’obiettivo è mantenere il backend:

* **coerente** con i principi DDD-light (modelli di dominio separati dai DTO),
* **estendibile**, grazie a una buona separazione per layer,
* **testabile**, tramite JokesApp.Test e l’uso di DTO/servizi ben definiti.

---

### 9.2 Struttura delle cartelle principali

```text
JokesApp.Server/
├── Controllers/                        // API Layer (Presentation)
│   └── WeatherForecastController.cs
│
├── Domain/                             // Domain Layer (DDD)
│   ├── Attributes/                     // Validazioni e attributi custom (es. Email)
│   │   └── CustomEmailAttribute.cs
│   │
│   ├── Errors/                         // Messaggi di errore di dominio
│   │   ├── ApplicationUserErrorMessages.cs
│   │   ├── ErrorMessages.cs
│   │   └── JokeErrorMessages.cs
│   │
│   ├── Events/                         // Domain Events
│   │   ├── DomainEvent.cs
│   │   ├── IDomainEvent.cs
│   │   ├── JokeWasCreated.cs
│   │   ├── JokeWasLiked.cs
│   │   ├── JokeWasUnliked.cs
│   │   └── JokeWasUpdated.cs
│   │
│   ├── Exceptions/                     // Eccezioni di dominio
│   │   ├── DomainException.cs
│   │   ├── DomainOperationException.cs
│   │   ├── DomainValidationException.cs
│   │   └── UnauthorizedDomainOperationException.cs
│   │
│   └── ValueObjects/                   // Value Objects (DDD)
│       ├── AnswerText.cs
│       ├── JokeId.cs
│       ├── QuestionText.cs
│       └── UserId.cs
│
├── Models/                             // Modelli persistenti / Identity models
│   ├── ApplicationUser.cs
│   └── Joke.cs
│
├── DTOs/                               // Request/Response DTO (API Contracts)
│   ├── JokeDto.cs
│   ├── RegisterUserDto.cs
│   └── UserDto.cs
│
├── Data/                               // Infrastructure (Data access)
│   ├── JokesDbContext.cs               // EF Core DbContext
│   └── Converters/                     // EF Core ValueConverters per i Value Objects
│       ├── AnswerTextConverter.cs
│       ├── JokeIdConverter.cs
│       ├── QuestionTextConverter.cs
│       └── UserIdConverter.cs
│
├── Migrations/                         // Migrazioni EF Core
│   ├── 20251201135759_InitialCreate.cs
│   ├── 20251201135759_InitialCreate.Designer.cs
│   └── JokesDbContextModelSnapshot.cs
│
├── Properties/
│   └── launchSettings.json             // Configurazioni profilo di esecuzione
│
├── Program.cs                          // Bootstrap: DI, Middleware, Routing, ecc.
├── appsettings.json                    // Configurazioni runtime
├── appsettings.Development.json        // Configurazioni ambiente Development
├── .env                                // Variabili d'ambiente (se presente)
│
└── Server_Backup/                      // Backup DB e snapshot progetto
    ├── BackupSQL/
    │   └── jokesdb_20251201_164308.backup
    └── SnapshotProject/
```

#### Cartelle chiave

* **`Controllers/`**
  Contiene i controller Web API, ciascuno responsabile di un sottoinsieme di funzionalità (es. `JokesController`, `AuthController`, `UsersController`).
  Ogni controller:

  * espone endpoint HTTP (GET, POST, PUT, DELETE, …),
  * riceve/ritorna DTO,
  * delega la logica di dominio a servizi, contesto EF, ecc.

* **`DTOs/`**
  Contiene i **Data Transfer Objects**, ossia i contratti dati utilizzati per comunicare con il frontend.
  Esempi:

  * `RegisterUserDto`: input per la registrazione utente, con attributi `[Required]`, `[EmailAddress]`, ecc.
  * `UserDto`: modello di output per rappresentare un utente lato client.
  * `JokeDto`: rappresentazione serializzabile di una barzelletta.

  Vantaggi:

  * separazione tra **modelli interni** e **contratto esterno**,
  * maggiore stabilità dell’API, indipendentemente da eventuali modifiche interne ai modelli o al database.

* **`Models/`**
  Contiene i **modelli di dominio** e, dove necessario, i modelli collegati a Identity (come `ApplicationUser`).
  Qui risiede la rappresentazione dei dati così come vengono persistiti e manipolati nel core dell’app.

* **`Domain/`**
  Contiene elementi a supporto della logica di dominio, per esempio:

  * `JokesErrorMessages.cs`: raccolta centralizzata dei messaggi di errore relativi alle jokes,
  * `ApplicationUserErrorMessages.cs`: messaggi di errore relativi a utenti, registrazione, autenticazione, ecc.

  Tenere i messaggi di errore in strutture dedicate permette:

  * uniformità dei messaggi,
  * facilità di manutenzione,
  * possibilità futura di localizzazione.

* **`Data/`**
  Contiene il `JokesDbContext`, il contesto EF Core che:

  * definisce i `DbSet<>` (es. `DbSet<Joke>`, `DbSet<ApplicationUser>`),
  * configura le relazioni,
  * gestisce le operazioni verso il database.

* **`Migrations/`**
  Cartella generata da EF Core che include le migrazioni (es. `InitialCreate`) applicate al database PostgreSQL.

* **`Program.cs` e `appsettings.json`**

  * `Program.cs`: configura servizi, DbContext, Identity, middleware, CORS, ecc.
  * `appsettings.json`: contiene parametri esterni, in particolare la **connection string** utilizzata da EF Core.

---

## 10. Test automatizzati – `JokesApp.Test`

### 10.1 Obiettivi dei test

`JokesApp.Test` contiene la batteria di test automatici del progetto.
L’obiettivo è:

* validare il comportamento di:

  * **modelli di dominio**,
  * **DTO e validazioni** (DataAnnotations, errori personalizzati),
  * **logica applicativa** più critica;
* ridurre il rischio di regressioni durante refactoring o nuove feature;
* fungere da **documentazione eseguibile** del comportamento atteso.

Il progetto è organizzato per rispecchiare (per quanto possibile) la struttura di `JokesApp.Server`, in modo da avere un **mirror logico**:

* a ogni classe importante del backend corrisponde, idealmente, una classe di test dedicata.

---

### 10.2 Struttura delle cartelle principali

```text
JokesApp.Test/
├── Unit/
│   ├── Domain/
│   │   ├── JokeTests.cs                // Test sui modelli/validazioni relativi alle jokes
│   │   └── ApplicationUserTests.cs     // Test sui modelli/validazioni relativi agli utenti
│   ├── DTOs/
│   │   └── RegisterUserDtoTests.cs     // Esempio: test delle regole di validazione del DTO di registrazione
│   └── ...                             // Altri test unitari, organizzati per area
├── Integration/
│   └── ...                             // (Futuro) Test di integrazione con Db/EF Core e API reali
└── TestUtilities/                      // (Opzionale) helper, factory, dati di esempio, ecc.
```

> Nota: i nomi delle cartelle possono variare; l’importante è mantenere una **corrispondenza chiara** con i componenti testati.

#### Convenzioni generali

* Ogni classe di produzione `Xyz` è, idealmente, associata a una classe di test `XyzTests`.
* I test seguono una nomenclatura chiara, ad esempio:

  * `MethodName_WhenCondition_ThenExpectedResult`
* Le suite di test sono pensate per coprire:

  * casi **validi** (happy path),
  * casi **invalidi** (errori, eccezioni, edge case),
  * casi **limite** (stringhe vuote, valori estremi, unicode, ecc.).

Esempi di aree attualmente coperte:

* `ApplicationUserTests.cs`:
  verifica delle regole di validazione e dei vincoli sul modello utente (email, password, ecc.),
* `JokeTests.cs`:
  verifica delle regole di dominio e validazione relative ai contenuti (es. testo non vuoto, lunghezza massima, ecc.),
* (se presenti) test sui DTO come `RegisterUserDto`, incluse le DataAnnotations e i messaggi di errore personalizzati.

---

## 11. Documentazione – `JokesApp.Doc`

`JokesApp.Doc` raccoglie tutta la **documentazione tecnica e architetturale** del progetto.
Segue la stessa struttura a macro-folder della solution:

```text
JokesApp.Doc/
├── JokesApp.Client/      // Documentazione del frontend (React, routing, state management, ecc.)
├── JokesApp.Server/      // Documentazione del backend (API, modelli, DbContext, migrazioni, ecc.)
├── JokesApp.Test/        // Documentazione del testing (strategia, tipologie di test, naming, ecc.)
├── README.md             // Overview globale dell’intero sistema (questo file)
└── ROADMAP.md            // Roadmap di sviluppo ed evoluzione
```

In particolare:

* **`README.md` (questo documento)**
  Fornisce una **visione complessiva** del sistema, dell’architettura, della solution e del ruolo dei singoli progetti.

* **`ROADMAP.md`**
  Conterrà una vista ad albero in stile diagramma directory, che sintetizza:

  * principali moduli/feature,
  * milestones,
  * possibili estensioni future.

  (La struttura sarà mantenuta minimalista e in formato puramente testuale/markdown, come richiesto.)

* Le sottocartelle:

  * `JokesApp.Client/`
  * `JokesApp.Server/`
  * `JokesApp.Test/`

  conterranno documenti dedicati (ad es. `Architecture.md`, `Models.md`, `ApiDesign.md`, `TestingStrategy.md`, ecc.) in cui verranno descritte **in dettaglio** le singole parti del sistema, con un taglio sia tecnico sia didattico.

---

## 12. Convenzioni di Codice C# adottate nel progetto

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

* **coerente** tra progetto principale (`JokesApp.Server`) e test (`JokesApp.Test`),
* **auto-esplicativa**, così da permettere di individuare rapidamente dove si trova una funzionalità o il relativo test.

Esempio di struttura logica del progetto server:

```text
JokesApp.Server/
 ├─ Models/                  // Modelli Entity / dominio
 │    ├─ Joke.cs
 │    └─ ApplicationUser.cs
 ├─ DTOs/                    // Data Transfer Objects (input/output API)
 ├─ Services/                // Business logic, servizi applicativi
 ├─ Controllers/             // API endpoints (Web API)
 └─ Data/                    // DbContext e accesso al database
```

Il progetto di test segue una struttura a **mirror**, in modo che ogni componente del codice di produzione abbia idealmente una controparte di test in una posizione prevedibile:

```text
JokesApp.Test/
 ├─ Models/                  // Test per i modelli di dominio
 ├─ DTOs/                    // Test per i DTO e le validazioni
 ├─ Services/                // Test per la business logic
 └─ Controllers/             // (eventuali) test sui controller / API
```

#### Namespace

I namespace seguono la gerarchia delle cartelle e iniziano con il nome del progetto radice.

Esempi:

```csharp
namespace JokesApp.Server.Models
{
    public class Joke { }
}

namespace JokesApp.Test.Models
{
    public class JokeTests { }
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

