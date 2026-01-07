# 📘 JokesApp.Doc/JokesApp.Tests/Domain/ValueObjects/04b_AvatarUrlTests.md — Manuale didattico per `AvatarUrl`

> **File di test:** `JokesApp.Tests/Domain/ValueObjects/AvatarUrlTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.ValueObjects.AvatarUrlTests`  
> **Soggetto:** Value Object `AvatarUrl` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il Value Object `AvatarUrl` con un’impostazione **DDD “valido per costruzione”**, ma con una scelta progettuale importante: `AvatarUrl` è un VO **tollerante** rispetto all’assenza di valore.

In pratica:

- se l’input è **assente** ( `null`, stringa vuota, solo whitespace ), la factory **non lancia eccezioni** e normalizza tutto in `AvatarUrl.Empty`;
- se l’input è **presente**, allora applica regole “forti”:
  - normalizzazione tramite `Trim()`;
  - vincolo di lunghezza massima (`AvatarUrl.MaxLength`);
  - validazione di **URL** (schema ammesso: tipicamente `http` / `https`).

Obiettivo: evitare `null` nel dominio (pattern “Empty Object”) e mantenere comunque un contratto chiaro e sicuro quando l’URL viene fornito.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: scenario singolo (qui usato per il test di `MaxLength`).
- **`[Theory]`** + **`[InlineData]`**: stessa regola validata su più input (qui usato per: null/empty/whitespace, URL invalidi, URL validi).

### ✅ FluentAssertions
- **`.Should().BeSameAs(...)`**: verifica **identità di riferimento** (reference equality).  
  In questa suite è fondamentale per confermare che `AvatarUrl.Empty` sia un **singleton** (o comunque una istanza condivisa) e che la factory restituisca proprio *quella* istanza.
- **`.Should().BeTrue()` / `.Should().BeFalse()`**: assert booleani leggibili (qui: `IsEmpty`).
- **`.Should().Be(...)`**: confronto diretto del valore normalizzato (qui: `Value` deve essere `Trim()`).
- **`.Throw<DomainValidationException>()`**: vincola la validazione a un’eccezione di dominio.
- **`.WithMessage(...)`**: vincola il messaggio d’errore centralizzato in `ApplicationUserErrorMessages`.
- **`.Which.MemberName`**: vincola il “campo logico” responsabile dell’errore, qui coerentemente `nameof(AvatarUrl)`.

### ✅ Lambda “act” per test di eccezioni
Quando ci si aspetta un `throw`, l’azione va incapsulata in una lambda:

```csharp
var act = () => AvatarUrl.Create(value);
```

Così FluentAssertions può invocare l’azione e intercettare l’eccezione in modo deterministico.

---

## 3️⃣ Test documentati (1:1 con `AvatarUrlTests.cs`)

### 🧪 Test: `Create_ShouldReturnEmpty_WhenValueIsNullOrWhitespace`

**Scopo:** verificare il comportamento “tollerante” della factory: input assenti non sono errori, ma vengono normalizzati in `AvatarUrl.Empty`.

**Sintassi (estratto):**

```csharp
var avatarUrl = AvatarUrl.Create(value);

avatarUrl.Should().BeSameAs(AvatarUrl.Empty);
avatarUrl.IsEmpty.Should().BeTrue();
```

**Casi coperti (Theory):**

* `null`
* `""`
* `"   "`

**Spiegazione:**

* Questo test sancisce una scelta architetturale: *l’assenza dell’avatar è lecita*.
* `BeSameAs(AvatarUrl.Empty)` non verifica solo l’uguaglianza logica, ma l’identità dell’istanza: il risultato deve essere **esattamente** `AvatarUrl.Empty` (pattern “Empty Object” / singleton).
* `IsEmpty == true` permette di rappresentare esplicitamente “nessun avatar” senza usare `null` nel dominio.

**Aspettativa di dominio:** l’avatar può mancare senza generare eccezioni; chi usa il VO può controllare lo stato con `IsEmpty`.

