# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04a_AnswerTextTests.md — Manuale didattico per `AnswerText`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/AnswerTextTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.AnswerTextTests`  
> **Soggetto:** Value Object `AnswerText` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `AnswerText` secondo il principio DDD “**valido per costruzione**”: un `AnswerText` può esistere **solo** se rispetta le regole del dominio. Di conseguenza, tutta la logica rilevante è concentrata nella factory:

- `AnswerText.Create(...)` deve **normalizzare** l’input (trim degli spazi laterali);
- deve **rifiutare** input non significativi (`null`, stringa vuota, solo whitespace);
- deve **enforzare** il vincolo di lunghezza massima (`MaxLength`);
- deve esporre proprietà coerenti con il valore normalizzato (`Value`, `IsEmpty`, `Length`).

In breve: questi test garantiscono che `AnswerText` non diventi mai un “contenitore permissivo” di stringhe arbitrarie, ma resti un **contratto** forte del dominio.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo (scenario unico, non parametrico).
- **`[Theory]`**: test parametrico, eseguito più volte con input diversi.
- **`[InlineData(...)]`**: fornisce i casi in ingresso alla `Theory` (in questa suite: `null`, `""`, `"   "`).

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto dei valori.
- **`.Should().BeFalse()` / `.Should().BeTrue()`**: assert booleani leggibili.
- **`.Throw<DomainValidationException>()`**: vincola il tipo di eccezione al contratto di validazione di dominio.
- **`.WithMessage(...)`**: vincola il messaggio d’errore (centralizzato in `JokeErrorMessages`, quindi stabile e “domain-driven”).
- **`.Which.MemberName`**: vincola il “campo logico” responsabile dell’errore (qui: `nameof(AnswerText)`), utile per diagnosi e coerenza degli errori.

### ✅ Lambda “act” per test di eccezioni
Nei test in cui ci si aspetta un’eccezione, l’operazione viene incapsulata in una lambda:

```csharp
var act = () => AnswerText.Create(value);
```

Questo è necessario perché FluentAssertions deve **invocare** l’azione e intercettare il `throw` in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `AnswerTextTests.cs`)

### 🧪 Test: `Create_ShouldReturnTrimmedValue_WhenInputIsValid`

**Scopo:** verificare che la factory:

1. normalizzi l’input (`Trim`),
2. produca un Value Object non vuoto,
3. esponga una lunghezza coerente con il valore normalizzato.

**Sintassi (estratto):**

```csharp
const string raw = "  An impasta!  ";

var answer = AnswerText.Create(raw);

answer.Value.Should().Be("An impasta!");
answer.IsEmpty.Should().BeFalse();
answer.Length.Should().Be("An impasta!".Length);
```

**Spiegazione:**

* `raw` contiene spazi iniziali e finali: è un input tipico “sporco” proveniente dall’esterno (UI, API, import).
* `AnswerText.Create(raw)` deve applicare **normalizzazione**: la regola qui è “tolgo gli spazi ai bordi”, non altero l’interno.
* `Value` deve risultare `"An impasta!"` (spazi laterali rimossi).
* `IsEmpty` deve essere `false`: dopo normalizzazione il testo è significativo.
* `Length` deve riflettere la lunghezza del **valore normalizzato**, non quella dell’input originale. Questo dettaglio è importante perché evita discrepanze tra ciò che il dominio “accetta” e ciò che il dominio “espone”.

**Aspettativa di dominio:** un `AnswerText` valido è sempre coerente e già pronto all’uso (non richiede ulteriori trim o controlli altrove).

---

### 🧪 Test: `Create_ShouldThrow_WhenValueIsNullOrWhitespace`

**Scopo:** impedire la creazione del Value Object quando l’input è:

* `null`,
* stringa vuota,
* solo whitespace.

**Sintassi (estratto):**

```csharp
var act = () => AnswerText.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.AnswerNullOrEmpty)
   .Which.MemberName.Should().Be(nameof(AnswerText));
```

**Nota di struttura (xUnit):**
Questo test è una **`[Theory]`** con **`[InlineData]`**:

* `null`
* `""`
* `"   "`

Quindi lo stesso contratto viene validato su più edge-case senza duplicare codice.

**Spiegazione:**

* Il dominio non accetta una “risposta” che non contenga contenuto semantico.
* Il test vincola tre aspetti fondamentali dell’errore:

  * **Tipo**: `DomainValidationException` (non eccezioni generiche).
  * **Messaggio**: `JokeErrorMessages.AnswerNullOrEmpty` (coerenza e diagnosi).
  * **MemberName**: `nameof(AnswerText)` (il fallimento è attribuito al Value Object come campo logico; scelta intenzionale per uniformare gli errori dei VO).

**Aspettativa di dominio:** “se esiste un `AnswerText`, allora ha sempre contenuto valido”; altrimenti l’oggetto non deve essere creato.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueExceedsMaxLength`

**Scopo:** impedire la creazione quando la lunghezza supera il limite massimo consentito dal dominio.

**Sintassi (estratto):**

```csharp
string tooLong = new('a', AnswerText.MaxLength + 1);

var act = () => AnswerText.Create(tooLong);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.AnswerTooLong)
   .Which.MemberName.Should().Be(nameof(AnswerText));
```

**Spiegazione:**

* `AnswerText.MaxLength` è il vincolo di dominio “hard”.
* `new('a', AnswerText.MaxLength + 1)` costruisce un input **sicuramente fuori limite** (pattern chiaro e deterministico).
* Anche qui, il test vincola:

  * **Tipo**: `DomainValidationException`,
  * **Messaggio**: `JokeErrorMessages.AnswerTooLong`,
  * **MemberName**: `nameof(AnswerText)`.

**Aspettativa di dominio:** il limite massimo è applicato **alla frontiera** (factory), impedendo che testi eccessivi entrino nel modello e “esplodano” più avanti (UI, persistence, logging, ecc.).

---

## 4️⃣ Conclusione

La suite `AnswerTextTests` certifica che `AnswerText` sia un Value Object:

* **normalizzato** (trim applicato all’ingresso),
* **non degenerabile** (mai `null`/empty/whitespace),
* **vincolato** da `MaxLength`,
* **coerente** nelle proprietà esposte (`Value`, `IsEmpty`, `Length`),
* **rigorosamente valido per costruzione**, in linea con DDD e con un dominio “fail-fast”.

Questo rende `AnswerText` un mattone affidabile: chi lo usa nel dominio può assumere che sia sempre corretto senza introdurre controlli duplicati altrove.

---
