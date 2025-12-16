# Flow Update pratico (come usarli mentre sviluppi)

* **TIMELINE.md = ordine di sviluppo** (cosa fare prima/dopo) e stato globale per step. 
* **toDo.md = cruscotto operativo** (cosa esiste / cosa manca / cosa è stato chiuso), anche non ordinato. 

### 1) Parti sempre dalla TIMELINE

* Scegli **il primo step non chiuso** (🟡 o ⬜) dall’alto verso il basso. 
* Quello è il tuo “blocco di lavoro” della sessione.

### 2) Esegui il blocco fino a “Definition of Done”

Per considerare un punto “chiuso” (✅), la regola che ti consiglio è:

* codice implementato + verificato (e se serve refactor)
* documentazione allineata
* build/run ok (e migrazione aggiornata se siamo in 07a/07b)
* almeno i test minimi coerenti con lo step (se già presenti)

### 3) Aggiorna DOPO il TODO (non prima)

* Nel TODO:

  * sposti voci da 🟡→✅ quando le hai davvero verificate/chiuse 
  * aggiungi nuove task scoperte “per non perderle”, anche se non sono in ordine.

### 4) Chiudi anche la TIMELINE

Quando uno step è concluso:

* in TIMELINE cambi 🟡/⬜ → ✅ sullo step (e sulle sotto-voci). 

Quindi sì: **la timeline guida, il TODO traccia e fotografa**. È un flusso molto maturo.
Se vuoi continuare in modo rigoroso, il prossimo blocco naturale è:
**00 Repo hygiene → 07a Persistence (DbContext + Converters + Migrations)**.

---

# 📌 TIMELINE DEL PROGETTO (sequenza completa aggiornata 01 → 11)
```md
00 - Repo hygiene (non funzionale) ⬜
       ├─ .gitignore (bin/ obj/ *.user Server_Backup/) ✅
       └─ rimozione template WeatherForecast (se non serve) ⬜

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
      ├─ Data/JokesDbContext.cs (presente, da verificare) 🟡
      ├─ Data/Converters/*.cs (presenti, da verificare) 🟡
      ├─ Migrations/* (presenti, da verificare e riallineare al dominio) 🟡
      └─ Mapping VO + Entities + relazioni + constraints (da completare) ⬜      

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
      ├─ Program.cs + appsettings* + launchSettings (hosting/config) 🟡
      ├─ JWT auth pipeline (Authentication/Authorization) ⬜
      └─ Controllers/WeatherForecastController.cs (template / da rimuovere o sostituire) 🟡

      │
      ▼
11 - Testing (Unit + Integration + End-to-End) ⬜
        └─ Unit: Test suite per il Domain da implementare/validare
```

---

# 🎯 Interpretazione dettagliata della timeline

## 🧱 Big Step 1 — Domain Layer

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

## 🗄️ Big Step 2 — Persistence & Infrastructure (prossimo vero step)

### 07 — Persistence 🟡
Lo step 07 si divide in 07a (Domain model) e 07b (Identity)

Qui inizi davvero con:

* mapping EF Core dei VO,
* mapping di `Joke` e `ApplicationUser`,
* relazione `ApplicationUser (1) → (N) Jokes`,
* migrations.

#### 07a (DbContext/Converters/Migrations del dominio) 🟡
(mapping EF Core dei VO, mapping di Joke/ApplicationUser, migrations del modello dominio)

#### 07b — Identity Persistence & Security Baseline ⬜
(Identity tables + migrazioni Identity + policy base: password/lockout/confirmed email)

---

## 🧩 Big Step 3 — Application & API Layer

### 08 — DTO 🟡
* Presenti: `JokeDto`, `UserDto`, `RegisterUserDto`
* Da completare: DTO per Create/Update + validazione/mapping

### 09 — Use Cases / Application Services ⬜

### 10 — Controllers & Integration 🟡
* Presente: `WeatherForecastController` (template)
* Da fare: controllers reali + error model + auth pipeline
* Hosting & configuration (`Program.cs`, `appsettings*`, `launchSettings`)
* (inclusa la pipeline JWT: Authentication/Authorization + AuthController)

---

## ✅ Big Step 4 — Testing

### 11 — Testing ⬜

* Unit: VO + aggregate + primitive
* Integration: DbContext/Repo + UseCases
* E2E: pipeline API

---

