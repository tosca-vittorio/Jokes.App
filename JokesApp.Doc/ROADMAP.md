# 🗺️ JokesApp.Doc/ROADMAP.md (AS-IS / Next Steps)

Documento di direzione per mantenere la doc “truth-first”. Riassume lo stato reale del repository e le prossime milestone evolutive.

---

## 1) Stato attuale (AS-IS)
- **Backend — Domain**: entità/VO/eventi/eccezioni completati e coerenti con DDD.
- **Backend — Application Layer**: non esiste ancora un layer dedicato (use case/ports/handlers assenti).
- **Backend — Infrastructure**: `JokesDbContext` e ValueConverters presenti ma da riallineare al Domain; migrations rimosse in attesa di rigenerazione; avvio vincolato alla connection string PostgreSQL (`JokesDb`), gestita via variabili d’ambiente (caricate da `.env` in locale).
- **Backend — API**: nessun controller reale; DTO e attributi di validazione esistono ma non sono ancora usati da endpoint reali.
- **Auth**: Identity da configurare.
- **Frontend**: progetto React + Vite in stato di scaffold, senza routing o chiamate API.
- **Testing**: **solution separata `JokesApp.Tests`** (unit test Domain: Value Objects / Entities / Events / Validation) + doc-hub dedicato in `JokesApp.Doc/JokesApp.Tests` (timeline e toDo separati).
  - Nota: la timeline del Server (`JokesApp.Doc/JokesApp.Server/TIMELINE.md`) resta focalizzata su Domain/Persistence/API e **non** traccia i test.
- **CI/CD**: nessuna pipeline configurata (`.github/workflows/` non presente).
- **Documentazione**: hub e documenti globali aggiornati allo stato AS-IS/TO-BE attuale; audit doc generale chiuso.

---

## 2) Milestone vicine (0–1)
- 🟡 **Allineamento persistenza/boot**
  - **A2 (senza EF):** config locale (`appsettings.json` / `.env`) + fail-fast; smoke test `GET /api/db/ping` (Npgsql + `SELECT 1`) per validare connection string + raggiungibilità + credenziali.
  - **B (EF senza migrations):** verificare tooling `dotnet-ef` e installare/validare provider PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`) + (se serve) `Microsoft.EntityFrameworkCore.Design`.
  - **07a (quando il Domain è stabile):** riallineare `JokesDbContext` + Converters e rigenerare/applicare le **migrations** su PostgreSQL reale.

- 🟡 **API + Application Layer (MVP Jokes)**  
  - Definire Ports + Use Case (Create/Update/Like/Unlike/Get) in un Application Layer dedicato.  
  - Implementare controller HTTP reali (es. `JokesController`) che orchestrano i use case e mappano DTO ↔ dominio.  
  - Gestire trasformazione errori dominio → HTTP (ProblemDetails o simile).

- 🟡 **Identity/Auth pipeline**  
  - Configurare policy/password/lockout.  
  - Implementare JWT issuing/validation + controller di auth.  
  - Proteggere gli endpoint di dominio.

---

## 3) Milestone successive (1–2)
- ⬜ **Frontend first-pass**  
  - Routing client-side, viste jokes (list/create/update/like).  
  - Client HTTP centralizzato per le API backend.

- ⬜ **Testing strategico**  
  - Unit test su VO/entità/aggregate root.  
  - Integration test per DbContext + controller.  
  - Hardening auth/JWT con test dedicati.

- ⬜ **Osservabilità & hardening**  
  - Logging/coerentizzazione errori HTTP.  
  - Event dispatch del dominio (in-process) dopo commit.  
  - Validazione timestamp/constraint DB.

---

## 4) Milestone a medio termine (2+) — backlog
- ⬜ **Realtime / SignalR** per eventi live.  
- ⬜ **Audit trail / logging funzionale**.  
- ⬜ **CI/CD**: pipeline build/test (dotnet + npm) e pubblicazione.  
- ⬜ **Refinement frontend**: UX, state management, testing UI/E2E.

---

## 5) Stato degli owner documentali
- `ARCHITECTURE.md` → allineato al codice attuale (Application layer dichiarato TO-BE).  
- `DESIGN_PATTERNS.md` → evidenzia pattern usati (factory/events) e quelli TO-BE (adapter/command).  
- `WORKFLOW.md` → chiarisce che la monorepo è già bootstrapata; CI/CD assente.  
- `toDo.md` → cruscotto operativo per i task concreti (da usare come fonte di verità).

---
