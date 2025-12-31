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
| A2 Bootstrap (senza EF) | 🟡 | `Program.cs` + config/DI da verificare; manca ping DB + fail-fast |
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

## A2) Bootstrap backend (config + connessione DB SENZA EF) 🟡

**Obiettivo A2:** avvio applicazione + config locale + validazione minima della connettività PostgreSQL **senza EF/migrations**.

### A2.1 — Config & startup 🟡
- 🟡 `Program.cs`: caricamento `.env` (DotNetEnv) + env vars in `IConfiguration`
- 🟡 Risoluzione connection string `ConnectionStrings:JokesDb` (source of truth: env)
- ⬜ Fail-fast: se `JokesDb` manca o è vuota → errore chiaro in startup (solo in Development, o comunque con messaggio non ambiguo)

### A2.2 — Preflight DB connectivity ⬜
- ⬜ Endpoint temporaneo (solo Development): `GET /api/db/ping` → `SELECT 1` con `Npgsql`
- ⬜ Post-verifica: disabilitare o rimuovere `/api/db/ping` fuori da Development

> Nota: A2 serve a separare problemi di credenziali/config da problemi EF/mapping (07a).

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

> Prerequisiti: A2 chiuso (ping OK) + B chiuso (tooling/provider OK).

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
