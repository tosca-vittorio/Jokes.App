# 📚 JokesApp — Documentation Hub

Questo file è l’**entrypoint della documentazione** contenuta in `JokesApp.Doc/`.
Serve a:
- guidare la lettura (percorsi consigliati, per obiettivi),
- indicare *dove* si trovano i dettagli (per area),
- fissare poche regole pratiche per mantenere la doc **aggiornata e non ridondante**.

> Nota: questo hub **non** ri-spiega l’architettura. Per quello esiste un documento dedicato (“fonte di verità globale”).

---

## ✅ Percorso di lettura consigliato (ordine “standard”)

1) **Architettura globale (fonte di verità)**  
   → `./ARCHITECTURE.md`

2) **Pattern + motivazioni**  
   → `./DESIGN_PATTERNS.md`

3) **Convenzioni (naming, struttura, doc, commit)**
   → `./CONVENTIONS.md`

4) **Regole operative**  
   → `./WORKFLOW.md`

5) **Direzione e milestone (TO-BE dichiarato)**  
   → `./ROADMAP.md`

6) **Cruscotto operativo (task verificabili)**  
   → `./toDo.md`

7) **Sequenza operativa Backend (step-by-step)**  
   → `./JokesApp.Server/TIMELINE.md`

---

## 🎯 Percorsi rapidi (in base a cosa devi fare)

### Se devi “capire il progetto” (onboarding rapido)
- `./ARCHITECTURE.md` → confini, layer, regole
- `JokesApp.Server/Program.md` → bootstrap, config e run
- `./CONVENTIONS.md` → naming, struttura, regole doc/commit
- `./WORKFLOW.md` → come si lavora sul repo e come si committa

### Se devi lavorare sul Backend
- `JokesApp.Server/TIMELINE.md` → ordine logico e dipendenze tra step
- `JokesApp.Server/` → documenti di dettaglio (Domain, Data, DTO, Validation…)

### Se devi lavorare sui Test
- `JokesApp.Tests/README.md` → overview test
- `JokesApp.Tests/doc/testing_targets.md` → obiettivi/target di test
- `JokesApp.Tests/Data/` → setup e note su DbContext test

### Se devi aggiungere/aggiornare documentazione
- `CONVENTIONS.md` → regole di naming/struttura e convenzioni documentali
- `WORKFLOW.md` → regole “truth-first”, no duplicazioni, commit chirurgici
- Se il contenuto è *globale* → aggiorna `ARCHITECTURE / ROADMAP / DESIGN_PATTERNS`
- Se è *specifico* → aggiorna la cartella area (`JokesApp.Server/`, `JokesApp.Tests/`, `JokesApp.Client/`)

---

## 🧭 Indice per area (mappa reale)

### 📌 Documenti globali (root `JokesApp.Doc/`)
- `ARCHITECTURE.md` — architettura globale, confini, dependency rule, AS-IS vs TO-BE
- `DESIGN_PATTERNS.md` — pattern adottati e motivazioni
- `WORKFLOW.md` — regole operative
- `ROADMAP.md` — direzione e milestone
- `toDo.md` — task operative verificabili
- `README.md` — navigazione e regole (questo file)
- `CONVENTIONS.md` — convenzioni (naming, struttura cartelle, doc, commit)

### 🔴 Backend — `JokesApp.Doc/JokesApp.Server/`
Entry-point principali:
- `Program.md` — bootstrap/hosting/config/middleware/DI
- `TIMELINE.md` — sequenza di sviluppo/integrazione
- `A_postgresql_appsettingsjson.md` — note pratiche su PostgreSQL + configurazione
- `B_entity_framework.md` — note pratiche su EF Core e persistenza

Indice “per argomento”:
- **Domain** → `JokesApp.Server/Domain/`
  - `Entities/` (entità)
  - `ValueObjects/` (VO)
  - `Events/` (domain events)
  - `Exceptions/` (eccezioni di dominio)
  - `Errors/` (messaggi/error catalog)
  - `Primitives/` (primitive/aggregate root)
- **Data (persistence)** → `JokesApp.Server/Data/`
- **DTOs (contratti API)** → `JokesApp.Server/DTOs/`
- **Validation** → `JokesApp.Server/Validation/`
- **Migrations** → `JokesApp.Server/Migrations/`

