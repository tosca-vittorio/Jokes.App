# JokesApp.Doc/JokesApp.Server/TIMELINE.md

## Flow Update pratico (come usarli mentre sviluppi)

* **TIMELINE.md = ordine di sviluppo** (cosa fare prima/dopo) e stato globale per step. 
* **toDo.md = cruscotto operativo** (cosa esiste / cosa manca / cosa è stato chiuso), anche non ordinato. 

### 1) Parti sempre dalla TIMELINE

* Scegli **il primo step non chiuso** (🟡 o ⬜) dall’alto verso il basso. 
* Quello è il tuo “blocco di lavoro” della sessione.

### 2) Esegui il blocco fino a “Definition of Done”

Per considerare un punto “chiuso” (✅), la regola è:

* codice implementato + verificato
* documentazione allineata
* build/run ok 
* almeno i test minimi coerenti con lo step

### 3) Aggiorna DOPO il TODO (non prima)

* Nel TODO:

  * sposti voci da 🟡→✅ quando le hai davvero verificate/chiuse 
  * aggiungi nuove task scoperte “per non perderle”, anche se non sono in ordine.

### 4) Chiudi anche la TIMELINE

Quando uno step è concluso:

* in TIMELINE cambi 🟡/⬜ → ✅ sullo step (e sulle sotto-voci). 

---

## 📌 TIMELINE DEL SERVER (sequenza completa aggiornata A1 → 10)

### Legenda stati
- ✅ = completato e verificato
- 🟡 = presente ma da verificare/chiudere (parziale)
- ⬜ = da fare

