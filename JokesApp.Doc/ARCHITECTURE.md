# 📘 **Architettura del Progetto: Linee Guida Teoriche e Strutturazione dei Layer**

## 1. Introduzione generale

Lo sviluppo di applicazioni moderne richiede l’adozione di modelli architetturali chiari, scalabili e in grado di mantenere la qualità del software anche in presenza di modifiche frequenti.
Nel presente progetto didattico è stato scelto di adottare una combinazione consolidata e ampiamente utilizzata nel mondo enterprise:

* **Clean Architecture** come struttura a layer indipendenti.
* **Domain-Driven Design (DDD)** per la modellazione della logica di dominio.
* **Hexagonal Architecture (Ports & Adapters)** per isolare il dominio dal mondo esterno.
* Principi **SOLID**, **DRY**, **KISS**, **YAGNI** come linee guida di progettazione.
* **CQRS leggero** per distinguere le operazioni di comando da quelle di lettura.

L’obiettivo è ottenere un’architettura robusta, estensibile, ben testabile e allineata agli standard progettuali richiesti nell’industria software contemporanea.

---

## 2. Stack tecnologico

Lo stack tecnico definisce *gli strumenti*, non la loro organizzazione architetturale.
Per il presente progetto:

* **Backend:** ASP.NET Core
* **Frontend:** React
* **Database:** PostgreSQL / SQL Server tramite *Entity Framework Core*
* **Template base:** “ASP.NET + React” fornito da Visual Studio

Questo stack è compatibile con un’architettura a strati pulita e consente di separare efficacemente presentation, business logic e Persistent Storage.

---

## 3. Paradigma architetturale adottato

### 3.1 Significato di paradigma architetturale

Uno *stile architetturale* definisce:

* la **distribuzione delle responsabilità** tra i vari strati,
* il **flusso dei dati** e delle dipendenze,
* la **modalità di evoluzione del sistema**,
* l’isolamento tra logica di business e dettagli tecnologici.

Non riguarda dunque le singole classi o i pattern GoF, ma l’organizzazione concettuale alla base del backend.

### 3.2 Scelte progettuali

| Scelta                      | Stato | Motivazione                                                                 |
| --------------------------- | ----- | --------------------------------------------------------------------------- |
| **Clean Architecture**      | ✔️    | Offre separazione chiara tra domain, application e infrastruttura           |
| **Domain-Driven Design**    | ✔️    | Adatto per modellare la logica del dominio in modo rigoroso                 |
| **Hexagonal Architecture**  | ✔️    | Implementazione concreta della Clean in termini di Ports & Adapters         |
| **SOLID, DRY, KISS, YAGNI** | ✔️    | Migliorano qualità, leggibilità e manutenibilità                            |
| **CQRS leggero**            | ✔️    | Semplifica separazione command/query senza introdurre complessità eccessiva |

---

## 4. La struttura architetturale finale

L’architettura completa adottata è composta da **sei macro-layer**, tipici dei sistemi enterprise:

1. **Domain Layer**
2. **Application Layer**
3. **Infrastructure Layer**
4. **API / Presentation Layer**
5. **Cross-Cutting Layer**
6. **Testing Layer**

Le dipendenze sono *unidirezionali* e vanno dall’esterno verso l’interno:

```
API → Application → Domain
Infrastructure ↗︎  Application
```

---

## 5. Descrizione dei Layer

---

### 🟦 5.1 Domain Layer — Il nucleo del sistema

Il **Domain Layer** rappresenta la parte più stabile e duratura dell’applicazione.
In esso risiede la logica di business pura, non influenzata da tecnologie esterne o infrastrutture.

#### Responsabilità principali

* Definizione delle **regole di dominio**
* Modellazione di **Entità**, **Value Objects** e **Aggregate Roots**
* Gestione degli **invarianti** del sistema
* Pubblicazione di **Domain Events**
* Validazioni profonde
* Eccezioni specifiche di dominio

#### Contenuto

* **Entities** (es. `Joke`, `User`, ...)
* **Value Objects** (es. `Email`, `UserId`, ...)
* **Domain Events** (es. `JokeCreatedEvent`)
* **Domain Services**
* **Domain Exceptions**

#### Elementi da escludere rigorosamente

❌ Database
❌ EF Core
❌ Logging
❌ Controller
❌ HTTP, Serializzazione
❌ Repository concreti

