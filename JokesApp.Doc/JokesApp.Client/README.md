# 📘 Frontend — JokesApp.Client (UI, routing, API services)

Entry-point della documentazione del **client React**. Descrive lo stato attuale (AS-IS), la struttura attesa (TO-BE) per UI e routing, e come organizzare i servizi HTTP verso la Web API.

> Regola: evitare duplicazioni con i documenti globali. Qui vanno i dettagli operativi del frontend.

---

## 🧭 Scope e stato
- **AS-IS:** scaffold Vite/React generato dal template “ASP.NET + React” (pagina `Weather forecast` con fetch diretto a `weatherforecast`).
- **TO-BE dichiarato:** SPA dedicata alle funzionalità Jokes, con routing client-side e servizi HTTP centralizzati.

---

## 🏗️ Struttura UI (AS-IS → TO-BE)
- **Mounting point:** `src/main.jsx` monta `<App />` (nessun router configurato).
- **Shell attuale:** `src/App.jsx` gestisce l’unica vista demo (tabella meteo).
- **Struttura consigliata (TO-BE, da creare):**
  - `src/routes/` → definizione delle route React Router (layout + nested routes).
  - `src/pages/` → pagine di alto livello (`Home`, `JokesList`, `JokeDetail`, `About`…).
  - `src/components/` → componenti riutilizzabili (layout, navbar/footer, cards, form).
  - `src/styles/` → styling modulare o CSS-in-JS secondo convenzioni di progetto.
  - `src/hooks/` → hook condivisi (es. gestione fetch, stato loading/error).
  - `src/services/` → client HTTP e servizi API (vedi sezione dedicata).

---

## 🗺️ Routing previsto (TO-BE)
Obiettivo: introdurre **React Router** con layout applicativo e route annidate.
- Route base: `/` → landing/overview.
- Area Jokes:
  - `/jokes` → lista/browse con filtri/paginazione (TO-BE).
  - `/jokes/:id` → dettaglio (TO-BE).
  - `/jokes/new` → creazione (TO-BE, protetta se/quando esiste auth).
- Rotte tecniche:
  - `/health` o `/status` (opzionale) per verificare integrazione client-server.
  - Fallback `*` → pagina 404 coerente.

> Nota: finché il router non è introdotto, le rotte sopra sono **pianificate**, non ancora implementate.

---

## 🔌 Servizi API e chiamate HTTP
- **AS-IS:** fetch diretto dentro `App.jsx` verso `weatherforecast`.
- **TO-BE:** centralizzare i client in `src/services/` per isolare le chiamate HTTP:
  - `httpClient.ts|js` → wrapper generico (`fetch`/`axios`, base URL da `import.meta.env.VITE_API_BASE_URL`).
  - `jokesService.ts|js` → funzioni di dominio (`getJokes`, `getJoke(id)`, `createJoke(payload)`…).
  - Eventuali adapter per mapping DTO ↔︎ view-model.
- **Regole pratiche:**
  - Niente fetch “inline” nei componenti di pagina; usare servizi o hook dedicati.
  - Propagare gli stati `loading/error` dai servizi agli hook/UI.
  - DTO e contratti devono restare allineati con `JokesApp.Doc/JokesApp.Server/DTOs/`.

---

## 🧭 Convenzioni (locali al frontend)
- Naming React: componenti `PascalCase`, hook `useX`, file `kebab-case` o `camelCase` coerente con `JokesApp.Doc/CONVENTIONS.md`.
- Directory per feature accettata se/quando le pagine crescono (`src/features/jokes/…`), mantenendo servizi condivisi in `src/services/`.
- No duplicazioni di concetti architetturali già coperti in `ARCHITECTURE.md`: qui solo note operative UI/API.
- Annotare AS-IS vs TO-BE quando si documentano funzionalità non ancora presenti nel codice.