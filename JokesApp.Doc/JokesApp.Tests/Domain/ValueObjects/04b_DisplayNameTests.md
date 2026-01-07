# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04b_DisplayNameTests — Manuale didattico per `DisplayName`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/DisplayNameTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.DisplayNameTests`  
> **Soggetto:** Value Object `DisplayName` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `DisplayName` applicando il principio DDD **“valido per costruzione”**: nel dominio un `DisplayName` può esistere **solo** se rispetta tutte le invarianti richieste. Se l’input è invalido, la costruzione deve fallire in modo **fail-fast** e con un errore **diagnostico** (eccezione di dominio + messaggio centralizzato + `MemberName` coerente).

In particolare, i test assicurano che la factory:

- `DisplayName.Create(...)` **normalizzi** l’input tramite `Trim()` (forma canonica del nome mostrato);
- rifiuti input **non significativi**: `null`, stringa vuota e stringhe composte solo da whitespace;
- faccia rispettare il vincolo di lunghezza massima tramite `DisplayName.MaxLength`;
- mantenga coerenti le proprietà del VO (`Value`, `Length`, `IsEmpty`) rispetto al valore normalizzato.

Obiettivo pratico: evitare nomi “sporchi” o non inizializzati nel dominio, prevenendo inconsistenze tra UI/API/persistence e riducendo controlli duplicati “a valle”.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit

- **`[Fact]`**: scenario singolo e atomico (una regola principale per test).
- **`[Theory]`** + **`[InlineData]`**: stessa regola validata su più input (edge-case) senza duplicare codice.

In questa suite:

- `[Fact]` viene usato per happy-path e per la regola di `MaxLength`;
- `[Theory]` copre i casi `null` / empty / whitespace.

### ✅ FluentAssertions

- **`.Should().Be(...)`**: confronto diretto e leggibile tra valore atteso e valore reale.
- **`.Should().BeFalse()`**: assert booleano semantico (qui: `IsEmpty`).
- **`.Throw<DomainValidationException>()`**: vincola la violazione a un errore **di dominio**.
- **`.WithMessage(...)`**: vincola il messaggio d’errore centralizzato in `ApplicationUserErrorMessages`.
- **`.Which.MemberName`**: vincola l’informazione diagnostica sul “campo logico” responsabile dell’errore (qui: `nameof(DisplayName)`).

### ✅ Lambda “act” per test di eccezioni

Per poter testare un `throw`, l’azione deve essere incapsulata in una lambda:

```csharp
var act = () => DisplayName.Create(value);
```

Questo permette a FluentAssertions di invocare l’azione e intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `DisplayNameTests.cs`)

### 🧪 Test: `Create_ShouldReturnTrimmedDisplayName_WhenValueIsValid`

**Scopo:** verificare che la factory:

1. applichi correttamente il `Trim()` ai bordi,
2. esponga un `Value` già normalizzato,
3. mantenga `Length` coerente col valore normalizzato,
4. mantenga `IsEmpty == false` per un valore valido.

**Sintassi (estratto):**

```csharp
const string rawValue = "  Ada Lovelace  ";

var displayName = DisplayName.Create(rawValue);

displayName.Value.Should().Be("Ada Lovelace");
displayName.Length.Should().Be("Ada Lovelace".Length);
displayName.IsEmpty.Should().BeFalse();
```

**Spiegazione:**

* `rawValue` simula un input realistico “sporco” (spazi laterali) proveniente dall’esterno.
* `Create(rawValue)` deve produrre la **forma canonica**: elimina gli spazi iniziali/finali ma non altera il contenuto interno.
* Il test vincola tre aspetti complementari:

  * **`Value`** deve essere esattamente `"Ada Lovelace"`;
  * **`Length`** deve riflettere la lunghezza del valore canonico, non dell’input grezzo;
  * **`IsEmpty`** deve essere `false` perché il nome è presente e significativo.

**Aspettativa di dominio:** se un `DisplayName` esiste, allora è già pulito e utilizzabile senza ulteriori normalizzazioni.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueIsNullOrWhitespace`

**Scopo:** impedire la creazione del VO quando il valore è:

* `null`,
* stringa vuota,
* stringa composta solo da whitespace.

**Sintassi (estratto):**

```csharp
var act = () => DisplayName.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
   .Which.MemberName.Should().Be(nameof(DisplayName));
```

**Nota di struttura (xUnit):** questo test è una **`[Theory]`** con tre casi:

* `null`
* `""`
* `"   "`

**Spiegazione:**

* Anche se l’input fosse `"   "`, dopo `Trim()` diventerebbe `""`: il dominio deve trattarlo come **non significativo**.
* Il test vincola il contratto d’errore su tre dimensioni:

  1. **Tipo**: `DomainValidationException` (violazione di regole di dominio).
  2. **Messaggio**: `ApplicationUserErrorMessages.DisplayNameRequired` (centralizzato e coerente).
  3. **MemberName**: `nameof(DisplayName)` (attribuzione diagnostica al “campo logico” del VO).

**Aspettativa di dominio:** nel dominio non deve esistere mai un `DisplayName` privo di contenuto; se l’input è invalido, l’oggetto non deve nascere.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueExceedsMaxLength`

**Scopo:** impedire la creazione del VO quando la stringa supera `DisplayName.MaxLength`.

**Sintassi (estratto):**

```csharp
string tooLong = new('a', DisplayName.MaxLength + 1);

var act = () => DisplayName.Create(tooLong);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.DisplayNameMaxLength)
   .Which.MemberName.Should().Be(nameof(DisplayName));
```

**Spiegazione:**

* `DisplayName.MaxLength` è un vincolo “hard” che protegge:

  * coerenza del dominio,
  * limiti di persistenza (colonne, indici),
  * robustezza verso input incontrollati.
* `new('a', MaxLength + 1)` crea un input **deterministico** e sicuramente fuori limite.
* Anche qui il test vincola il contratto d’errore:

  * eccezione di dominio,
  * messaggio centralizzato corretto,
  * `MemberName` coerente (`nameof(DisplayName)`).

**Aspettativa di dominio:** il limite massimo viene applicato **alla frontiera** (factory), evitando propagazione di dati ingestibili nel resto del sistema.

---

## 4️⃣ Conclusione

La suite `DisplayNameTests` certifica che `DisplayName` sia un Value Object:

* **normalizzato** (trim applicato all’ingresso → forma canonica),
* **non degenerabile** (mai `null`/empty/whitespace),
* **vincolato** da `MaxLength`,
* **coerente** nelle proprietà esposte (`Value`, `Length`, `IsEmpty`),
* **fail-fast** e diagnostico (eccezioni di dominio con messaggio e `MemberName` vincolati).

È quindi una documentazione **chiudibile**: completa, deterministica e allineata 1:1 con `DisplayNameTests.cs`.

---