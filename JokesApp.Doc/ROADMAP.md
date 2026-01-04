# 🗺️ JokesApp.Doc/ROADMAP.md (AS-IS / Next Steps)

Documento di direzione per mantenere la doc “truth-first”. Riassume lo stato reale del repository e le prossime milestone evolutive.

---

## 1) Stato attuale (AS-IS)

- **Backend — Bootstrap (A1→A3)**
  - **A1** completato: PostgreSQL locale + ruolo/permessi + `.env` gitignored + `appsettings.json` placeholder non sensibile.
  - **A2** completato: bootstrap backend senza EF chiuso (config, fail‑fast, logging safe, health + ping DB).
  - **A3** non presente: nessun workflow GitHub Actions in `.github/workflows/`.

- **Backend — Domain**
  - Dominio (VO / Entities / Events / Exceptions) completato e coerente con DDD.

- **Backend — Application Layer**
  - Non esiste ancora un layer dedicato (Use Cases / Ports / Handlers assenti).

- **Backend — Infrastructure / Persistence**
  - `JokesDbContext` e ValueConverters presenti ma da riallineare al Domain.
  - Migrations non consolidate (fase di rigenerazione prevista quando 07a parte).
  - Connessione DB gestita via variabili d’ambiente in locale (caricate da `.env` in Development).

- **Backend — API**
  - Nessun controller “di prodotto” reale.
  - DTO e attributi di validazione esistono ma non sono ancora agganciati a endpoint reali.

- **Auth**
  - Identity/Security baseline non configurata.

- **Frontend**
  - React + Vite in stato di scaffold; nessun routing e nessuna integrazione API stabile.

- **Testing**
  - Solution separata `JokesApp.Tests` con unit test del Domain (Value Objects / Entities / Events / Validation).
  - Nota: la timeline del Server (`JokesApp.Doc/JokesApp.Server/TIMELINE.md`) traccia Domain/Persistence/API; i test hanno doc-hub dedicato in `JokesApp.Doc/JokesApp.Tests`.

- **CI/CD**
  - Nessuna pipeline configurata (assenza di `.github/workflows/`).

- **Documentazione**
  - Hub e documenti globali allineati; TIMELINE e toDo del Server aggiornati (Step 0: A1→A3 introdotto).

---

## 2) Milestone vicine (0–1)

### 🟡 Step 0 — Quality Gate (A3)

- ⬜ **A3 — CI baseline (GitHub Actions)**
  - Workflow minimo “quality gate” su `push` + `pull_request`:
    - `dotnet restore`
    - `dotnet build --no-restore`
    - `dotnet test --no-build`
  - Obiettivo: prima run “verde” su GitHub.

> Nota: A3 è volutamente baseline. Niente Docker/Jenkins/K8s in questa fase.

### 🟡 Preflight EF (B) e avvio 07a

- **B — EF Core preflight (senza DbContext/migrations)**
  - verificare `dotnet-ef`
  - validare provider PostgreSQL `Npgsql.EntityFrameworkCore.PostgreSQL`
  - (se serve) `Microsoft.EntityFrameworkCore.Design`

- **07a — Persistence Domain (quando A2+B sono chiusi)**
  - riallineare `JokesDbContext` + Converters
  - rigenerare e applicare migrations su PostgreSQL reale
  - verificare schema risultante e riproducibilità su DB vuoto

---

## 3) Milestone successive (1–2)

- ⬜ **API + Application Layer (MVP Jokes)**
  - Definire Ports + Use Cases (Create/Update/Like/Unlike/Get/List) in un Application Layer dedicato.
  - Implementare controller HTTP reali (es. `JokesController`) che orchestrano i use case e mappano DTO ↔ dominio.
  - Trasformazione errori dominio → HTTP (ProblemDetails o modello coerente).

- ⬜ **Identity/Auth pipeline**
  - Configurare baseline security (policy/password/lockout).
  - JWT issuing/validation + controller di auth.
  - Protezione endpoint.

- ⬜ **Frontend first-pass**
  - Routing client-side, viste jokes (list/create/update/like).
  - Client HTTP centralizzato per le API backend.

- ⬜ **Testing strategico**
  - Estendere unit test dove utile (Domain già coperto).
  - Integration test per DbContext + controller.
  - Hardening auth/JWT con test dedicati.

- ⬜ **Osservabilità & hardening**
  - Logging e standardizzazione errori HTTP.
  - Dispatch DomainEvents (in-process) post-persistenza.
  - Validazione timestamp/constraint DB.

---

## 4) Milestone a medio termine (2+) — backlog

- ⬜ **CD/Deploy** (solo quando esiste un MVP end-to-end)
  - ambiente di staging minimo
  - publish/build artifacts
  - eventuale deploy (provider da decidere)

- ⬜ **Containerization / DevOps avanzato (didattico, non obbligatorio)**
  - Docker (compose per Postgres + app) quando serve riproducibilità cross-machine
  - Jenkins/Kubernetes solo se l’obiettivo diventa “pipeline avanzata” o multi-ambiente reale

- ⬜ **Realtime / SignalR** per eventi live
- ⬜ **Audit trail / logging funzionale**
- ⬜ **Refinement frontend**: UX, state management, testing UI/E2E

---

## 5) Stato degli owner documentali

- `ARCHITECTURE.md` → allineato al codice attuale (Application layer dichiarato TO-BE).
- `DESIGN_PATTERNS.md` → pattern usati (factory/events) e quelli TO-BE (adapter/command).
- `WORKFLOW.md` → monorepo bootstrapata; CI/CD assente (A3 pianificato).
- `JokesApp.Doc/JokesApp.Server/TIMELINE.md` → sequenza A1→10 aggiornata (include A3).
- `JokesApp.Doc/JokesApp.Server/toDo.md` → cruscotto operativo (fonte di verità operativa).
