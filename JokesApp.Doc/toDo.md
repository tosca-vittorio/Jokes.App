# 📄 PROJECT-TODO.md

Documentazione centralizzata delle attività concluse, in corso o da implementare per completare **JokesApp** secondo:
- Clean Architecture + DDD
- SOLID / DRY / KISS / YAGNI
- documentazione “truth-first” (descrive solo ciò che esiste davvero)

---

## ✅ Legenda
- ✅ **Fatto / Chiuso (verificato e allineato a codice + doc)**
- 🟡 **Presente / In corso (esiste nel repo ma non ancora verificato/chiuso)**
- ⬜ **Da fare**

---

# 🧠 0) Idee future (backlog, non vincolante)

Possibili estensioni naturali del modello `Joke` (solo se emergono casi d’uso reali):
- ⬜ `Category` / `Tags`
- ⬜ `Rating` (1–5) oppure ranking/score
- ⬜ `IsPublic` / `IsDeleted` (soft delete / visibilità)
- ⬜ likes user-based (non solo contatore)

> Nota: queste non sono “debiti”, sono backlog. Si implementano solo quando servono davvero.

---

# 🖥️ A) SERVER (BackEnd)

## A1) Domain Layer — ✅ COMPLETATO (chiuso)

### A1.1 Exceptions (01_*)
- ✅ DomainException
- ✅ DomainOperationException
- ✅ DomainValidationException
- ✅ UnauthorizedDomainOperationException

### A1.2 Domain Errors (03_*)
- ✅ JokeErrorMessages
- ✅ ApplicationUserErrorMessages

### A1.3 Value Objects (04a_* + 04b_*)
- ✅ Joke: QuestionText, AnswerText, JokeId, UserId
- ✅ ApplicationUser: EmailAddress, DisplayName, AvatarUrl

### A1.4 Entities / Aggregates (05a_* + 05b_*)
- ✅ Joke (Aggregate Root)
- ✅ ApplicationUser (Domain Entity)

### A1.5 Primitives (06a_*)
- ✅ AggregateRoot (event queue + PullDomainEvents)

### A1.6 Domain Events (06b_*)
- ✅ IDomainEvent + DomainEvent
- ✅ JokeWasCreated / JokeWasUpdated / JokeWasLiked / JokeWasUnliked

### A1.7 Domain Services (06c_*)
- ✅ Skipped (YAGNI) — nessun caso reale emerso finora

---

## A2) Cose già presenti nel repo ma NON ancora verificate/chiuse 🟡

> Queste esistono già nel filesystem, ma non le abbiamo ancora validate “file-by-file” come il Domain.

- 🟡 `Program.cs` (bootstrap / DI / middleware)
- 🟡 `appsettings*.json` + `.env`
- 🟡 `Properties/launchSettings.json`
- 🟡 `Validation/CustomEmailAttribute.cs`
- 🟡 `DTOs/*` (JokeDto, UserDto, RegisterUserDto)
- 🟡 `Data/JokesDbContext.cs`
- 🟡 `Data/Converters/*`
- 🟡 `Migrations/*`
- 🟡 `Controllers/WeatherForecastController.cs` + `WeatherForecast.cs` (template)

---

## A3) Persistence & Data (Step 07a) — 🟡

### A3.1 DbContext + EF Core
- 🟡 Verificare `JokesDbContext` (mapping Entities + VO)
- 🟡 Verificare `Converters` (ValueObjects <-> DB)
- 🟡 Verificare migrations esistenti e coerenza con Domain attuale
- ⬜ Nuova migration (se necessaria) dopo riallineamento mapping
- ⬜ Policy su timestamp (CreatedAt/UpdatedAt) e constraints DB

### A3.2 Repository & UoW (interfacce)
- ⬜ Definire interfacce repository (es. `IJokeRepository`, `IUserRepository`)
- ⬜ Definire unit-of-work / transaction boundary (se serve)
- ⬜ Strategia dispatch DomainEvents (in-process o Outbox)

---

## A4) Application Layer / Use Cases (Step 09) — ⬜ DA FARE

> Qui si scrivono i casi d’uso, non “services generici” senza direzione.

- ⬜ Use case: CreateJoke
- ⬜ Use case: UpdateJoke
- ⬜ Use case: LikeJoke / UnlikeJoke
- ⬜ Use case: GetJokeById / ListJokes (con filtri)
- ⬜ Use case: RegisterUser / UpdateProfile
- ⬜ Gestione eccezioni Domain -> error handling applicativo
- ⬜ Dispatch DomainEvents dopo persistenza

