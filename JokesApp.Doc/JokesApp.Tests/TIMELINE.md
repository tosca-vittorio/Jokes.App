# JokesApp.Doc/JokesApp.Tests/TIMELINE.md

## Flow Update pratico (Tests)

* **TIMELINE.md = ordine di sviluppo** e stato globale per step.
* **toDo.md = cruscotto operativo** (task anche non in ordine, ma tracciate).

### Legenda stati
- ✅ = completato e verificato
- 🟡 = presente ma da verificare/chiudere (parziale)
- ⬜ = da fare

### Regola di coerenza (truth-first)
Per ogni test “ufficiale” deve esistere **sia**:
- `JokesApp.Tests/.../<Nome>Tests.cs`
- `JokesApp.Doc/JokesApp.Tests/.../<Nome>Tests.md`

---

## 📌 TIMELINE TESTS (sequenza completa aggiornata T00 → T08)

```md
T00 - Setup solution tests + convenzioni 🟡
      ├─ xUnit + FluentAssertions (packages, references, using) 🟡
      ├─ naming conventions (stile `*Tests.cs`) 🟡
      ├─ struttura cartelle (es. Domain/Validation/...) 🟡
      └─ `dotnet test` baseline green 🟡

T01 - Doc-hub tests (fonte di verità) ✅
      ├─ README.md ✅
      ├─ TIMELINE.md ✅
      ├─ toDo.md ✅
      ├─ testing_theory_introduction.md ✅
      ├─ testing_targets.md ✅
      └─ Domain/01_DomainTestMatrix.md ✅

T02 - Unit Tests: Domain Value Objects 🟡
      ├─ AnswerTextTests (04a_AnswerText) 🟡
      ├─ QuestionTextTests (04a_QuestionText) 🟡
      ├─ JokeIdTests (04a_JokeId) 🟡
      ├─ UserIdTests (04a_UserId) 🟡
      ├─ AvatarUrlTests (04b_AvatarUrl) 🟡
      ├─ DisplayNameTests (04b_DisplayName) 🟡
      └─ EmailAddressTests (04b_EmailAddress) 🟡

T03 - Unit Tests: Domain Entities 🟡
      ├─ JokeTests (05a_Joke) 🟡
      └─ ApplicationUserTests (05b_ApplicationUser) 🟡

T04 - Unit Tests: Domain Events 🟡
      ├─ JokeWasCreatedTests (06_JokeWasCreated) 🟡
      ├─ JokeWasUpdatedTests (06_JokeWasUpdated) 🟡
      ├─ JokeWasLikedTests (06_JokeWasLiked) 🟡
      └─ JokeWasUnlikedTests (06_JokeWasUnliked) 🟡

T05 - Unit Tests: Validation / Attributes 🟡
      └─ CustomEmailAttributeTests 🟡

T06 - Integration Tests: Persistence (EF Core) ⬜
 > Prerequisito: Server 07a chiuso (DbContext + mapping + migrations verificati su PostgreSQL).
      ├─ DbContext creation + smoke connection ⬜
      ├─ Converters roundtrip (VO ↔ DB) ⬜
      ├─ Migrations apply su DB vuoto ⬜
      └─ Relazioni + constraints (User 1→N Jokes) ⬜

T07 - Integration Tests: API ⬜
      ├─ smoke endpoints ⬜
      ├─ controllers reali ⬜
      └─ auth pipeline (JWT) ⬜

T08 - E2E / Smoke pipeline ⬜
      └─ test minimi end-to-end sul flusso principale ⬜
```

---