#### Principio guida

> Il Dominio non deve conoscere nulla del mondo esterno.
> È *eterno*, stabile e immune ai cambiamenti tecnologici.

---

### 🟩 5.2 Application Layer — L’orchestratore dei casi d’uso

L’**Application Layer** rappresenta il livello operativo che coordina:

* il dominio,
* i repository,
* la gestione dei comandi,
* le query,
* le transazioni,
* il dispatch degli eventi.

Contiene la logica applicativa, non la logica di business.

#### Responsabilità

* Implementare **casi d’uso** (Use Cases)
* Coordinare entità e servizi del dominio
* Eseguire validazioni superficiali (pre-condition)
* Gestire **Command Handler** e **Query Handler** (CQRS)
* Comunicare con infrastruttura e dominio tramite **porte** (Ports)
* Dispatch degli eventi di dominio al termine delle transazioni

#### Contenuto

* **CommandHandler / QueryHandler**
* **Application Services**
* **Interfacce dei Repository** (Ports)
* **Event Dispatcher**
* **DTO applicativi** (non API)

#### Da escludere

❌ EF Core
❌ SQL
❌ Logica di dominio profonda
❌ HTTP / Controller

#### Relazione con Hexagonal Architecture

L’application layer costituisce le **Port** dell’architettura esagonale, mentre l’infrastruttura implementa gli **Adapter**.

---

### 🟧 5.3 Infrastructure Layer — Implementazione tecnica

Qui vive tutto ciò che è tecnologicamente concreto o dipendente da strumenti esterni.

#### Responsabilità

* Accesso ai dati (repository)
* Implementazione delle Ports dell’application layer
* Integrazione con servizi esterni (API, email, cloud)
* Logging, filesystem, rete
* Mapping EF Core

#### Contenuto

* `DbContext`
* Repository concreti
* Adapters (HTTP client, SMTP client, ...)
* Configurazioni Fornite dalla piattaforma
* Conversioni e mapping dati

#### Da escludere

❌ Regole del dominio
❌ Logica applicativa

---

### 🟥 5.4 Presentation Layer / API — L’interfaccia verso l’utente

Espone e gestisce il livello HTTP/API.

#### Responsabilità

* Routing e Controller
* Validazione input tramite DTO
* Conversione DTO → Command/Query
* Autenticazione e autorizzazione
* Restituzione delle risposte HTTP

#### Contenuto

* Controller ASP.NET
* Request/Response DTO
* Filtri, middleware
* Mapping API → Application

#### Da escludere

❌ Logica di business
❌ Accesso al DB
❌ Use cases interni

---

### 🟪 5.5 Cross-Cutting Layer — Componenti trasversali

Gestisce le funzionalità che permeano più layer.

#### Esempi

* Logging (Serilog)
* Middleware globali
* Dependency Injection
* Configurazioni
* Caching
* Rate limiting
* Gestione eccezioni

---

### 🟫 5.6 Testing Layer — Verifica della qualità

L’architettura pulita rende i test estremamente semplici grazie all’isolamento dei layer.

#### Tipologie di test

1. **Unit Test di Dominio**
   Testano enti, VO e logica di business *senza* DB.

2. **Unit Test di Application**
   Testano use case isolati da infrastruttura reale.

3. **Integration Test**
   Verificano repository, EF Core, WebApplicationFactory.

4. **End-to-End**
   Dalla richiesta HTTP al database e ritorno.

---

## 6. Riepilogo sintetico dei livelli (tabella accademica)

| Layer              | Responsabilità                 | Contenuto                      | Deve escludere           |
| ------------------ | ------------------------------ | ------------------------------ | ------------------------ |
| **Domain**         | Logica di business, invarianti | Entity, VO, Events, Exceptions | SQL, EF, API             |
| **Application**    | Casi d’uso, orchestrazione     | UseCase, Ports, Handlers       | Logica business profonda |
| **Infrastructure** | Accesso dati, servizi tecnici  | EF, Repo concreti, HTTP client | Regole di dominio        |
| **API**            | Interazioni HTTP, input/output | Controller, DTO                | Business logic           |
| **Cross-Cutting**  | Logging, config, middleware    | Pipeline, DI                   | Regole di dominio        |
| **Testing**        | Validazione sistema            | Unit, Integration              | —                        |