```md
A1 - Setup DB locale (PostgreSQL) ✅
      ├─ Verifica client `psql` + servizio attivo ✅
      ├─ Creazione DB `jokes` + ruolo `jokes_migrator` ✅
      ├─ Ownership + privilegi su schema `public` ✅
      ├─ Verifica permessi via psql (CREATE/DROP test) ✅
      ├─ .env locale (gitignored) ✅
      └─ appsettings.json (fallback non sensibile) ✅

      │ │ │
      ▼ ▼ ▼

A2 - Bootstrap backend (config + connessione DB SENZA EF) ✅
      ├─ Program.cs: Caricamento `.env` (DotNetEnv) + env vars in IConfiguration (DEV-only .env) ✅
      ├─ Risoluzione connection string (priorità esplicita) ✅
      │    ├─ A) Env var: `ConnectionStrings__JokesDb` (preferita) ✅
      │    ├─ B) Fallback config: `ConnectionStrings:JokesDb` (placeholder non sensibile) ✅
      │    └─ C) Composizione da `DB_*` (Host/Port/Name/User/Password) ✅
      ├─ Fail-fast: se config DB manca/placeholder → errore chiaro in startup ✅
      ├─ Logging startup: tracciamento source connessione + mascheramento password ✅
      ├─ Servizi minimi: AddControllers/MapControllers possono essere registrati senza introdurre Controllers reali; in A2 si espongono solo endpoint tecnici (health/ping) via Minimal API. ✅
      ├─ Pipeline per ambienti (DEV vs NON-DEV) ✅
      │    ├─ DEV: OpenAPI (endpoint spec) solo Development ✅
      │    ├─ DEV: DeveloperExceptionPage ✅
      │    └─ NON-DEV: exception handling + security baseline (UseExceptionHandler, HSTS) ✅
      ├─ DEV: Preflight DB `GET /api/db/ping` (Npgsql, `SELECT 1`) ✅
      └─ Health endpoints (safe, anche fuori da Development) ✅
           ├─ `GET /health` (liveness: processo vivo) ✅
           └─ `GET /health/ready` (readiness DB: Npgsql `SELECT 1`, senza EF) ✅

A2a - launchSettings.json (profilo DEV/PROD configurato) ✅
      ├─ Profilo `server-dev` con configurazione DEV (HTTP/HTTPS, Debug/Logging, SPA proxy) ✅
      └─ Profilo `server-prod` con configurazione PROD (solo HTTP, senza SPA proxy) ✅
      
A2b - Program.cs: finalizzazione e chiusura (per completamento) ✅
      ├─ Finalizzare `Program.cs` per concludere il blocco A2b. ✅
      ├─ **Test degli endpoint**: 
      │    ├─ **GET /health** (liveness: processo vivo) → Testato con PowerShell (`Invoke-RestMethod` per verificare risposta 200 OK). ✅
      │    ├─ **GET /health/ready** (readiness DB: `SELECT 1` via Npgsql) → Testato con PowerShell (comando `Invoke-RestMethod` per confermare corretto stato del DB). ✅
      │    └─ **GET /api/db/ping** → Testato in ambiente **Development** tramite PowerShell per confermare che restituisse `SELECT 1`. ✅
      └─ **Nota**: A2b copre la finalizzazione di Program.cs, e i test degli endpoint sono stati eseguiti come parte di questa fase per verificare il funzionamento.

 > Nota: in A2 il file .env viene caricato solo in Development (local-first).
 > Nota: in ambienti NON-DEV la reachability DB è verificata tramite /health/ready (output minimale).
 > Nota: in A2 si espongono solo endpoint tecnici (health/ping) via Minimal API; eventuale AddControllers/MapControllers è ammesso, ma i Controllers “di prodotto” verranno introdotti nello Step 10.
 > Nota: A2 copre bootstrap/config + health/diagnostica;

      │ │ │
      ▼ ▼ ▼

A3 - CI baseline (GitHub Actions) ⬜
      ├─ Workflow: restore/build/test su push + PR ⬜
      ├─ Target: .NET SDK coerente col progetto (es. 8.0.x) ⬜
      └─ Verifica run verde su GitHub ⬜

      │ │ │
      ▼ ▼ ▼

B - EF Core (preflight: tooling + provider, SENZA DbContext e SENZA migrations) ⬜
      ├─ Verifica tool `dotnet-ef` (`dotnet ef --version`) ⬜
      ├─ Installare/validare provider PostgreSQL ⬜
      │    └─ `Npgsql.EntityFrameworkCore.PostgreSQL` ⬜
      └─ (se necessario) pacchetto Design-time per EF (`Microsoft.EntityFrameworkCore.Design`) ⬜

 > Nota: in B non si crea DbContext e non si eseguono migrations.


      │ │ │
      ▼ ▼ ▼

C - Baseline architetturale (paradigma backend) ✅
      ├─ Clean Architecture + Hexagonal ✅
      └─ DDD + SOLID/DRY/KISS/YAGNI ✅

      │ │ │
      ▼ ▼ ▼

00 - Repo hygiene (non funzionale) ✅
       ├─ .gitignore (bin/ obj/ *.user Server_Backup/) ✅
       └─ rimozione template WeatherForecast ✅

      │ │ │
      ▼ ▼ ▼

01 - Domain/Exceptions/
       ├─ 01_DomainException.cs/md ✅
       ├─ 01_DomainOperationException.cs/md ✅
       ├─ 01_DomainValidationException.cs/md ✅
       └─ 01_UnauthorizedDomainOperationException.cs/md ✅

      │ │ │
      ▼ ▼ ▼

02 - Data/Errors/
       └─ 02_StartupErrorMessages.cs/md ✅ 

      │ │ │
      ▼ ▼ ▼

03 - Domain/Errors/
       ├─ 03_JokeErrorMessages.cs/md ✅
       └─ 03_ApplicationUserErrorMessages.cs/md ✅

      │ │ │
      ▼ ▼ ▼

04 - Domain/ValueObjects/
  (04a_Joke_ValueObjects)
       ├─ 04a_QuestionText.cs/md ✅
       ├─ 04a_AnswerText.cs/md ✅
       ├─ 04a_JokeId.cs/md ✅
       └─ 04a_UserId.cs/md ✅

  (04b_ApplicationUser_ValueObjects)
       ├─ 04b_AvatarUrl.cs/md ✅
       ├─ 04b_EmailAddress.cs/md ✅
       └─ 04b_DisplayName.cs/md ✅

      │ │ │
      ▼ ▼ ▼

05 - Domain/Entities & Aggregates/
       ├─ 05a_Joke.cs/md ✅
       └─ 05b_ApplicationUser.cs/md ✅

      │ │ │
      ▼ ▼ ▼

06a - Domain/Primitives/
       └─ 06_AggregateRoot.cs/md ✅

      │ │ │
      ▼ ▼ ▼

06b - Domain/Events/
       ├─ 06_IDomainEvent.cs/md ✅
       ├─ 06_DomainEvent.cs/md ✅
       ├─ 06_JokeWasCreated.cs/md ✅
       ├─ 06_JokeWasUpdated.cs/md ✅
       ├─ 06_JokeWasLiked.cs/md ✅
       └─ 06_JokeWasUnliked.cs/md ✅

07a - Persistence (Domain Data Model: EF Core + DbContext + Migrations) 🟡

 > Prerequisiti: 
 > - A2 completato (config + DB reachability verificata via /api/db/ping in DEV o /health/ready, SENZA EF)
 > - B completato (tooling/provider EF pronti, SENZA migrations)

      ├─ Data/JokesDbContext.cs (definizione + configurazione) 🟡
      ├─ Data/Converters/*.cs (VO ↔ DB) 🟡
      ├─ Mapping Entities/Aggregates + relazioni + constraints ⬜
      ├─ Migrations/* (prima migration iniziale) ⬜
      └─ Applicazione migration su PostgreSQL + verifica schema ⬜

07b - Identity Persistence & Security Baseline (DB + tabelle Identity + policy) ⬜
      ├─ Scelta modello Identity (infrastruttura, non Domain) ⬜
      ├─ Configurazione EF/Stores Identity + migrazione Identity ⬜
      ├─ Password policy + lockout + confirmed email ⬜
      └─ (opzionale) seeding ruoli/policy ⬜

      │
      ▼
08 - DTO Definition (Input/Output API) 🟡
      ├─ DTOs/JokeDto.cs (presente, da verificare) 🟡
      ├─ DTOs/RegisterUserDto.cs (presente, da verificare) 🟡
      └─ DTOs/UserDto.cs (presente, da verificare) 🟡

      │
      ▼
09 - Application Services (UseCases / Business Logic) ⬜
      └─ Auth use cases (register/login, token issuing) ⬜
      │
      ▼
10 - API Controllers & Integration (frontend ↔ backend) 🟡
      ├─ Program.cs (hardening hosting + security/auth pipeline + ambienti) + appsettings* + launchSettings 🟡
      ├─ JWT auth pipeline (Authentication/Authorization) ⬜
      └─ Controllers reali (JokesController / UsersController / AuthController) ⬜

 > Nota: lo step 10 estende Program.cs oltre A2 (auth/JWT, CORS, hosting finale, integrazione client, ecc.)
```
---