### 🔵 Frontend — `JokesApp.Doc/JokesApp.Client/`
Cartella dedicata alla documentazione del client (struttura UI, routing, servizi API, convenzioni).  
> Qui l’obiettivo è avere documenti *mirati* e non duplicare l’architettura globale.

### 🧪 Testing — `JokesApp.Doc/JokesApp.Tests/`
Entry-point principali:
- `README.md` — overview della suite
- `toDo.md` — backlog test
- `doc/testing_targets.md` e `doc/testing_theory_introduction.md` — strategia e obiettivi

Sottosezioni tipiche:
- `Data/` → setup e note su test con DbContext
- `Domain/` → test relativi a componenti del dominio
- `Model/` → test sulle entità/model

---

## 🧩 Dove mettere cosa (anti-obsolescenza)

Regola: **un documento = uno scopo**. Se un contenuto “stona” nel file corrente, va spostato.

- **Scelte architetturali globali, confini, dependency rule, AS-IS vs TO-BE**
  → `ARCHITECTURE.md`

- **Pattern, motivazioni, alternative scartate**
  → `DESIGN_PATTERNS.md`

- **Processo di lavoro**
  → `WORKFLOW.md`

- **Milestone e direzione (con TO-BE dichiarato)**
  → `ROADMAP.md`

- **Task concrete e verificabili**
  → `toDo.md` (globale) o `JokesApp.Server/toDo.md`, `JokesApp.Tests/toDo.md` (area)

- **Dettagli implementativi (manualistica)**
  → sempre nelle cartelle per area (`JokesApp.Server/`, `JokesApp.Client/`, `JokesApp.Tests/`)

---

## 🧷 Convenzioni dei file “di dettaglio” (Backend/Test)

Nella documentazione di area (es. backend/domain) i file sono spesso numerati per:
- favorire un ordine di lettura,
- raggruppare per categoria (es. eccezioni, VO, eventi…),
- rendere immediato capire “che tipo di cosa” stai leggendo.

Esempio tipico:
- `01_...` → eccezioni
- `03_...` → error messages
- `04a/04b_...` → value objects
- `05a/05b_...` → entità
- `06a/06b_...` → primitives / eventi
- `07_...` → DTO
- `0x_...` → validation/custom attributes

---

## ✅ Regole di qualità documentale (pratiche)

- **Truth-first**
  - Se è implementato oggi → scrivilo come **AS-IS**.
  - Se è pianificato → marcatura **TO-BE** (senza venderlo come già presente).

- **No duplicazioni**
  - Non copiare paragrafi tra documenti globali e documenti di area: si linka.

- **Coerenza con il codice**
  - Se cambia una cartella/namespace/responsabilità → aggiornare il documento “owner”.

- **Commit chirurgici**
  - Un file alla volta: `git add <file>` mirato.
  - Messaggio chiaro e coerente (es. `(docs): update doc hub`).

---

## 🔁 Manutenzione: quando aggiornare cosa

Aggiorna **sempre** il documento “owner” quando:
- cambi responsabilità di un layer/modulo,
- sposti file/cartelle o rinomini namespace pubblici,
- modifichi contratti API (DTO/endpoint),
- cambi bootstrap/config/middleware,
- modifichi strategia test o target di copertura.

Aggiorna **anche** i documenti globali solo se:
- cambia un vincolo architetturale,
- cambia una milestone o una direzione (ROADMAP),
- cambia il processo di lavoro (WORKFLOW).

---

## 🧾 Owner map (single source of truth)

| Se cerchi… | Documento “owner” |
|---|---|
| Confini/layer/AS-IS vs TO-BE | `ARCHITECTURE.md` |
| Pattern e motivazioni | `DESIGN_PATTERNS.md` |
| Regole operative (Git + doc) | `WORKFLOW.md` |
| Milestone e direzione | `ROADMAP.md` |
| Task verificabili | `toDo.md` |
| Sequenza operativa backend | `JokesApp.Server/TIMELINE.md` |
| Convenzioni (naming/codice/doc) | `CONVENTIONS.md` |

---

## 🔗 Link utili

- Torna all’entrypoint del repo: `../README.md`
- Architettura globale: `./ARCHITECTURE.md`
- Sequenza operativa server: `./JokesApp.Server/TIMELINE.md`

---