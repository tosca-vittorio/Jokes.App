# 📄 JokesApp.Doc/toDo.md

## 📚 C) DOCUMENTAZIONE (Doc)

### Legenda stati
- ✅ = completato e verificato
- 🟡 = presente ma da verificare/chiudere (parziale)
- ⬜ = da fare

### C1) Stato attuale
- ✅ Documentazione Domain (file-by-file) completata e allineata
- ✅ `JokesApp.Doc/ROADMAP.md` riallineata allo stato reale
- 🟡 README root (`README.md`) + `ARCHITECTURE.md`: ultimo pass di verifica (albero directory + riferimenti ai layer)

### C2) Da fare
- ⬜ Aggiornare albero directory nel README root (quando stabilizzi i layer)
- ⬜ Documentare Persistence (DbContext/mapping/migrations) dopo verifica (07a)
- ⬜ Documentare Application Layer (use cases) dopo implementazione
- ⬜ Documentare API (endpoint, error model, auth) quando i controller esistono
- ⬜ Diagramma architetturale aggiornato (opzionale ma consigliato)

---

## ⭐ Overview sintetica (verità ad oggi)

| Area | Stato | Note |
|---|---|---|
| Domain Layer | ✅ | Chiuso |
| Persistence | 🟡 | `JokesDbContext` + ValueConverters presenti ma da verificare/chiudere; migrations da rigenerare/applicare in 07a |
| DTO | 🟡 | Presenti ma da verificare/chiudere (non ancora agganciati a endpoint reali) |
| Application Layer | ⬜ | Da fare |
| Controllers/API | ⬜ | Da fare |
| Identity/JWT | ⬜ | Da fare |
| Client | ⬜ | Da fare (non verificato) |
| Testing | 🟡 | **Solution separata `JokesApp.Tests`**: unit test Domain presenti + doc-hub; mancano integration (Persistence/API) |