---

## 7. Conclusione

La combinazione di Clean Architecture, DDD e Hexagonal Architecture fornisce un modello estremamente robusto per organizzare l’applicazione, rendendola:

* facilmente estendibile,
* resistente ai cambiamenti tecnologici,
* testabile,
* leggibile,
* orientata al dominio.

Questa struttura è utilizzata da grandi aziende come Microsoft, Amazon, Netflix e Shopify, e rappresenta una base solida sia per progetti didattici sia per applicazioni enterprise di larga scala.

---

# 🧩 **Come applicare i GoF Patterns nel tuo progetto Clean + DDD + Hexagonal**

I **Design Patterns GoF** non sostituiscono Clean Architecture, DDD o Hexagonal Architecture:
👉 **li completano**.
👉 **vivono dentro i layer giusti**, migliorandone la qualità strutturale.
👉 **non vanno applicati per moda**, ma quando risolvono problemi concreti.

I documenti che hai scritto descrivono un progetto composto da *Domain, Application, Infrastructure, API, Cross-Cutting* .
Ogni pattern GoF si applica **solo in alcuni layer**, e soprattutto **solo quando necessario**.

---

## 1. 📌 Dove si applicano i GoF nei tuoi layer

| Pattern Area GoF                                                                     | Layer corretto              | Perché                                                                |
| ------------------------------------------------------------------------------------ | --------------------------- | --------------------------------------------------------------------- |
| **Creazionali** (Factory, Builder, Singleton)                                        | Domain, Application         | Creazione controllata di oggetti che devono rispettare invarianti DDD |
| **Strutturali** (Adapter, Facade, Composite, Proxy, Decorator)                       | Application, Infrastructure | Perfetti nella Port/Adapter Architecture e integrazioni esterne       |
| **Comportamentali** (Observer, Mediator, Strategy, Command, Chain of Responsibility) | Domain, Application         | Ideali per domain events, orchestrazione e use case                   |

Questa tabella rispecchia la tua architettura documentata nei file  .

---

## 2. 📘 Pattern GOF e la tua architettura (uno per uno)

### 2.1 Creational Patterns (per la creazione controllata nel Domain)

#### **Factory / Factory Method — *Consigliatissimo per il tuo Dominio***

Nel tuo dominio hai:

* Value Objects (`JokeId`, `AnswerText`, `QuestionText`)
* Aggregate roots (`Joke`, `ApplicationUser`)
* Domain Events (`JokeWasCreated`, ecc.)

Questi elementi devono rispettare **invarianti e regole di validazione** definite nel Domain Layer (documentate nei tuoi file) .

💡 **Applicazione consigliata:**

* Crea **static factories** per impedire stati incoerenti.
* Esempio: `Joke.Create(questionText, answerText, userId)` produce l’oggetto già in uno “stato valido” e pubblica l’evento `JokeWasCreated`.

#### **Builder**

Utile quando una Entity complessa richiede molti parametri opzionali.

Nel tuo dominio:
→ può essere utile per costruire oggetti `ApplicationUser` con molte proprietà e validazioni.

---

### 2.2 Structural Patterns (perfetti per la tua Hexagonal Architecture)

#### **Adapter — *Il pattern più importante nel tuo progetto***

Il tuo file *ARCHITECTURE.md* descrive chiaramente l’uso dell’architettura esagonale (Ports & Adapters) .

I repository concreti in Infrastructure (es. EF Core) **sono Adapter**:

```
Application Layer → IRepo (Port)
Infrastructure → RepoEFCore (Adapter)
```

🔧 Il pattern GoF “Adapter” formalizza esattamente questo concetto.

#### **Facade**

Puoi usarlo:

* per racchiudere complessità di chiamate multiple ai repository,
* per semplificare l'accesso da parte dell’Application Layer.

Esempio:
`JokesFacade` può incapsulare operazioni complesse come *crea joke*, *notifica frontend*, *registra evento*.

#### **Decorator**

Perfetto per:

* logging,
* caching,
* cross-cutting concerns.

Potresti implementarlo per avvolgere i repository con log automatico degli accessi, integrandosi bene col tuo Eventing.

---

### 2.3 Behavioral Patterns (fondamentali nel Domain + Application)

#### **Observer — Già presente nei tuoi Domain Events**

