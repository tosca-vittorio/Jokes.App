# 📘 JokesApp — Monorepo (React + Vite · ASP.NET Core Web API · xUnit)

Monorepo didattico/progettuale che integra:

- **JokesApp.Client** — frontend (React + Vite)
- **JokesApp.Server** — backend (ASP.NET Core Web API)
- **JokesApp.Tests** — test automatici (xUnit)
- **JokesApp.Doc** — documentazione estesa e tracciamento dello stato 

Il backend segue principi **Clean Architecture** e **Domain-Driven Design (DDD)**, mantenendo il dominio indipendente da HTTP/DB/framework.

> Questo README è l’**entrypoint** del repository. 

### Owner map (single source of truth)

| Se cerchi…                           | Documento owner                                |
| ------------------------------------ | ---------------------------------------------- |
| Architettura globale (AS-IS / TO-BE) | `JokesApp.Doc/ARCHITECTURE.md`                 |
| Pattern e motivazioni                | `JokesApp.Doc/DESIGN_PATTERNS.md`              |
| Regole operative                     | `JokesApp.Doc/WORKFLOW.md`                     |
| Milestone e direzione                | `JokesApp.Doc/ROADMAP.md`                      |
| Task verificabili                    | `JokesApp.Doc/toDo.md`                         |
| Sequenza operativa backend           | `JokesApp.Doc/JokesApp.Server/TIMELINE.md`     |

---

## 🗂️ Repository layout (alto livello)

```text
/JokesApp
├─ .github/workflows/           # CI/CD (GitHub Actions)
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

## 🧱 Architettura in breve

React SPA (**JokesApp.Client**) ↔ ASP.NET Core Web API (**JokesApp.Server**).

Nel backend:

* **Domain**: invarianti, Value Objects, Aggregate Root, Domain Events, Domain Exceptions
* **Application**: orchestrazione dei casi d’uso (in evoluzione)
* **Infrastructure**: persistenza e adapter tecnici
* **Presentation (API)**: controller + DTO + HTTP concerns

Dettagli e scelte: [JokesApp.Doc/ARCHITECTURE.md](JokesApp.Doc/ARCHITECTURE.md)

---

## 🔎 Entry-point utili

* Server changelog: [JokesApp.Server/CHANGELOG.md](JokesApp.Server/CHANGELOG.md)
* Client changelog: [JokesApp.Client/CHANGELOG.md](JokesApp.Client/CHANGELOG.md)
* API scratch file: [JokesApp.Server/JokesApp.Server.http](JokesApp.Server/JokesApp.Server.http)

---

## 🔧 Prerequisiti

* **.NET SDK** (per `JokesApp.Server` e `JokesApp.Tests`)
* **Node.js + npm** (per `JokesApp.Client`)

> Repo hygiene: non versionare artefatti locali come `node_modules/`, `bin/`, `obj/`, `dist/`, file `.env` con segreti, backup e file user-specific.

---

## ▶️ Avvio rapido

### Backend

```bash
dotnet build JokesApp.slnx
dotnet run --project JokesApp.Server
```

#### Configurazione

La configurazione è gestita tramite:

* `JokesApp.Server/appsettings.json` (versionato)
* `JokesApp.Server/appsettings.Development.json` (locale, ignorato da git)
* variabili d’ambiente (consigliato per valori sensibili)

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

## 🧩 Documentazione

* **Hub documentazione:** [JokesApp.Doc/README.md](JokesApp.Doc/README.md)

Documenti chiave:

* [JokesApp.Doc/ARCHITECTURE.md](JokesApp.Doc/ARCHITECTURE.md) — scelte architetturali, confini, regole
* [JokesApp.Doc/DESIGN_PATTERNS.md](JokesApp.Doc/DESIGN_PATTERNS.md) — pattern adottati e motivazioni
* [JokesApp.Doc/WORKFLOW.md](JokesApp.Doc/WORKFLOW.md) — workflow Git/monorepo e basi DevOps
* [JokesApp.Doc/ROADMAP.md](JokesApp.Doc/ROADMAP.md) — direzione e criteri di evoluzione
* [JokesApp.Doc/toDo.md](JokesApp.Doc/toDo.md) — cruscotto operativo (fonte di verità)
* [JokesApp.Doc/JokesApp.Server/TIMELINE.md](JokesApp.Doc/JokesApp.Server/TIMELINE.md) — ordine di sviluppo step-by-step (Server)

> Regola: la documentazione deve essere **truth-first** (descrive ciò che esiste davvero nel repository).

---

## 📐 Convenzioni di progetto (sintesi)

* Invarianti nel **Domain Layer**; nessuna dipendenza diretta da EF/HTTP.
* Verso l’esterno usare **DTO** (non esporre direttamente entità/Value Object).
* La documentazione vive in `JokesApp.Doc/` ed è mantenuta allineata a codice e struttura.

---

## 🤝 Contributi / Workflow Git

Se lavori con branch e PR, segui:

* [JokesApp.Doc/WORKFLOW.md](JokesApp.Doc/WORKFLOW.md)

---