---

## A5) API Layer / Controllers (Step 10) — 🟡/⬜

- 🟡 Rimuovere o sostituire template WeatherForecast
- ⬜ `JokesController`
- ⬜ `UsersController`
- ⬜ `AuthController`
- ⬜ Mapping errori in `ProblemDetails` (coerente e standard)
- ⬜ Versioning API (opzionale)

---

## A6) Identity & Security (Step 07b/09/10) — ⬜

### A6.1 Identity Options
- ⬜ Password policy + lockout + require confirmed email
- ⬜ Gestione ruoli/policy

### A6.2 JWT Auth
- ⬜ Token generation + validation
- ⬜ Refresh token (se previsto)
- ⬜ Revoca token / blacklist (se previsto)
- ⬜ Protezione endpoint + policy

---

## A7) Eventing avanzato / Logging / Audit / SignalR — ⬜ (backlog avanzato)

> Questo è un “Big Feature Set”. Si fa dopo MVP.

- ⬜ Event handlers (log funzionale/tecnico)
- ⬜ Audit trail (tabella AuditEvent)
- ⬜ SignalR hub `/eventHub`
- ⬜ Broadcast eventi al client (toast/popup/animazioni)
- ⬜ Monitoraggio realtime e metriche (opzionale)

---

## A8) Testing (Step 11) — ⬜ (da impostare)

> Stato reale: **non verificato** nel repo. Quindi lo tratto come da impostare.

### A8.1 Unit test
- ⬜ Unit test Value Objects
- ⬜ Unit test Aggregate (`Joke`) e invarianti
- ⬜ Unit test `AggregateRoot` (Pull/Clear/Add)

### A8.2 Integration test
- ⬜ DbContext + mapping (VO + Entities)
- ⬜ Controllers con `WebApplicationFactory`
- ⬜ JWT/Auth integration tests

### A8.3 E2E
- ⬜ Playwright/Cypress (se hai UI)

---

# 🌐 B) CLIENT (FrontEnd)

> Non risulta ancora verificato/organizzato nel repo: tratto tutto come backlog.

## B1) Setup base
- ⬜ Setup progetto (React/Vite o Next) + routing
- ⬜ Client HTTP (fetch/axios) + error handling standard
- ⬜ Gestione config env

## B2) Auth
- ⬜ Login / Register UI
- ⬜ Gestione token + refresh (se previsto)
- ⬜ `useAuth` + route protection

## B3) Jokes UI
- ⬜ Lista jokes + filtri/ordinamenti
- ⬜ Create/Update joke
- ⬜ Like/Unlike realtime (anche senza SignalR inizialmente)

## B4) Profile UI
- ⬜ Profilo utente (display name / avatar / email)
- ⬜ Update profile

## B5) Realtime (SignalR) — backlog
- ⬜ Client SignalR + gestione eventi
- ⬜ Toast/popup/badge + animazioni

## B6) Testing FE
- ⬜ Unit test componenti/hook
- ⬜ E2E (Playwright/Cypress)

---

# 📚 C) DOCUMENTAZIONE (Doc)

## C1) Stato attuale ✅/🟡
- ✅ Documentazione Domain (file-by-file) completata e allineata
- ✅ TIMELINE.md aggiornata (con 06a/06b e 07a/07b, … , ✅/🟡/⬜)
- 🟡 README/ROADMAP/ARCHITECTURE: da riallineare allo stato reale

## C2) Da fare ⬜
- ⬜ Aggiornare albero directory nel README root (quando stabilizzi i layer)
- ⬜ Documentare Persistence (DbContext/mapping/migrations) dopo verifica
- ⬜ Documentare Application Layer (use cases) dopo implementazione
- ⬜ Documentare API (endpoint, error model, auth) quando i controller esistono
- ⬜ Diagramma architetturale aggiornato (opzionale ma consigliato)

---

# ⭐ Overview sintetica (verità ad oggi)
| Area              | Stato |
|------------------|------:|
| Domain Layer      | ✅ Chiuso |
| Persistence       | 🟡 Presente ma da verificare/chiudere |
| DTO               | 🟡 Presenti ma da verificare/chiudere |
| Application Layer | ⬜ Da fare |
| Controllers/API   | 🟡 Template presente, resto da fare |
| Identity/JWT      | ⬜ Da fare |
| Client            | ⬜ Da fare (non verificato) |
| Testing           | ⬜ Da impostare/implementare |
| Doc generale       | 🟡 Da riallineare (README/ROADMAP/ARCHITECTURE) |
