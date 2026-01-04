# 📄 JokesApp.Doc/JokesApp.Server/toDo.md

Documentazione centralizzata delle attività concluse, in corso o da implementare per completare **JokesApp.Server** secondo:
- Clean Architecture + DDD
- SOLID / DRY / KISS / YAGNI
- documentazione “truth-first” (descrive solo ciò che esiste davvero)

---

## ✅ Legenda stati
- ✅ **Fatto / Chiuso (verificato e allineato a codice + doc)**
- 🟡 **Presente / In corso (esiste nel repo ma non ancora verificato/chiuso)**
- ⬜ **Da fare**

---

## ⭐ Overview sintetica (verità ad oggi)

| Area | Stato | Note |
|---|---|---|
| A1 Setup DB locale (PostgreSQL) | ✅ | DB/ruolo/permessi + `.env` + `appsettings.json` fallback verificati |
| A2 Bootstrap (senza EF) | ✅ | `Program.cs` env/config + fail-fast + logging safe + health; ping DB solo DEV |
| A3 CI baseline (GitHub Actions) | ⬜ | restore/build/test su push + PR (quality gate minimo) |
| B EF Core preflight | ⬜ | tooling/provider ok prima di introdurre DbContext/migrations |
| Domain Layer (01–06) | ✅ | VO/Entities/Events/Exceptions allineati e documentati |
| 07a Persistence Domain (EF + DbContext + Migrations) | 🟡 | DbContext/Converters presenti ma da riallineare e verificare; migrations da rigenerare |
| 07b Identity/Security baseline | ⬜ | da introdurre (infrastruttura) |
| 08 DTO + Validation | 🟡 | presenti ma da verificare/agganciare a endpoint reali |
| 09 Application Layer (Use Cases) | ⬜ | da creare |
| 10 Controllers + Integration | 🟡 | hardening/config parziale; controllers reali ancora ⬜ |

---

# 🧠 0) Idee future (backlog, non vincolante)

Possibili estensioni naturali del modello `Joke` (solo se emergono casi d’uso reali):
- ⬜ `Category` / `Tags`
- ⬜ `Rating` (1–5) oppure ranking/score
- ⬜ `IsPublic` / `IsDeleted` (soft delete / visibilità)
- ⬜ likes user-based (non solo contatore)

> Nota: backlog = non è debito. Si implementa solo quando serve davvero.

---

# 🖥️ A) SERVER (Backend)

## A1) Setup DB locale (PostgreSQL) ✅

- ✅ Verifica client `psql` + servizio attivo
- ✅ Creazione DB `jokes` + ruolo `jokes_migrator`
- ✅ Ownership + privilegi su schema `public`
- ✅ Verifica permessi via psql (CREATE/DROP test)
- ✅ `.env` locale (gitignored)
- ✅ `appsettings.json` fallback non sensibile

---

## A2) Bootstrap backend (config + connessione DB SENZA EF) ✅

**Obiettivo A2:** avvio applicazione + configurazione “environment-aware” + verifica minima DB PostgreSQL **senza EF/migrations**.

> Nota: A2 serve a separare problemi di credenziali/config da problemi EF/mapping (07a).
> Nota: in A2 è ammesso registrare `AddControllers/MapControllers`, ma **non** si introducono ancora Controllers “di prodotto” (Step 10).

### A2.1 — Config & startup ✅
- ✅ `Program.cs`: caricamento `.env` **solo in Development** (local-first) + `builder.Configuration.AddEnvironmentVariables()`
- ✅ Risoluzione connection string con priorità esplicita:
  - ✅ A) env var `ConnectionStrings__JokesDb` (preferita)
  - ✅ B) fallback config `ConnectionStrings:JokesDb` (placeholder non sensibile)
  - ✅ C) composizione da `DB_*` (Host/Port/Name/User/Password)
- ✅ Logging startup: tracciamento “source” della connessione + **password sempre mascherata**

### A2.2 — Pipeline per ambienti (DEV vs NON-DEV) ✅
- ✅ DEV:
  - ✅ OpenAPI disponibile solo in Development
  - ✅ DeveloperExceptionPage in Development
- ✅ NON-DEV:
  - ✅ `UseExceptionHandler()` (gestione errori “pulita”)
  - ✅ `UseHsts()` (security baseline)

### A2.3 — Endpoints tecnici (senza EF) ✅
- ✅ Health endpoints (safe, anche fuori da Development):
  - ✅ `GET /health` (liveness: processo vivo)
  - ✅ `GET /health/ready` (readiness DB: `SELECT 1` con `Npgsql`, **senza EF**)
- ✅ Endpoint temporaneo (solo Development): `GET /api/db/ping` → `SELECT 1` con `Npgsql`
- ✅ Post-verifica: `/api/db/ping` non deve essere raggiungibile fuori da Development (non mappato / 404)

### A2a — launchSettings.json (profilo DEV/PROD configurato) ✅
- ✅ Profilo `server-dev` con configurazione DEV (HTTP/HTTPS, Debug/Logging, SPA proxy)
- ✅ Profilo `server-prod` con configurazione PROD (solo HTTP, senza SPA proxy)

