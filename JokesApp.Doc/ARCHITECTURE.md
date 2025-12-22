# 📘 Architettura del Progetto - React Frontend + ASP.NET Core Web API (Clean Architecture + DDD + Hexagonal)

Lo sviluppo di applicazioni moderne richiede l’adozione di modelli architetturali chiari, scalabili e in grado di mantenere la qualità del software anche in presenza di modifiche frequenti. Nel presente progetto didattico è stato scelto di adottare una combinazione consolidata e ampiamente utilizzata nel mondo enterprise.

Questo documento è la **fonte di verità architetturale** del progetto: descrive **confini, responsabilità, dipendenze** e lo stato **AS-IS / TO-BE**
- **AS-IS**: ciò che esiste oggi nel repository.
- **TO-BE**: direzione di evoluzione dichiarata (senza spacciare il futuro per presente).

- Entry-point documentazione: `JokesApp.Doc/README.md`
- Entry-point frontend (UI/routing/services): `JokesApp.Doc/JokesApp.Client/README.md`
- Workflow repo: `JokesApp.Doc/WORKFLOW.md`
- Stato operativo: `JokesApp.Doc/toDo.md`
- Timeline (Server): `JokesApp.Doc/JokesApp.Server/TIMELINE.md`

---

## 0. Stack tecnologico
Lo stack tecnico definisce *gli strumenti*, non la loro organizzazione architetturale.
Per il presente progetto:

* **Backend:** ASP.NET Core
* **Frontend:** React
* **Database:** PostgreSQL tramite *Entity Framework Core*
* **Template base:** “ASP.NET + React” fornito da Visual Studio

Questo stack è compatibile con un’architettura a strati pulita e consente di separare efficacemente presentation, business logic e Persistent Storage.

---

## 1. Paradigma architetturale adottato

### 1.1 Significato di paradigma architetturale

Uno *stile architetturale* definisce:

* la **distribuzione delle responsabilità** tra i vari strati,
* il **flusso dei dati** e delle dipendenze,
* la **modalità di evoluzione del sistema**,
* l’isolamento tra logica di business e dettagli tecnologici.

### 1.2 Scelte progettuali

| Scelta                      | Stato     | Motivazione                                                                 |
| --------------------------- | --------- | --------------------------------------------------------------------------- |
| **Clean Architecture**      | AS-IS     | Separazione a layer + dependency rule verso l’interno (dominio)             |
| **Domain-Driven Design**    | AS-IS     | Dominio modellato con Entities/VO/Eventi/Exceptions e invarianti            |
| **Hexagonal Architecture**  | Parziale  | Adottata come *mental model*; Ports & Adapters formalizzati (Handlers/Ports) sono TO-BE |
| **SOLID, DRY, KISS, YAGNI** | AS-IS     | Linee guida applicate per mantenere codice e dominio puliti e manutenibili  |
| **CQRS leggero**            | TO-BE     | Separazione Command/Query prevista nell’Application Layer (Use Cases/Handlers) |


### 1.3 La struttura architetturale finale

L’architettura completa adottata è composta da **sei macro-layer**, tipici dei sistemi enterprise:

1. **Domain Layer**
2. **Application Layer**
3. **Infrastructure Layer**
4. **API / Presentation Layer**
5. **Cross-Cutting Layer**
6. **Testing Layer**

---

## 2. Introduzione: Visione Architetturale Complessiva (VAC)

### 2.1 Generale: 
Il progetto **JokesApp** adotta un’architettura moderna, modulare e scalabile, basata su:

* **Frontend React** sviluppato come **Single Page Application (SPA)**
* **Backend ASP.NET Core Web API** progettato secondo i paradigmi:

  * **Clean Architecture** come struttura a layer indipendenti.
  * **Domain-Driven Design (DDD)** per la modellazione della logica di dominio.
  * **Hexagonal Architecture (Ports & Adapters)** per isolare il dominio dal mondo esterno.
  * Principi **SOLID**, **DRY**, **KISS**, **YAGNI** come linee guida di progettazione.
  * **CQRS leggero** per distinguere le operazioni di comando da quelle di lettura.

L’obiettivo è ottenere un’architettura robusta, estensibile, ben testabile e allineata agli standard progettuali richiesti nell’industria software contemporanea.