---

### 🧪 Test: `Create_ShouldThrow_WhenValueExceedsMaxLength`

**Scopo:** impedire che un URL “troppo lungo” entri nel dominio.

**Sintassi (estratto):**

```csharp
string tooLong = "https://example.com/" + new string('a', AvatarUrl.MaxLength);

var act = () => AvatarUrl.Create(tooLong);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.AvatarUrlMaxLength)
   .Which.MemberName.Should().Be(nameof(AvatarUrl));
```

**Spiegazione:**

* Il test costruisce un input volutamente fuori soglia, in modo **deterministico**: prefisso noto + coda di caratteri.
* Anche senza conoscere i dettagli interni, l’intento è chiaro: superare `AvatarUrl.MaxLength` e verificare che la factory faccia fail-fast.
* Il contratto d’errore è vincolato in modo “enterprise-grade”:

  * **Tipo**: `DomainValidationException`
  * **Messaggio**: `ApplicationUserErrorMessages.AvatarUrlMaxLength`
  * **MemberName**: `nameof(AvatarUrl)`

**Aspettativa di dominio:** lunghezze eccessive vengono fermate alla frontiera (factory), evitando propagazione verso persistence, log, mapping o UI.

---

### 🧪 Test: `Create_ShouldThrow_WhenUrlIsInvalid`

**Scopo:** impedire URL non conformi alle regole del dominio (schema non ammesso o stringa non interpretabile come URL).

**Sintassi (estratto):**

```csharp
var act = () => AvatarUrl.Create(value);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.AvatarUrlInvalid)
   .Which.MemberName.Should().Be(nameof(AvatarUrl));
```

**Casi coperti (Theory):**

* `"ftp://example.com/avatar.png"` → schema non consentito (non `http/https`)
* `"not-a-url"` → stringa che non rappresenta una URL valida

**Spiegazione:**

* Qui `AvatarUrl` non è più “tollerante”: **se un valore viene fornito**, deve rispettare le regole.
* Il test mostra bene la distinzione tra:

  * *assenza di valore* ⇒ `Empty` (nessuna eccezione)
  * *valore presente ma invalido* ⇒ eccezione di dominio

**Aspettativa di dominio:** un avatar URL fornito deve essere un URL reale e conforme agli schemi ammessi.

---

### 🧪 Test: `Create_ShouldReturnUrl_WhenValueIsValid`

**Scopo:** verificare che, con input valido, la factory ritorni un VO non vuoto e normalizzato.

**Sintassi (estratto):**

```csharp
var avatarUrl = AvatarUrl.Create(value);

avatarUrl.Value.Should().Be(value.Trim());
avatarUrl.IsEmpty.Should().BeFalse();
```

**Casi coperti (Theory):**

* `"https://example.com/avatar.png"`
* `"  http://example.com/avatar.png  "` (con spazi ai bordi)

**Spiegazione:**

* Il test vincola la **forma canonica** del valore: `Value` deve essere sempre `Trim()` dell’input.
* `IsEmpty` deve essere `false`: qui non stiamo gestendo “assenza”, ma un URL effettivo e valido.

**Aspettativa di dominio:** quando l’avatar è presente, il dominio espone un valore pulito e coerente (nessun trim a valle).

---

## 4️⃣ Conclusione

La suite `AvatarUrlTests` certifica che `AvatarUrl` è un Value Object:

* **tollerante** sull’assenza di input (null/empty/whitespace ⇒ `AvatarUrl.Empty`, con identità verificata via `BeSameAs`);
* **rigoroso** quando un valore è fornito (vincoli su lunghezza e formato URL);
* **normalizzato** (trim applicato all’ingresso);
* **diagnostico e coerente** sugli errori (eccezione di dominio + messaggio centralizzato + `MemberName` vincolato).

È quindi una documentazione **chiudibile**: completa, deterministica e allineata 1:1 con `AvatarUrlTests.cs`.

---