# 🎯 Interpretazione dettagliata della timeline

## 🧪 Step 0 — Bootstrap & Quality Gate (A1 → A3)

Obiettivo: stabilizzare l’avvio del backend e introdurre un controllo qualità automatico prima di EF/migrations e prima di iterare sul resto.

### A1 — Setup DB locale ✅
Serve a garantire che PostgreSQL sia disponibile e correttamente configurato (ruolo dedicato, privilegi, `.env` gitignored, placeholder in appsettings).

### A2 — Bootstrap backend senza EF 🟡
In A2 si vuole dimostrare che:
- la configurazione è pulita e “environment-aware” (DEV vs NON-DEV);
- la connection string viene risolta con priorità esplicita (env cs → fallback config → DB_* compose);
- l’app fallisce subito se la configurazione DB è assente o fittizia (fail-fast);
- la diagnostica ricca resta confinata a Development;
- esistono endpoint minimi standard per verificare stato processo e readiness del DB senza EF.

**Definition of Done (A2)**
- `GET /health` → 200
- `GET /health/ready` → 200 se DB raggiungibile, 503 se DB non raggiungibile
- `/api/db/ping`, OpenAPI e DeveloperExceptionPage → disponibili solo in Development
- log “startup” senza segreti (password sempre mascherata)
- In NON-DEV: OpenAPI e /api/db/ping non sono raggiungibili (404 o non mappati).