Questa combinazione offre:
* una **separazione netta** tra logica di presentazione e logica applicativa,
* alta **manutenibilità**, **testabilità** e **scalabilità**,
* possibilità di evolvere frontend e backend in modo indipendente,
* struttura robusta e adatta a scenari enterprise.

JokesApp è un **monorepo** composto da:
- **Client**: React SPA (`JokesApp.Client`)
- **Server**: ASP.NET Core Web API (`JokesApp.Server`)
- **Tests**: test automatici (`JokesApp.Tests`)
- **Doc**: documentazione (`JokesApp.Doc`)

### 2.2 Architettura Backend
Il backend segue un’impostazione **Clean Architecture + DDD + Ports & Adapters**.

Obiettivi principali del backend:
- Implementa logica applicativa e di dominio,
- Fornisce esclusivamente API REST in formato JSON,
- È strutturato secondo Clean Architecture.
- Mantiene il **dominio indipendente** da HTTP/DB/framework (Clean Architecture + DDD “pragmatico”).
- Rende espliciti confini e responsabilità (Ports & Adapters / Hexagonal come mental model).
- Garantisce che la *codebase* risulti **testabile** (dominio testabile senza DB) e **manutenibile**.

> Nota: “Hexagonal / Ports & Adapters” e “Clean Architecture” descrivono la stessa idea chiave:  
> *il dominio e i casi d’uso non devono dipendere dai dettagli esterni.*

### 2.3 Architettura Frontend (AS-IS / TO-BE)
**AS-IS:** il frontend è presente come scaffold/template (React + Vite), ma non è ancora evoluto né validato architetturalmente.  
**TO-BE:** React SPA con routing client-side e livello `services` per chiamate HTTP/JSON verso la Web API (nessuna logica di dominio lato client).

---

## 3. Principi guida (regole non negoziabili)

L’applicazione segue un modello **API-Driven** via HTTP/JSON, con due componenti chiaramente distinti:

```
[ React SPA ]  <—HTTP/JSON—>  [ ASP.NET Core Web API ]  <—EF Core—>  [ Database ]
```

- Il **Client** gestisce UI, routing e chiamate HTTP.
- Il **Server** espone endpoint REST e contiene dominio + accesso dati.
- Il **Database** è gestito via EF Core (migrations incluse).

### 3.1 Dependency Rule (Clean Architecture)
Le dipendenze devono puntare **verso l’interno (il dominio)**: il dominio non conosce dettagli infrastrutturali.

Schema concettuale:

```text
Presentation (HTTP/API)
        ↓
Application (Use Cases, Ports)
        ↓
      Domain (Core)
        ↑
Infrastructure (EF Core, external services) — implementa Ports
```

Le dipendenze sono *unidirezionali* e vanno dall’esterno verso l’interno:

```text
API → Application → Domain
Infrastructure ↗︎  Application
```

### 3.2 Dominio “puro” (DDD pragmatico)
Nel **Domain** vivono:
- invarianti,
- Entities / Aggregate Root,
- Value Objects,
- Domain Events,
- Domain Exceptions e messaggistica di errore di dominio.

Il dominio **non** deve contenere logica HTTP/Controller né concetti di persistenza.

### 3.3 Contratti esterni tramite DTO
L’API parla tramite **DTO**: non si espongono direttamente Entities/Value Objects oltre il boundary HTTP.

### 3.4 CQRS leggero (quando serve)
Separazione concettuale tra:
- **Commands** (modificano stato),
- **Queries** (leggono stato),
senza introdurre complessità infrastrutturale (bus, event sourcing, ecc.) se non necessaria.

---

## 4. AS-IS vs TO-BE (stato reale)

### AS-IS (oggi nel repo)
- **Domain**: presente e consistente (Entities, Value Objects, Domain Events, Exceptions).
- **Presentation/API**: presente (Controllers; al momento incluso controller template) + contratti esterni (DTOs).
- **Data/Infrastructure (persistenza)**: presente tramite EF Core (`Data/`, `Migrations/`, converters).
- **Application layer**: **non ancora separato come layer dedicato**; parte dell’orchestrazione è ancora in evoluzione.

### TO-BE (obiettivo prossimo)

#### Application Layer (TO-BE)
- Introdurre un vero **Application Layer** con:
  - Use Cases (Command/Query) in stile CQRS leggero,
  - **Ports** (interfacce) verso persistenza/servizi esterni,
  - coordinamento transazioni e dispatch eventi,
  - mapping API → Use Case (senza logica di business nei controller).
