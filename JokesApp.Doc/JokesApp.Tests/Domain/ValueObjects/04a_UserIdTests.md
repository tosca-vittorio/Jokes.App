# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04a_UserIdTests.md — Manuale didattico per `UserId`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/UserIdTests.cs`
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.UserIdTests`
> **Soggetto:** Value Object `UserId` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `UserId` secondo il principio DDD **“valido per costruzione”**: nel dominio, un `UserId` può esistere **solo** se rispetta tutte le invarianti previste. L’idea è tipica dei Value Object: **se l’oggetto esiste, allora è valido**; se non è valido, la costruzione deve fallire in modo *fail-fast* e diagnostico.

In particolare, i test garantiscono che la factory:

* `UserId.Create(...)` **normalizzi** l’input tramite `Trim()` (forma canonica dell’identificatore);
* rifiuti input non significativi: `null`, stringa vuota e stringhe composte solo da whitespace;
* faccia rispettare il vincolo di lunghezza massima tramite `UserId.MaxLength`;
* mantenga coerenti le proprietà del VO (`Value`, `IsEmpty`) rispetto al valore normalizzato.

Obiettivo pratico: impedire che nel dominio circolino identificativi “sporchi” o non inizializzati (es. `"  user-123  "` oppure `"   "`), evitando inconsistenze e bug a catena (persistenza, correlazioni, log, audit, eventi di dominio).

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit

* **`[Fact]`**: scenario singolo e atomico (una regola principale per test).
* **`[Theory]`** + **`[InlineData]`**: stessa regola validata su più input (edge-case) senza duplicare codice.

In questa suite:

* `[Fact]` viene usato per happy-path e per la regola di `MaxLength`;
* `[Theory]` copre i casi `null` / empty / whitespace.

### ✅ FluentAssertions

* **`.Should().Be(...)`**: confronto diretto e leggibile.
* **`.Should().BeFalse()`**: assert booleano semantico.
* **`.Throw<DomainValidationException>()`**: vincola la violazione a un errore **di dominio** (non eccezioni generiche).
* **`.WithMessage(...)`**: vincola il messaggio centralizzato in `ApplicationUserErrorMessages`.
* **`.Which.MemberName`**: vincola l’informazione diagnostica sul “campo logico” responsabile dell’errore (qui: `nameof(UserId)`).

### ✅ Lambda “act” per test di eccezioni

Per poter testare un `throw`, l’azione deve essere incapsulata in una lambda:

```csharp
var act = () => UserId.Create(value);
```

Questo permette a FluentAssertions di invocare l’azione e intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `UserIdTests.cs`)

### 🧪 Test: `Create_ShouldReturnTrimmedValue_WhenInputIsValid`

**Scopo:** verificare che la factory:

1. applichi correttamente il `Trim()` ai bordi,
2. esponga un `Value` già normalizzato,
3. mantenga `IsEmpty == false` per un ID valido.

**Sintassi (estratto):**

```csharp
const string raw = "  user-123  ";

var userId = UserId.Create(raw);

userId.Value.Should().Be("user-123");
userId.IsEmpty.Should().BeFalse();
```

**Spiegazione:**

* `raw` simula un input realistico proveniente dall’esterno (UI/API/import) con spazi “sporchi” ai bordi.
* `Create(raw)` deve produrre la **forma canonica**: l’ID che entra nel dominio deve essere già pronto, senza richiedere ulteriori `Trim()` altrove.
* Il test vincola due elementi complementari:

  * **`Value`** deve essere esattamente `"user-123"`;
  * **`IsEmpty`** deve essere `false` perché l’identificatore è presente e significativo.

**Aspettativa di dominio:** se `UserId` viene creato, allora è già coerente e usabile per correlazione, persistenza ed eventi.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueIsNullOrWhitespace`

**Scopo:** impedire la creazione del VO quando il valore è:

* `null`,
* stringa vuota,
* stringa composta solo da whitespace.

**Sintassi (estratto):**

```csharp
var act = () => UserId.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
   .Which.MemberName.Should().Be(nameof(UserId));
```

**Nota di struttura (xUnit):** questo test è una **`[Theory]`** con tre casi:

* `null`
* `""`
* `"   "`

**Spiegazione:**

* Un identificativo utente non può essere “non inizializzato”: `null`, empty e whitespace rappresentano tutti input **non significativi**.
* Il test vincola un contratto d’errore completo e ripetibile:

  1. **Tipo**: `DomainValidationException` (violazione di regole di dominio).
  2. **Messaggio**: `ApplicationUserErrorMessages.UserIdNullOrEmpty` (centralizzato e consistente).
  3. **MemberName**: `nameof(UserId)` (il fallimento viene attribuito al “campo logico” del VO).

**Aspettativa di dominio:** nel dominio non deve esistere mai un `UserId` privo di contenuto; se l’input è non valido, l’oggetto non deve nascere.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueExceedsMaxLength`

**Scopo:** impedire la creazione del VO quando la stringa supera `UserId.MaxLength`.

**Sintassi (estratto):**

```csharp
string tooLong = new('u', UserId.MaxLength + 1);

var act = () => UserId.Create(tooLong);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.UserIdTooLong)
   .Which.MemberName.Should().Be(nameof(UserId));
```

**Spiegazione:**

* `UserId.MaxLength` è un vincolo “hard” che protegge:

  * coerenza del dominio,
  * limiti di persistenza (colonne, indici),
  * sicurezza/robustezza (input incontrollati).
* `new('u', MaxLength + 1)` crea un input **deterministico** e sicuramente fuori limite.
* Anche qui il test vincola il contratto d’errore su:

  * eccezione di dominio,
  * messaggio centralizzato corretto,
  * `MemberName` coerente (`nameof(UserId)`).

**Aspettativa di dominio:** il limite massimo viene applicato **alla frontiera** (factory), evitando propagazione di dati ingestibili nel resto del sistema.

---

## 4️⃣ Conclusione

La suite `UserIdTests` certifica che `UserId` sia un Value Object:

* **normalizzato** (trim applicato all’ingresso → forma canonica dell’ID),
* **non degenerabile** (mai `null`/empty/whitespace),
* **vincolato** da `MaxLength`,
* **coerente** nelle proprietà esposte (`Value`, `IsEmpty`),
* **fail-fast** e diagnostico (eccezioni di dominio con messaggio e `MemberName` vincolati).

Questa suite è quindi **chiudibile** come riferimento stabile: è completa, deterministica e allineata 1:1 con `UserIdTests.cs`.

---
