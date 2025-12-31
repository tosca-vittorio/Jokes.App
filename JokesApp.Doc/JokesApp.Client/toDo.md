# 📄 JokesApp.Doc/JokesApp.Client/toDo.md

Documentazione centralizzata delle attività concluse, in corso o da implementare per completare **JokesApp.Client** secondo:
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
| Scaffold FE | 🟡 | Progetto presente (React + Vite), ma non verificato/organizzato file-by-file |
| Routing | ⬜ | Non risulta implementato |
| Chiamate API | ⬜ | Nessun client HTTP/API wiring verificato |
| Auth UI + token | ⬜ | Dipende da pipeline JWT del Server |
| UI Jokes (MVP) | ⬜ | Dipende da controllers reali e DTO/API contract |
| Testing FE | ⬜ | Non impostato |

> Nota: al momento il focus del progetto è sul **backend**. Questo file mantiene il backlog FE, senza forzare scelte premature.

---

# 🌐 CLIENT (FrontEnd)

## 0) Stato attuale (AS-IS) 🟡
- 🟡 Progetto Client presente nel repo (React + Vite), considerato **scaffold**.
- ⬜ Non verificati: routing, integrazione API, auth, testing.

---

## 1) Prerequisiti minimi lato Server (dipendenze) ⬜
Prima di iniziare seriamente il Client, è consigliato avere:
- ⬜ Server **10** avviabile con **controllers reali** + error model stabile (es. `ProblemDetails`)
- ⬜ Contratto API minimo per Jokes (DTO + endpoint)
- ⬜ Se serve auth: pipeline **JWT** (login/register) almeno baseline

---

## 2) Setup base Client (quando si riparte) ⬜
- ⬜ Verifica/organizzazione struttura progetto (cartelle, convenzioni, lint/format se presenti)
- ⬜ Routing client-side (pagine minime: Home, Jokes, Login/Register)
- ⬜ Gestione config env (base URL API, modalità dev/prod)
- ⬜ Client HTTP centralizzato + gestione errori standard (scelta tool da fare quando serve)

---

## 3) Auth (dipende da JWT Server) ⬜
- ⬜ UI Login / Register
- ⬜ Gestione token (storage + attach a request) + route protection
- ⬜ Hook/service `useAuth` (o equivalente) + logout

---

## 4) Jokes UI (MVP) ⬜
- ⬜ Lista jokes (minimo: list + detail)
- ⬜ Create/Update joke
- ⬜ Like/Unlike (inizialmente senza realtime)

---

## 5) Profile UI ⬜
- ⬜ Profilo utente (display name / avatar / email)
- ⬜ Update profile

---

## 6) Realtime (SignalR) — backlog ⬜
- ⬜ Client SignalR + gestione eventi
- ⬜ UX feedback (toast/badge) + aggiornamento live

---

## 7) Testing FE ⬜
- ⬜ Unit test componenti/hook (scelta tool quando il setup è stabile)
- ⬜ E2E (Playwright/Cypress) sul flusso principale