- Spostare la logica “procedurale” fuori dai Controller (quando cresceranno gli use case).

L’**Application Layer** sarà l’orchestratore dei casi d’uso: coordina dominio e persistenza senza introdurre logica di business “profonda”.

**Responsabilità (TO-BE)**
- implementare i **Use Case** (Command/Query in stile CQRS leggero),
- coordinare entità e servizi di dominio,
- gestire pre-condizioni e validazioni “superficiali”,
- definire **Ports** (interfacce) verso persistenza/servizi esterni,
- coordinare transazioni e **dispatch** dei Domain Events a fine operazione.

**Contenuti tipici (TO-BE)**
- Command/Query Handlers
- Application Services
- Repository interfaces (Ports)
- Event dispatcher
- DTO applicativi (non HTTP)

**Da escludere**
- EF Core / SQL
- HTTP / Controller
- dipendenze infrastrutturali

---

## 5. Mappatura layer ↔ cartelle (backend)

```text
JokesApp.Server/
├─ Controllers/                # Presentation (HTTP endpoints)
├─ DTOs/                       # API contracts (request/response)
├─ Domain/                     # Domain layer (DDD)
│  ├─ Entities/
│  ├─ ValueObjects/
│  ├─ Events/
│  ├─ Exceptions/
│  ├─ Errors/
│  └─ Primitives/
├─ Data/                       # Infrastructure (EF Core)
│  ├─ JokesDbContext.cs
│  ├─ Converters/              # EF Core ValueConverters (VO mapping)
│  └─ Errors/                  # StartupErrorMessages.cs
├─ Migrations/                 # EF Core migrations
├─ Validation/                 # Validation attribute a supporto del boundary HTTP
├─ Program.cs                  # Bootstrap + DI + middleware pipeline
├─ appsettings*.json           # Configurazione runtime
└─ .env                        # Variabili d’ambiente (dev)
```

> Nota: eventuali cartelle come `Models/` o `Server_Backup/` non rappresentano un layer architetturale (AS-IS), ma contenuti transitori/di supporto.

---

## 6. Domain Layer (il core)

### 6.1 Responsabilità
Nel Domain vivono:
- **invarianti** (regole che devono sempre essere vere),
- modellazione tramite **Entities** e **Value Objects**,
- **Domain Events** (fatti rilevanti del dominio),
- **Domain Exceptions** (violazioni delle regole),
- primitive comuni (es. `AggregateRoot`).

### 6.2 Regole chiave
- Il Domain **non dipende** da:
  - HTTP / Controller / DTO,
  - EF Core / DbContext / Migrations,
  - librerie infrastrutturali.
- La validazione “seria” (invarianti) sta nel Domain:
  - Value Object auto-validanti,
  - metodi di dominio che proteggono lo stato,
  - eccezioni tipizzate.

### 6.3 Esempi (pattern effettivamente usati)
- Value Object con factory + invarianti (es. lunghezza massima, non vuoto).
- Entity che genera Domain Events su operazioni significative.
- Guard/validazioni e eccezioni di dominio per stati illegali.

### 6.4 Value Objects auto-validanti (AS-IS)

I Value Objects incapsulano validazioni e normalizzazione (es. trimming, max length, formato). In caso di input non valido, il Domain solleva `DomainValidationException`. 

### 6.5 Aggregate Root + Domain Events (AS-IS / TO-BE)

**AS-IS:** gli aggregate producono eventi (es. `JokeWasCreated`, `JokeWasLiked`) tramite una coda interna gestita dall’`AggregateRoot`. 
**TO-BE:** dispatch strutturato nel layer Application dopo persistenza/commit transazionale.

### 6.6 Error handling (AS-IS / TO-BE)

* **Domain (AS-IS):** eccezioni tipizzate (validation/operation/authorization). 
* **Presentation (TO-BE):** trasformazione coerente in risposte HTTP (400/403/409/500) tramite middleware/filter globale (quando consolidato).

---

## 7. Presentation/API (HTTP)

### 7.1 Responsabilità
Il livello API deve occuparsi solo di:
- ricevere input (DTO),
- validazioni “di frontiera” (formato, required, ecc.),
- trasformare input in chiamate applicative (oggi: ancora in evoluzione),
- restituire output (DTO) e codici HTTP.

