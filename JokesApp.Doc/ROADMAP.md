# 🗺️ JokesApp — Roadmap (AS-IS / Next Steps)

Documento di direzione per mantenere la doc “truth-first”. Riassume lo stato reale del repository e le prossime milestone evolutive.

---

## 1) Stato attuale (AS-IS)
- **Backend — Domain**: entità/VO/eventi/eccezioni completati e coerenti con DDD.
- **Backend — Application Layer**: non esiste ancora un layer dedicato (use case/ports/handlers assenti).
- **Backend — Infrastructure**: `JokesDbContext` e ValueConverter presenti ma da riallineare al Domain; migrations rimosse in attesa di rigenerazione; avvio vincolato alla connection string PostgreSQL (`DefaultConnection`).
- **Backend — API**: nessun controller reale (template `WeatherForecast` rimosso); DTO e attributi di validazione esistono ma non sono ancora usati da endpoint reali.
- **Auth**: Identity configurata ma senza pipeline JWT/policy né controller di autenticazione/autorizzazione.
- **Frontend**: progetto React + Vite in stato di scaffold, senza routing o chiamate API.
- **Testing**: solo test di esempio (“green placeholder”), nessuna suite su dominio/persistenza/API.
- **CI/CD**: nessuna pipeline configurata (`.github/workflows/` non presente).
- **Documentazione**: hub e documenti globali aggiornati allo stato AS-IS/TO-BE attuale; audit doc generale chiuso.

---

## 2) Milestone vicine (0–1)
- 🟡 **Allineamento persistenza/boot**  
  - Verificare `JokesDbContext`, converters e migration con PostgreSQL reale.  
  - Gestire config locale (`appsettings.Development.json` / `.env`) e fail-fast.

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