### A3 — CI baseline (GitHub Actions) ⬜
Introduce un gate automatico: ogni push/PR deve passare restore/build/test.

**Definition of Done (A3)**
- workflow presente in `.github/workflows/*`
- su push/PR: restore/build/test ok
- prima run su GitHub “verde”

## 🧱 Step 1 — Domain Layer

Obiettivo: un **dominio completo, coerente e indipendente** da DB, framework e HTTP.

---

### 01 — Domain Exceptions (`01_*.cs` / `01_*.md`) ✅

Gerarchia eccezioni di dominio (pura), usata per:

* violazioni regole business,
* operazioni non consentite,
* validazioni fallite,
* permessi mancanti.

---

### 02 — Data Errors Startup (`02_StartupErrorMessages.cs` / `02_*.md`) ✅

Messaggi tecnici di avvio (DB/DbContext). È nel layer Data, ma è un prerequisito pratico del backend.

---

### 03 — Domain Errors (`03_*.cs` / `03_*.md`) ✅

Cataloghi messaggi:

* `JokeErrorMessages`
* `ApplicationUserErrorMessages`

Usati da VO, entità/aggregate ed eccezioni di validazione.

---

### 04 — Domain Value Objects (`04a_*` / `04b_*`) ✅

Schema per entità:

* `04a_*` → VO di `Joke` ✅
* `04b_*` → VO di `ApplicationUser` ✅

VO immutabili, auto-validanti, con messaggi centralizzati.

---

### 05 — Domain Entities & Aggregates ✅

* `Joke` è **Aggregate Root** e oggi eredita da `AggregateRoot` (Domain/Primitives), quindi genera eventi tramite API base.
* `ApplicationUser` è entità di dominio pura, con VO e regole minime coerenti (doc “truth-first”, niente legacy confondente).

---

### 06a — Domain Primitives ✅

Qui collochi correttamente `AggregateRoot`:

* standardizza la **coda eventi**,
* evita boilerplate negli aggregate,
* abilita un flusso pulito: mutate → accoda evento → persist → pull & publish.

---

### 06b — Domain Events ✅

* Contratti base: `IDomainEvent`, `DomainEvent`
* Eventi concreti per `Joke`: Created / Updated / Liked / Unliked
* Tutto documentato e allineato al dominio attuale.

---

## 🗄️ Step 2 — Persistence & Infrastructure

### 07 — Persistence 🟡
Lo step 07 copre la **persistenza su PostgreSQL** e si divide in:

- **07a — Domain Data Model (EF Core)**: persistenza del **modello di dominio** (Aggregate/Entities/Value Objects) tramite `DbContext`, mapping e migrations.
- **07b — Identity Persistence & Security Baseline**: persistenza e configurazione dell’infrastruttura **Identity/Security** (tabelle Identity, policy, ecc.).

> Prerequisito (fuori dallo step 07): reachability DB verificata (bootstrap A2 completato)
> Questo evita di confondere problemi di configurazione/credenziali con problemi di mapping EF.

---

#### 07a — Persistence del dominio (EF Core + DbContext + Converters + Migrations) 🟡

Obiettivo: rendere persistibile il **Domain Model** in PostgreSQL introducendo EF Core in modo controllato e producendo una **prima migrazione iniziale** riproducibile e verificabile.

##### 07a.1 — Definizione del DbContext del dominio 🟡
- 🟡 Verificare/implementare `Data/JokesDbContext.cs` come **fonte di verità** per la persistenza del dominio.
- ⬜ Definire chiaramente dove risiedono le configurazioni (Fluent API / configuration classes), evitando contaminazioni nel Domain Layer (niente attributi EF nel Domain, salvo scelta esplicita documentata).
- ⬜ Verificare la configurazione di base: provider Npgsql, schema target, naming, ecc. (solo quanto serve allo schema iniziale).

