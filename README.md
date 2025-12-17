# 📘 JokesApp — Monorepo (React + ASP.NET Core)

Monorepo didattico/progettuale che integra:

- **JokesApp.Client** — frontend (React + Vite)
- **JokesApp.Server** — backend (ASP.NET Core Web API)
- **JokesApp.Tests** — test automatici (in evoluzione)
- **JokesApp.Doc** — documentazione estesa e tracciamento dello stato

Il backend è progettato con principi **Clean Architecture** e **Domain-Driven Design (DDD)**, mantenendo il dominio indipendente da HTTP/DB/framework.

---

## 🗂️ Repository layout (alto livello)

```text
/JokesApp
├─ .github/workflows/           # CI/CD (se presenti)
├─ JokesApp.Client/             # Frontend
├─ JokesApp.Server/             # Backend
├─ JokesApp.Tests/              # Test
├─ JokesApp.Doc/                # Documentazione (per area + stato)
├─ JokesApp.slnx
└─ README.md                    # Questo file
```

Link rapidi:

* [JokesApp.Client/](JokesApp.Client/)
* [JokesApp.Server/](JokesApp.Server/)
* [JokesApp.Tests/](JokesApp.Tests/)
* [JokesApp.Doc/](JokesApp.Doc/)

---

## 🔧 Prerequisiti

* **.NET SDK** (per `JokesApp.Server` e `JokesApp.Tests`)
* **Node.js + npm** (per `JokesApp.Client`)

> Nota “repo hygiene”: non versionare artefatti locali come `node_modules/`, `bin/`, `obj/`, `dist/`, `.env`, backup e file user-specific.
> Regole operative: [JokesApp.Doc/WORKFLOW.md](JokesApp.Doc/WORKFLOW.md)

---

## ▶️ Avvio rapido

### Backend

```bash
dotnet build JokesApp.slnx
dotnet run --project JokesApp.Server
```

#### Configurazione

La configurazione applicativa è gestita tramite:
* `JokesApp.Server/appsettings.json`
* `JokesApp.Server/appsettings.Development.json`
* eventuale `.env` locale

Dettagli e note operative: [JokesApp.Doc/JokesApp.Server/Program.md](JokesApp.Doc/JokesApp.Server/Program.md)

### Frontend

```bash
cd JokesApp.Client
npm install
npm run dev
```

### Test

```bash
dotnet test JokesApp.slnx
```

---

## 🧩 Documentazione: cosa leggere e in quale ordine

### Indice documentazione (punto di ingresso)

* [JokesApp.Doc/README.md](JokesApp.Doc/README.md)

### Decisioni e principi

* [JokesApp.Doc/ARCHITECTURE.md](JokesApp.Doc/ARCHITECTURE.md) — scelte architetturali, confini, regole
* [JokesApp.Doc/WORKFLOW.md](JokesApp.Doc/WORKFLOW.md) — workflow Git/monorepo e basi DevOps
* [JokesApp.Doc/ROADMAP.md](JokesApp.Doc/ROADMAP.md) — direzione e criteri di evoluzione

### Stato del progetto (fonte di verità operativa)

* [JokesApp.Doc/toDo.md](JokesApp.Doc/toDo.md) — cruscotto operativo (cosa esiste / cosa manca / cosa è chiuso)
* [JokesApp.Doc/JokesApp.Server/TIMELINE.md](JokesApp.Doc/JokesApp.Server/TIMELINE.md) — ordine di sviluppo step-by-step

> Regola: la documentazione deve essere **truth-first** (descrive ciò che esiste davvero nel repository).

---

## Convenzioni di progetto (sintesi)

* **Regola**: invarianti nel **Domain Layer**; nessuna dipendenza diretta da EF/HTTP.
* **Regola**: verso l’esterno usare **DTO** (non esporre direttamente entità/Value Object).
* **Regola**: la documentazione vive in `JokesApp.Doc/` ed è mantenuta allineata a codice e struttura.

---

## Contributi / Workflow Git

Se lavori con branch e PR (consigliato), segui il flusso definito in:

* [JokesApp.Doc/WORKFLOW.md](JokesApp.Doc/WORKFLOW.md)

---

