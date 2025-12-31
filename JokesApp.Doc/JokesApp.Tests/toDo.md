# 📄 JokesApp.Doc/JokesApp.Tests/toDo.md

Documentazione centralizzata delle attività concluse, in corso o da implementare per completare **JokesApp.Tests** secondo:
- Clean Architecture + DDD
- SOLID / DRY / KISS / YAGNI
- documentazione “truth-first” (descrive solo ciò che esiste davvero)

---

## ✅ Legenda stati
- ✅ **Fatto / Chiuso (verificato e allineato a codice + doc)**
- 🟡 **Presente / In corso (esiste nel repo ma non ancora verificato/chiuso)**
- ⬜ **Da fare**

---

## A8) Testing (Solution separata: `JokesApp.Tests`) 🟡

**Scopo:** validare il **Domain Layer** e, progressivamente, la Persistence (EF Core) e l’API, senza “sporcare” la timeline del Server.

**Stato attuale (truth-first):**
- esiste un **doc-hub dedicato** in `JokesApp.Doc/JokesApp.Tests/` (README + timeline + toDo + teoria/target + matrice);
- sono presenti  **unit test di dominio** (Value Objects / Entities / Events) e test di **validation** (CustomEmailAttribute);
- sono ancora da impostare i test di **integrazione**:
  - Persistence (dipende da **Server 07a**),
  - API (dipende da **Server 10**).

> Regola di coerenza: per ogni test “ufficiale” deve esistere **sia** il file `.cs` in `JokesApp.Tests`
> **sia** il corrispondente `.md` nel doc-hub (`JokesApp.Doc/JokesApp.Tests`).

---

### A8.1 — Doc-hub & matrice target ✅
- ✅ `JokesApp.Doc/JokesApp.Tests/README.md`
- ✅ `JokesApp.Doc/JokesApp.Tests/TIMELINE.md`
- ✅ `JokesApp.Doc/JokesApp.Tests/toDo.md`
- ✅ `JokesApp.Doc/JokesApp.Tests/testing_theory_introduction.md`
- ✅ `JokesApp.Doc/JokesApp.Tests/testing_targets.md`
- ✅ `JokesApp.Doc/JokesApp.Tests/Domain/01_DomainTestMatrix.md`

---

### A8.2 — Unit tests Domain (baseline) 🟡

**Value Objects** 🟡
- 🟡 `AnswerTextTests` (04a_AnswerText)
- 🟡 `QuestionTextTests` (04a_QuestionText)
- 🟡 `JokeIdTests` (04a_JokeId)
- 🟡 `UserIdTests` (04a_UserId)
- 🟡 `AvatarUrlTests` (04b_AvatarUrl)
- 🟡 `DisplayNameTests` (04b_DisplayName)
- 🟡 `EmailAddressTests` (04b_EmailAddress)

**Entities** 🟡
- 🟡 `JokeTests` (05a_Joke)
- 🟡 `ApplicationUserTests` (05b_ApplicationUser)

**Events** 🟡
- 🟡 `JokeWasCreatedTests` (06_JokeWasCreated)
- 🟡 `JokeWasUpdatedTests` (06_JokeWasUpdated)
- 🟡 `JokeWasLikedTests` (06_JokeWasLiked)
- 🟡 `JokeWasUnlikedTests` (06_JokeWasUnliked)

**Validation / Attributes** 🟡
- 🟡 `CustomEmailAttributeTests`

---

### A8.3 — Integration tests Persistence (EF Core) ⬜
> Prerequisito: **Server 07a chiuso** (DbContext + mapping + migrations verificati su PostgreSQL).

- ⬜ smoke test: creazione `DbContext` + apertura connessione
- ⬜ test mapping/converters (VO ↔ DB) + roundtrip
- ⬜ test migrazioni su DB vuoto / DB “pulito”
- ⬜ test relazione `ApplicationUser (1) → (N) Jokes` + constraints essenziali

---

### A8.4 — Integration tests API ⬜

- ⬜ smoke test endpoint
- ⬜ test `JokesController` / `UsersController` / `AuthController`
- ⬜ test auth (JWT) quando attivo

---

## ✅ Definition of Done (baseline Testing) ⬜
- ⬜ `dotnet test` green sulla solution `JokesApp.Tests`
- ⬜ doc-hub allineato (TIMELINE + toDo + matrice target)
- ⬜ convenzione `*.cs` ↔ `*.md` rispettata per i test “ufficiali”

---