##### 07a.2 — Mapping Entities / Aggregates ⬜
- ⬜ Mappare le entità principali (almeno `Joke` e `ApplicationUser`) con:
  - ⬜ Primary Key coerente con gli ID del dominio (Value Object o tipo scalar equivalente).
  - ⬜ Proprietà richieste / opzionali (Required/Optional) coerenti col Domain.
  - ⬜ Vincoli di lunghezza e forma (es. max length su testi, URL, display name, ecc.).
  - ⬜ Indici essenziali (solo se motivati).
- ⬜ Definire relazioni e cardinalità (es. `ApplicationUser (1) -> (N) Jokes`) con vincoli e comportamento di delete coerenti.
- ⬜ Se si introducono colonne tecniche (es. CreatedAt/UpdatedAt), documentare dove vivono (Domain vs Infrastructure) e perché.

##### 07a.3 — Mapping dei Value Objects (Converters / Owned Types) 🟡
- 🟡 Verificare/implementare `Data/Converters/*` per convertire i Value Object del dominio verso tipi persistibili e viceversa.
- ⬜ Applicare i converter ai mapping corretti (chiavi e proprietà).
- ⬜ Validare che i converter producano colonne coerenti (tipi, nullability, lunghezze, ecc.).
- ⬜ Documentare la scelta tecnica: ValueConverter vs Owned Types (se rilevante), in coerenza col progetto.

##### 07a.4 — Prima migration (iniziale) ⬜

> Prerequisito: Step B completato (tooling/provider OK). 
> In questa fase si eseguono le prime migrazioni del modello dominio.

- ⬜ Decidere convenzione naming migration (es. `InitialDomainModel`).
- ⬜ Generare la migration iniziale del **solo modello dominio**.
- ⬜ Applicare la migration su PostgreSQL reale.
- ⬜ Verificare lo schema risultante:
  - ⬜ tabelle attese presenti,
  - ⬜ colonne e tipi corretti,
  - ⬜ vincoli (PK/FK/unique) coerenti,
  - ⬜ indici essenziali presenti (se definiti).
- ⬜ Verificare che l’update sia ripetibile su DB vuoto (riproducibilità).

##### 07a.5 — Definition of Done (07a) ⬜
- ⬜ `dotnet ef migrations add <nome>` produce una migrazione pulita e consistente.
- ⬜ `dotnet ef database update` completa senza errori.
- ⬜ Lo schema risultante è coerente con Domain + mapping.
- ⬜ `Data/JokesDbContext.cs` e `Data/Converters/*` risultano verificati e “chiudibili”.
- ⬜ `Migrations/*` presenti e allineate allo stato reale.

Output atteso (quando 07a sarà chiuso):
- ⬜ `Data/JokesDbContext.cs` → ✅
- ⬜ `Data/Converters/*` → ✅
- ⬜ `Migrations/*` → ✅
- ⬜ DB aggiornato allo schema iniziale del dominio → ✅

---

#### 07b — Identity Persistence & Security Baseline (EF Identity + policy) ⬜
Obiettivo: introdurre la persistenza Identity (tabelle e migrazioni dedicate) e una baseline minima di security (password policy, lockout, conferme, ecc.), mantenendo la separazione tra Domain Model e Identity Model.

- ⬜ Scelta modello Identity (infrastruttura, non Domain).
- ⬜ Configurazione EF/Stores Identity + migrazione Identity.
- ⬜ Password policy + lockout + confirmed email.
- ⬜ (Opzionale) seeding ruoli/policy.
- ⬜ Definition of Done (07b): migrazione Identity applicata e pipeline minima coerente.


---

## 🧩 Step 3 — Application & API Layer

### 08 — DTO 🟡
* Presenti: `JokeDto`, `UserDto`, `RegisterUserDto`
* Da completare: DTO per Create/Update + validazione/mapping

### 09 — Use Cases / Application Services ⬜

### 10 — Controllers & Integration 🟡
* Template `WeatherForecast` rimosso ✅
* Da fare: controllers reali + error model + auth pipeline
* Hosting & configuration (`Program.cs`, `appsettings*`, `launchSettings`)
* (inclusa la pipeline JWT: Authentication/Authorization + AuthController)

---