### A2b — Program.cs: finalizzazione e chiusura ✅
- ✅ Finalizzare `Program.cs` per concludere il blocco A2b.
- ✅ **Test degli endpoint**:
  - ✅ **GET /health** (liveness: processo vivo) → Testato con PowerShell (`Invoke-RestMethod` per verificare risposta 200 OK). 
  - ✅ **GET /health/ready** (readiness DB: `SELECT 1` via Npgsql) → Testato con PowerShell (comando `Invoke-RestMethod` per confermare corretto stato del DB).
  - ✅ **GET /api/db/ping** → Testato in ambiente **Development** tramite PowerShell per confermare che restituisse `SELECT 1`.

> Nota: I test sugli endpoint sono stati eseguiti tramite PowerShell utilizzando il comando `Invoke-RestMethod` per verificare la risposta di ciascun endpoint. Questo ha confermato che gli endpoint di **liveness**, **readiness** e il ping DB funzionano come previsto.

---

## A3) CI baseline (GitHub Actions) ⬜

**Obiettivo A3:** introdurre un quality gate minimo automatico: ogni push e PR devono passare restore/build/test.

- ⬜ Creare workflow in `.github/workflows/ci.yml`
- ⬜ Trigger: `push` + `pull_request`
- ⬜ Steps minimi:
  - ⬜ `dotnet restore`
  - ⬜ `dotnet build --no-restore`
  - ⬜ `dotnet test --no-build`
- ⬜ Verificare prima run “verde” su GitHub

> Nota: A3 è volutamente “baseline”: niente Docker/Jenkins/K8s qui.

---

## B) EF Core (preflight: tooling + provider, SENZA DbContext e SENZA migrations) ⬜

- ⬜ Verifica tool `dotnet-ef` (`dotnet ef --version`)
- ⬜ Installare/validare provider PostgreSQL `Npgsql.EntityFrameworkCore.PostgreSQL`
- ⬜ (se necessario) pacchetto design-time `Microsoft.EntityFrameworkCore.Design`

> Nota: in B **non** si crea DbContext e **non** si eseguono migrations.

---

## C) Baseline architetturale + repo hygiene ✅

- ✅ Clean Architecture + Hexagonal
- ✅ DDD + SOLID/DRY/KISS/YAGNI
- ✅ Repo hygiene (es. rimozione template `WeatherForecast`, .gitignore, ecc.)

---

## 01–06) Domain Layer ✅ (chiuso)

- ✅ Exceptions (01_*)
- ✅ Domain Errors (03_*)
- ✅ Value Objects (04a_* + 04b_*)
- ✅ Entities/Aggregates (05a_* + 05b_*)
- ✅ Primitives (06a_*: `AggregateRoot` + event queue)
- ✅ Domain Events (06b_*: Created/Updated/Liked/Unliked)

---

## 07a) Persistence — Domain Data Model (EF Core + DbContext + Migrations) 🟡

> Prerequisiti: A2 chiuso (DB reachability verificata via `/health/ready` e ping DEV-only se previsto) + B chiuso (tooling/provider OK).

- 🟡 Verificare `Data/JokesDbContext.cs` (configurazione + mapping)
- 🟡 Verificare `Data/Converters/*` (Value Objects ↔ DB)
- ⬜ Definire mapping Entities/Aggregates + relazioni + constraints essenziali
- ⬜ Rigenerare migrations (iniziale)
- ⬜ Applicare migrations su PostgreSQL reale + verifica schema

---

## 07b) Identity Persistence & Security Baseline ⬜

- ⬜ Scelta modello Identity (infrastruttura, non Domain)
- ⬜ Configurazione EF/Stores Identity + migrazione Identity
- ⬜ Password policy + lockout + confirmed email
- ⬜ (opzionale) seeding ruoli/policy

---

## 08) DTO + Validation 🟡

- 🟡 `DTOs/JokeDto.cs`
- 🟡 `DTOs/UserDto.cs`
- 🟡 `DTOs/RegisterUserDto.cs`
- 🟡 `Validation/CustomEmailAttribute.cs`

- ⬜ Definire DTO per Create/Update + validazione coerente
- ⬜ Mapping DTO ↔ Domain (senza contaminare il Domain)

---

## 09) Application Layer / Use Cases ⬜

- ⬜ Ports + Use Cases (Create/Update/Like/Unlike/Get/List)
- ⬜ Register/Login + token issuing (quando si integra auth)
- ⬜ Gestione errori Domain → Application error model
- ⬜ Dispatch DomainEvents dopo persistenza

---

## 10) API Controllers & Integration 🟡/⬜

- ⬜ Controllers reali (`JokesController`, `UsersController`, `AuthController`)
- ⬜ Error model coerente (es. `ProblemDetails`)
- ⬜ Pipeline auth (JWT) + policy sugli endpoint
- ⬜ Hardening hosting/config (appsettings*, launchSettings, env)

---
