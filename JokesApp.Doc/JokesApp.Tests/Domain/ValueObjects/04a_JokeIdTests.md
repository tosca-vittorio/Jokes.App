# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04a_JokeIdTests.md — Manuale didattico per `JokeId`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/JokeIdTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.JokeIdTests`  
> **Soggetto:** Value Object `JokeId` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `JokeId` applicando il principio DDD **“valido per costruzione”**: se un `JokeId` esiste nel dominio, allora rappresenta **sempre** un’identità valida e **mai** un valore sentinella.

In particolare, i test assicurano che:

- `JokeId.New()` generi un identificatore **non vuoto** (quindi non `Guid.Empty`);
- `JokeId.Create(Guid)` accetti **solo** Guid validi e preservi esattamente il valore passato;
- `Guid.Empty` venga trattato come **input invalido** e produca una `DomainValidationException` con:
  - messaggio centralizzato nel dominio (`JokeErrorMessages.JokeIdEmpty`);
  - `MemberName` coerente e tracciabile (`nameof(JokeId)`).

L’obiettivo complessivo è impedire che nel dominio circolino ID “non inizializzati”, evitando bug silenziosi (es. update su entità sbagliate, eventi con ID non tracciabili, record non correlabili in persistence).

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo, non parametrico.  
  In questa suite ogni test rappresenta una regola precisa e atomica (una singola aspettativa principale).

### ✅ FluentAssertions
- **`.Should().Be(...)` / `.Should().NotBe(...)`**: confronto diretto, leggibile e semantico.
- **`.Throw<DomainValidationException>()`**: vincola il tipo di eccezione al contratto di validazione di dominio (niente eccezioni generiche).
- **`.WithMessage(...)`**: vincola il messaggio d’errore (centralizzato nel dominio, quindi stabile).
- **`.Which.MemberName`**: vincola l’informazione di diagnosi associata all’errore.  
  Qui la suite richiede esplicitamente `nameof(JokeId)` per mantenere coerenza nell’ecosistema dei Value Object.

### ✅ Lambda “act” per test di eccezioni
Per testare un’eccezione, l’operazione va incapsulata in una lambda:

```csharp
var act = () => JokeId.Create(Guid.Empty);
```

Questo serve perché FluentAssertions deve poter **invocare** l’azione e intercettare deterministically il `throw`.

---

## 3️⃣ Test documentati (1:1 con `JokeIdTests.cs`)

### 🧪 Test: `New_ShouldCreateNonEmptyId`

**Scopo:** verificare che `JokeId.New()` non produca mai il valore sentinella `Guid.Empty` e che lo stato del Value Object sia coerente (tramite `IsEmpty`).

**Sintassi (estratto):**

```csharp
var id = JokeId.New();

id.Value.Should().NotBe(Guid.Empty);
id.IsEmpty.Should().BeFalse();
```

**Spiegazione:**

* `Guid.Empty` (tutti zeri) è spesso usato come sentinella per indicare “non inizializzato”. Nel dominio questo è pericoloso perché può mascherare errori: un’entità “sembra” avere un ID ma in realtà non è identificabile.
* `JokeId.New()` deve generare internamente un `Guid` valido (o equivalente) e incapsularlo nel VO.
* Il test verifica due aspetti complementari:

  * `Value != Guid.Empty`: il valore grezzo non è sentinella;
  * `IsEmpty == false`: la proprietà di convenienza rispecchia la stessa verità.

**Aspettativa di dominio:** ogni ID generato dal dominio è immediatamente utilizzabile e tracciabile.

---

### 🧪 Test: `Create_ShouldReturnId_WhenGuidIsValid`

**Scopo:** verificare che `JokeId.Create(Guid)` costruisca correttamente un VO da un Guid valido e che **preservi** esattamente il valore passato (nessuna trasformazione).

**Sintassi (estratto):**

```csharp
Guid guid = Guid.NewGuid();

var id = JokeId.Create(guid);

id.Value.Should().Be(guid);
id.IsEmpty.Should().BeFalse();
```

**Spiegazione:**

* `Guid.NewGuid()` genera un valore non vuoto (quindi valido ai fini dell’invariante).
* `Create(guid)` rappresenta la “porta di ingresso” per ID provenienti dall’esterno (persistence, mapping, input già validato, ecc.).
* Il test vincola:

  * **uguaglianza**: `id.Value` deve essere *esattamente* `guid`;
  * **coerenza**: `IsEmpty` deve essere `false`.

**Aspettativa di dominio:** se il chiamante fornisce un Guid valido, il dominio lo accetta senza ambiguità e mantiene lo stesso identificatore.

---

### 🧪 Test: `Create_ShouldThrow_WhenGuidIsEmpty`

**Scopo:** verificare che `JokeId.Create(Guid.Empty)` venga rifiutato in modo fail-fast e diagnostico.

**Sintassi (estratto):**

```csharp
var act = () => JokeId.Create(Guid.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(JokeErrorMessages.JokeIdEmpty)
   .Which.MemberName.Should().Be(nameof(JokeId));
```

**Spiegazione:**

* `Guid.Empty` non può identificare una Joke reale: accettarlo renderebbe l’identità fragile e introdurrebbe stati inconsistenti.
* Il test vincola **tre** aspetti del contratto d’errore (tutti importanti in un dominio “serio”):

  1. **Tipo**: `DomainValidationException` (eccezione di dominio, non generica).
  2. **Messaggio**: `JokeErrorMessages.JokeIdEmpty` (centralizzato e coerente).
  3. **MemberName**: `nameof(JokeId)` (il “campo logico” responsabile dell’errore è l’ID stesso come Value Object).

Questa combinazione rende l’errore:

* prevedibile (testabile),
* leggibile,
* tracciabile in log/telemetria,
* uniforme tra tutti i Value Object.

**Aspettativa di dominio:** il dominio respinge valori sentinella e non permette identità “finte”.

---

## 4️⃣ Conclusione

La suite `JokeIdTests` certifica che `JokeId` sia un Value Object:

* **tipizzato** (evita di passare `Guid` grezzi ovunque),
* **valido per costruzione** (mai `Guid.Empty`),
* **coerente** (tramite `Value` e `IsEmpty`),
* **fail-fast** e diagnostico (eccezione di dominio con messaggio e `MemberName` vincolati).

Questo rende l’identità della Joke affidabile in tutta la pipeline (dominio → eventi → persistence → API), riducendo drasticamente il rischio di stati “non inizializzati” difficili da debuggare.

---

