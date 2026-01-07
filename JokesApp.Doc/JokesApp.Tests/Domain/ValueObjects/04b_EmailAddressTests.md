# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04b_EmailAddressTests.md — Manuale didattico per `EmailAddress`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/EmailAddressTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.EmailAddressTests`  
> **Soggetto:** Value Object `EmailAddress` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `EmailAddress` applicando il principio DDD **“valido per costruzione”**: nel dominio, un’istanza di `EmailAddress` può esistere **solo** se rispetta tutte le invarianti previste. In caso contrario, la factory deve fallire **immediatamente** (*fail-fast*) e in modo **diagnostico** (eccezione di dominio + messaggio centralizzato + `MemberName` coerente).

In particolare, i test garantiscono che la factory:

- `EmailAddress.Create(...)` **normalizzi** l’input tramite `Trim()` (forma canonica della mail);
- rifiuti input non significativi: `null`, stringa vuota e stringhe composte solo da whitespace;
- applichi un vincolo di lunghezza massima tramite `EmailAddress.MaxLength`;
- rifiuti formati email non validi secondo le regole del dominio (es. assenza di `@`, dominio non corretto, pattern vietati, caratteri non accettati dalla validazione).

Obiettivo pratico: impedire che nel dominio circolino email “sporche”, non valide o ingestibili, prevenendo inconsistenze in persistenza, autenticazione, correlazioni, log/audit e workflow applicativi.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Theory]`** + **`[InlineData]`**: stessa regola validata su più input senza duplicare codice.  
  In questa suite viene usato sia per gli **input validi**, sia per gli **edge-case invalidi**.
- **`[Fact]`**: test singolo, utile quando lo scenario è costruito “ad hoc” (qui: superamento di `MaxLength` con una stringa generata).

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto e leggibile (qui per `Value` e per `Length`).
- **`.Should().BeFalse()`**: assert booleano semantico (qui: `IsEmpty` deve essere `false` quando l’email è valida).
- **`.Throw<DomainValidationException>()`**: vincola il fallimento a un errore **di dominio**, evitando eccezioni generiche.
- **`.WithMessage(...)`**: vincola il messaggio d’errore centralizzato in `ApplicationUserErrorMessages` (coerenza e stabilità).
- **`.Which.MemberName`**: vincola l’informazione diagnostica sul “campo logico” responsabile dell’errore (qui: `nameof(EmailAddress)`).

### ✅ Lambda “act” per test di eccezioni
Quando un test deve verificare un `throw`, l’azione viene incapsulata in una lambda:

```csharp
var act = () => EmailAddress.Create(value);
```

Questo consente a FluentAssertions di invocare l’azione e intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `EmailAddressTests.cs`)

### 🧪 Test: `Create_ShouldReturnNormalizedEmail_WhenValueIsValid`

**Scopo:** verificare che, con input valido, la factory ritorni un VO non vuoto e coerente, applicando la normalizzazione attesa (`Trim()`).

**Sintassi (estratto):**

```csharp
var email = EmailAddress.Create(value);

email.Value.Should().Be(value.Trim());
email.Length.Should().Be(email.Value.Length);
email.IsEmpty.Should().BeFalse();
```

**Casi coperti (Theory):**

* `"test@example.com"` (caso semplice e canonico)
* `"USER.NAME+tag@sub.domain.co.uk"` (case misto + tag + sottodomini)
* `"  user_name-123@example.io  "` (spazi ai bordi → normalizzazione)

**Spiegazione:**

* `Value` deve essere la **forma canonica** dell’input: in questa suite la normalizzazione verificata è il `Trim()` degli spazi laterali.
* `Length` deve riflettere il valore **interno** dell’oggetto (`email.Value.Length`), così da evitare discrepanze tra input grezzo e dato realmente “entrato” nel dominio.
* `IsEmpty` deve essere `false`: un’email valida e presente non può essere considerata “vuota”.

**Aspettativa di dominio:** se l’oggetto esiste, allora è già pronto e coerente, senza necessità di ulteriori normalizzazioni o controlli a valle.

---

### 🧪 Test: `Create_ShouldThrow_WhenEmailIsNullOrWhitespace`

**Scopo:** impedire la creazione del VO quando il valore non è significativo.

**Sintassi (estratto):**

```csharp
var act = () => EmailAddress.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailRequired)
   .Which.MemberName.Should().Be(nameof(EmailAddress));
```

**Casi coperti (Theory):**

* `null`
* `""`
* `"   "`

**Spiegazione:**

* L’email è un dato obbligatorio in questo VO: `null` / empty / whitespace sono input equivalenti dal punto di vista semantico (non identificano alcun indirizzo).
* Il test vincola un contratto d’errore completo:

  1. **Tipo**: `DomainValidationException`
  2. **Messaggio**: `ApplicationUserErrorMessages.EmailRequired`
  3. **MemberName**: `nameof(EmailAddress)`

**Aspettativa di dominio:** l’oggetto non deve nascere se manca il contenuto informativo minimo.

---

### 🧪 Test: `Create_ShouldThrow_WhenEmailExceedsMaxLength`

**Scopo:** impedire la creazione quando la lunghezza complessiva supera il limite massimo.

**Sintassi (estratto):**

```csharp
var localPart = new string('a', EmailAddress.MaxLength) + "@example.com";

var act = () => EmailAddress.Create(localPart);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailTooLong)
   .Which.MemberName.Should().Be(nameof(EmailAddress));
```

**Spiegazione:**

* `EmailAddress.MaxLength` è un vincolo “hard” utile a proteggere:

  * robustezza del dominio contro input patologici,
  * limiti di persistenza (colonne, indici, constraint),
  * stabilità operativa (log, serializzazione, ecc.).
* La stringa è costruita in modo deterministico per essere “sicuramente oltre soglia”.

**Aspettativa di dominio:** dati eccessivi vengono fermati alla frontiera (factory), evitando propagazione nel resto del sistema.

---

### 🧪 Test: `Create_ShouldThrow_WhenEmailIsInvalid`

**Scopo:** rifiutare formati non accettati dalla validazione del dominio.

**Sintassi (estratto):**

```csharp
var act = () => EmailAddress.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailInvalid)
   .Which.MemberName.Should().Be(nameof(EmailAddress));
```

**Casi coperti (Theory):**

* `"plainaddress"` (assenza di `@`)
* `"missing@domain"` (dominio incompleto / non conforme)
* `"user@domain..com"` (pattern non valido nel dominio)
* `"user@-domain.com"` (pattern di dominio non consentito)
* `"user@domain.c"` (TLD troppo corto)
* `"màrio@example.com"` (caratteri non accettati dalla validazione del dominio)

**Spiegazione:**

* Questo set di input serve a evitare validazioni “troppo permissive” che farebbero entrare nel dominio stringhe non realmente utilizzabili.
* Anche qui il contratto d’errore è vincolato su tipo/messaggio/member per uniformità e diagnosi.

**Aspettativa di dominio:** se viene fornita un’email, deve essere formalmente valida secondo le regole di `EmailAddress`.

---

## 4️⃣ Conclusione

La suite `EmailAddressTests` certifica che `EmailAddress` sia un Value Object:

* **normalizzato** (trim applicato all’ingresso → forma canonica),
* **non degenerabile** (mai `null`/empty/whitespace),
* **vincolato** da `MaxLength`,
* **rigoroso** sul formato (input non conformi respinti),
* **fail-fast e diagnostico** (eccezioni di dominio con messaggio centralizzato e `MemberName` vincolato).

È quindi una documentazione **chiudibile**: completa, deterministica e allineata 1:1 con `EmailAddressTests.cs`.

---