### 7.2 Regola
Mai esporre direttamente entità/value object del dominio come contratto esterno:
- verso l’esterno usare **DTOs** stabili.

### 7.3 Flussi esemplificativi (AS-IS / TO-BE)

**Creazione “Joke” (happy path)**

1. Client → richiesta HTTP (DTO).
2. Controller: validazioni “di frontiera”.
3. Domain: costruzione con VO validi + generazione `JokeWasCreated`. 
4. EF Core: persistenza.
5. (TO-BE) Application Layer: estrazione e dispatch eventi dopo commit.

**Update / Like / Unlike (behavior-driven)**
Le operazioni sono metodi dell’entità: aggiornano lo stato solo se le precondizioni sono rispettate, generano eventi coerenti e sollevano eccezioni di dominio su violazioni. 

---

## 8. Data / Infrastructure (persistenza EF Core)

### 8.1 Responsabilità
Qui vive ciò che è “volatile” e dipendente dalla tecnologia:
- `DbContext`,
- mapping e conversioni (ValueConverters),
- migrazioni,
- (futuro) repository concreti che implementano Ports applicative.

### 8.2 Value Objects + EF Core
I Value Objects vengono persistiti tramite conversioni dedicate, mantenendo il dominio pulito e coerente.

### 8.3 Bootstrap e concern trasversali (Cross-Cutting)

> Nota: il Cross-Cutting è trasversale ai layer; qui è descritto vicino a `Program.cs` per praticità operativa.

In questa area ricadono i concern trasversali che toccano più layer, tipicamente configurati nel bootstrap dell’app (es. `Program.cs`):

- Dependency Injection (registrazione servizi e componenti)
- Middleware pipeline (gestione errori, CORS, sicurezza, ecc.)
- Configurazioni (appsettings + variabili d’ambiente)
- Logging (come concern trasversale, senza impattare il Domain)
- Policy e aspetti operativi (rate limiting/caching quando introdotti)

> Regola: il Domain non deve dipendere da implementazioni cross-cutting; al massimo può esporre segnali (eventi/eccezioni) che vengono gestiti all’esterno.

---

## 9. Testing

Il progetto `JokesApp.Tests` valida:
- invarianti di dominio e comportamento (Unit),
- (futuro) integrazione con EF Core / API (Integration),
- regressioni e casi limite.

Obiettivo: usare il Domain come “core testabile” senza dover avviare HTTP o DB per ogni test.

---

## 10. Principi non negoziabili (regole operative)

- **Dominio prima di tutto**: invarianti nel Domain.
- **Confini netti**: DTO per l’esterno, Domain per regole e integrità.
- **Niente logica di business nei controller** (target TO-BE).
- **Truth-first**: questo documento descrive ciò che esiste e ciò che è pianificato, senza vendere “TO-BE” come già implementato.

---

## 11. Riepilogo sintetico dei layer (reference)

| Layer              | Responsabilità                 | Contenuto                      | Deve escludere           |
| ------------------ | ------------------------------ | ------------------------------ | ------------------------ |
| **Domain**         | Logica di business, invarianti | Entity, VO, Events, Exceptions | SQL, EF, API             |
| **Application**    | Casi d’uso, orchestrazione     | UseCase, Ports, Handlers       | Logica business profonda |
| **Infrastructure** | Accesso dati, servizi tecnici  | EF Core, DbContext, converters, migrations, (TO-BE) repository concreti/adapter     | Regole di dominio        |
| **API**            | Interazioni HTTP, input/output | Controller, DTO                | Business logic           |
| **Cross-Cutting**  | Logging, config, middleware    | Pipeline, DI                   | Regole di dominio        |
| **Testing**        | Validazione sistema            | Unit, Integration              | —                        |

---

## 12. Conclusione

La combinazione di Clean Architecture, DDD e Ports & Adapters fornisce una struttura chiara per separare **regole di dominio**, **casi d’uso**, **dettagli tecnici** e **boundary HTTP**.

In JokesApp il Domain è già un nucleo coerente e testabile; l’evoluzione principale (TO-BE) è consolidare l’Application Layer per spostare l’orchestrazione fuori dai controller e rendere ancora più netti i confini tra livelli.

---
