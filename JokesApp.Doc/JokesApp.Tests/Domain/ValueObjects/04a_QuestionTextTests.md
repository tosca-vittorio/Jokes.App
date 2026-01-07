# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04a_QuestionTextTests.md — Manuale didattico per `QuestionText`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/QuestionTextTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.QuestionTextTests`  
> **Soggetto:** Value Object `QuestionText` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `QuestionText` applicando in modo rigoroso il principio DDD **“valido per costruzione”**: un `QuestionText` può esistere nel dominio **solo** se rispetta tutte le invarianti richieste.

In particolare, i test assicurano che la factory:

- `QuestionText.Create(...)` **normalizzi** l’input con `Trim()` (forma canonica del testo);
- rifiuti input non significativi: `null`, stringa vuota e stringhe composte solo da whitespace;
- faccia rispettare il vincolo di lunghezza massima esposto da `QuestionText.MaxLength`;
- mantenga coerenti le proprietà derivate (`Value`, `IsEmpty`, `Length`) rispetto al valore normalizzato.

L’obiettivo complessivo è impedire che nel dominio entri una domanda “sporca” o incoerente, evitando:
- differenze tra ciò che viene salvato e ciò che viene mostrato;
- controlli duplicati sparsi tra UI/API/persistence;
- bug silenziosi dovuti a testi vuoti o non inizializzati.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo e atomico (una regola principale per test).
- **`[Theory]`** + **`[InlineData]`**: la stessa regola validata su più input (edge-case) senza duplicare codice.

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto e leggibile tra valore atteso e valore reale.
- **`.Should().BeFalse()`**: assert booleani semantici.
- **`.Throw<DomainValidationException>()`**: vincola il tipo di eccezione al contratto di validazione di dominio.
- **`.WithMessage(...)`**: vincola il messaggio d’errore (centralizzato in `JokeErrorMessages`).
- **`.Which.MemberName`**: vincola l’informazione diagnostica sul “campo logico” responsabile dell’errore  
  (in questa suite: `nameof(QuestionText)`).

### ✅ Lambda “act” per test di eccezioni
Quando ci si aspetta un `throw`, l’azione viene incapsulata in una lambda:

```csharp
var act = () => QuestionText.Create(value);
```

Questo è necessario perché FluentAssertions deve poter **invocare** l’azione e intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `QuestionTextTests.cs`)

### 🧪 Test: `Create_ShouldReturnTrimmedValue_WhenInputIsValid`

**Scopo:** verificare che la factory:

1. applichi correttamente il `Trim()` agli spazi laterali,
2. produca un VO non vuoto,
3. esponga `Length` coerente con il valore normalizzato.

**Sintassi (estratto):**

```csharp
const string raw = "  What do you call a fake noodle?  ";

var question = QuestionText.Create(raw);

question.Value.Should().Be("What do you call a fake noodle?");
question.IsEmpty.Should().BeFalse();
question.Length.Should().Be("What do you call a fake noodle?".Length);
```

**Spiegazione:**

* `raw` simula un input realistico proveniente dall’esterno (UI, API, import) con spazi “sporchi” ai bordi.
* `Create(raw)` deve costruire la **forma canonica**: rimuove gli spazi iniziali/finali ma non altera il contenuto interno.
* Il test vincola tre aspetti fondamentali:

  * **`Value`** deve essere il testo “ripulito”;
  * **`IsEmpty`** deve risultare `false` (dopo normalizzazione la stringa è significativa);
  * **`Length`** deve riflettere la lunghezza del valore canonico, non dell’input grezzo.

**Aspettativa di dominio:** chi usa `QuestionText` non deve mai preoccuparsi di fare trim o controlli “a valle”: se l’oggetto esiste, è già corretto.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueIsNullOrWhitespace`

**Scopo:** impedire la creazione del VO se il valore è:

* `null`,
* stringa vuota,
* solo whitespace.

**Sintassi (estratto):**

```csharp
var act = () => QuestionText.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.QuestionNullOrEmpty)
   .Which.MemberName.Should().Be(nameof(QuestionText));
```

**Nota di struttura (xUnit):** questo test è una **`[Theory]`** con tre casi:

* `null`
* `""`
* `"   "`

**Spiegazione:**

* Anche se l’input fosse `"   "`, dopo `Trim()` diventerebbe `""`: il dominio deve trattarlo come **non significativo**.
* Il test vincola l’errore su tre dimensioni (contratto completo):

  1. **Tipo**: `DomainValidationException` (errore di dominio, non generico)
  2. **Messaggio**: `JokeErrorMessages.QuestionNullOrEmpty` (centralizzato e coerente)
  3. **MemberName**: `nameof(QuestionText)` (il “campo logico” responsabile è il VO stesso)

**Aspettativa di dominio:** “se esiste una domanda nel dominio, allora è sempre presente e significativa”.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueExceedsMaxLength`

**Scopo:** impedire la creazione se il testo supera la soglia massima `QuestionText.MaxLength`.

**Sintassi (estratto):**

```csharp
string tooLong = new('q', QuestionText.MaxLength + 1);

var act = () => QuestionText.Create(tooLong);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.QuestionTooLong)
   .Which.MemberName.Should().Be(nameof(QuestionText));
```

**Spiegazione:**

* `QuestionText.MaxLength` rappresenta un vincolo “hard” di dominio.
* `new('q', MaxLength + 1)` costruisce un input **deterministico** e sicuramente fuori limite.
* Anche qui il test vincola il contratto d’errore:

  * eccezione di dominio,
  * messaggio corretto,
  * `MemberName` coerente.

**Aspettativa di dominio:** il limite massimo viene applicato **alla frontiera** (factory), prevenendo la propagazione di dati ingestibili o non persistibili.

---

## 4️⃣ Conclusione

La suite `QuestionTextTests` certifica che `QuestionText` sia un Value Object:

* **normalizzato** (trim applicato all’ingresso → forma canonica),
* **non degenerabile** (mai `null`/empty/whitespace),
* **vincolato** da `MaxLength`,
* **coerente** nelle proprietà esposte (`Value`, `IsEmpty`, `Length`),
* **fail-fast** e diagnostico (eccezioni di dominio con messaggio e `MemberName` vincolati).

Questo rende `QuestionText` un componente affidabile e riusabile in tutto il dominio: chi lo usa può assumere correttezza e coerenza senza introdurre controlli duplicati.

---