Hai già implementato:

* `IDomainEvent`
* `DomainEvent`
* `JokeWasCreated`, `JokeWasLiked`, ecc.

Questo è esattamente **l’Observer pattern**, applicato in chiave DDD.
L’Application Layer sarà l’**Event Dispatcher** che notificherà i listener.

#### **Command — Già presente nel tuo CQRS leggero**

Nel tuo *README* descrivi l’idea di:

* Command
* Query
* Handlers

Questo è letteralmente il GoF **Command Pattern**.
In Clean Architecture + CQRS:

➡ il “Command Handler” **è** il Command pattern.

#### **Strategy — Perfetto per logiche variabili**

Esempi:

* diverse strategie di validazione,
* diverse modalità di sorting o filtraggio di jokes,
* plugin per generazione notifiche.

#### **Chain of Responsibility**

Utile per pipeline di validazione o autorizzazione.

Nel tuo progetto può funzionare nel Presentation Layer:

```
Input → [Validation Handler] → [Authorization Handler] → [Business Rules Handler]
```

---

## 3. 🧱 Come integrare i pattern GoF nel tuo progetto *step-by-step*

### **Step 1 — Rafforza il Domain con Factory + Observer**

Per ogni entità:

1. Usa Factory per creare oggetti validi.
2. Solleva eventi di dominio.
3. Aggiungi test (conforme al tuo sistema di test documentato).

Questo mantiene il Domain puro, coerente e indipendente dalla tecnologia.

---

### **Step 2 — Struttura l’Application Layer con Command + Mediator**

Se decidi di usare MediatR o un Dispatcher manuale:

* ogni caso d’uso diventa un **Command Handler**
* il Dispatcher (Mediator) coordina flow e eventi

Questo segue quanto descritto nella tua architettura .

---

### **Step 3 — Implementa Adapter nei repository Infrastructure**

I tuoi repository concreti devono:

* implementare le interfacce definite in Application (Ports)
* convertire Value Objects ↔ Entity Framework (tramite i Converter che già hai)
* loggare gli eventi tecnici (Decorator opzionale)

---

### **Step 4 — Applicare Decorator / Proxy al logging tecnico**

Hai indicato nella road map:

* log funzionali,
* log tecnici,
* audit trail,
* eventi live al frontend.

Puoi farlo così:

```
IRepository
↑
RepoLoggingDecorator (GoF Decorator)
↑
RepoEFCore (Adapter)
```

---

### **Step 5 — Applicare Facade nell’orchestrazione complessa**

Suggerito per future feature come:

* notifiche push SignalR,
* broadcast di eventi al frontend,
* pipeline di approvazione dei contenuti.

Una *JokesDomainService* o *ApplicationService* può fungere da Facade semplificata.

---

## 4. 🧩 Ricapitolazione finale — Pattern consigliati per ogni subsystem

| Subsystem                | Pattern GoF ideale                                   | Perché                                                     |
| ------------------------ | ---------------------------------------------------- | ---------------------------------------------------------- |
| **Domain Layer**         | Factory, Builder, Observer                           | Garantire invarianti, eventi di dominio e creazione sicura |
| **Application Layer**    | Command, Mediator, Strategy, Chain of Responsibility | Gestione dei casi d’uso e orchestrazione                   |
| **Infrastructure Layer** | Adapter, Decorator, Proxy, Facade                    | Ports & Adapters, logging, integrazioni tecniche           |
| **API Layer**            | Facade (per orchestrare), CoR (per validazioni)      | Semplificare input/output HTTP                             |
| **Cross-Cutting**        | Decorator, Proxy                                     | logging, caching, auditing                                 |

---

## 5. 🎯 Conclusione: come integrarli con la tua documentazione

I pattern GoF non vanno documentati come moduli separati, ma come **strumenti integrativi** all’interno dei layer già definiti nei file:

* *README* (overview architetturale) 
* *ARCHITECTURE.md* (layer dettagliati, DDD e Hexagonal) 

👉 Devi aggiungere una nuova sezione **“Design Patterns Adopted”** dentro *JokesApp.Doc/JokesApp.Server/Architecture.md* che spiega:

* quali pattern usi,
* in quale layer si trovano,
* a quale responsabilità architetturale rispondono,
* perché sono stati scelti.

---
