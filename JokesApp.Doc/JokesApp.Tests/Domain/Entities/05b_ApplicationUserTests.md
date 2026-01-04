# 📘 05b_ApplicationUserTests — Manuale didattico completo dei test per `ApplicationUser`

> **File di test:** `JokesApp.Tests/Domain/Entities/ApplicationUserTests.cs`  
> **Classe di test:** `JokesApp.Tests.Domain.Entities.ApplicationUserTests`  
> **Soggetto:** Entità di dominio `ApplicationUser` (Domain Layer)

---

## 1️⃣ Scopo della suite

Questa suite verifica il comportamento dell’entità di dominio `ApplicationUser` secondo un approccio coerente con DDD e Clean Architecture: creazione valida, mutazioni controllate e protezione da stati inconsistenti.

**Macro-obiettivi della suite:**
- garantire invarianti di creazione (Id, DisplayName, Email);
- verificare che le mutazioni (`UpdateProfile`, `ChangeEmail`, `SetAvatar`) producano uno stato coerente;
- verificare la gestione dei timestamp (`CreatedAt` UTC, `UpdatedAt` dopo le modifiche);
- verificare la “rete di sicurezza” `ValidateIntegrity` contro stati corrotti (forzati via reflection).

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit
- **`[Fact]`**: test singolo, non parametrico.

### ✅ FluentAssertions
- **`.Should().Be(...)`**: confronto diretto.
- **`.Should().BeNull()` / `.Should().NotBeNull()`**: verifica nullability.
- **`.Throw<DomainValidationException>()`**: vincola il tipo di eccezione.
- **`.WithMessage(...)`**: vincola il messaggio (centralizzato).
- **`.Which.MemberName`**: vincola il parametro/membro responsabile dell’errore.

### ✅ Lambda “act”
Nei test che verificano eccezioni, l’azione viene incapsulata in una lambda:
```csharp
var act = () => /* operazione che deve lanciare */;
```

Questo consente a FluentAssertions di intercettare l’eccezione in modo deterministico.

### ✅ Reflection (solo per test di integrità)

La reflection è usata esclusivamente per forzare stati invalidi e verificare che `ValidateIntegrity` intercetti corruzioni dello stato che l’API pubblica normalmente impedirebbe.

---

## 3️⃣ Test del costruttore

### 🧪 Test: `Constructor_ShouldSetProperties_WhenValuesAreValid`

**Scopo:** verificare che il costruttore assegni correttamente le proprietà e imposti i timestamp iniziali.

**Sintassi (estratto):**

```csharp
var id = UserId.Create("user-1");
var name = DisplayName.Create("Ada");
var email = EmailAddress.Create("ada@example.com");
var avatar = AvatarUrl.Create("https://example.com/avatar.png");

var user = new ApplicationUser(id, name, email, avatar);

user.Id.Should().Be(id);
user.DisplayName.Should().Be(name);
user.Email.Should().Be(email);
user.AvatarUrl.Should().Be(avatar);

user.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
user.UpdatedAt.Should().BeNull();
```

**Aspettativa di dominio:**

* creazione valida ⇒ stato coerente;
* `CreatedAt` deve essere in UTC;
* `UpdatedAt` deve restare nullo (nessuna modifica post-creazione).

---

### 🧪 Test: `Constructor_ShouldThrow_WhenIdIsEmpty`

**Scopo:** impedire creazione con `UserId.Empty`.

**Sintassi (estratto):**

```csharp
var act = () => new ApplicationUser(
    UserId.Empty,
    DisplayName.Create("A"),
    EmailAddress.Create("a@example.com"),
    AvatarUrl.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
   .Which.MemberName.Should().Be("id");
```

**Dettaglio importante:** `MemberName` atteso è `"id"` perché l’errore è associato al parametro del costruttore.

---

### 🧪 Test: `Constructor_ShouldThrow_WhenDisplayNameIsEmpty`

**Scopo:** imporre il vincolo “DisplayName richiesto”.

**Sintassi (estratto):**

```csharp
var act = () => new ApplicationUser(
    UserId.Create("user"),
    DisplayName.Empty,
    EmailAddress.Create("a@example.com"),
    AvatarUrl.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
   .Which.MemberName.Should().Be("displayName");
```

---

### 🧪 Test: `Constructor_ShouldThrow_WhenEmailIsEmpty`

**Scopo:** imporre il vincolo “Email richiesta”.

**Sintassi (estratto):**

```csharp
var act = () => new ApplicationUser(
    UserId.Create("user"),
    DisplayName.Create("A"),
    EmailAddress.Empty,
    AvatarUrl.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailRequired)
   .Which.MemberName.Should().Be("email");
```

---

## 4️⃣ Test `UpdateProfile`

`UpdateProfile` rappresenta una mutazione controllata del profilo. Nei test, il metodo viene usato in due modalità:

* con **tre parametri** (nome, avatar, email) quando si intende aggiornare anche l’email;
* con **due parametri** (nome, avatar) quando l’email non è coinvolta.

### 🧪 Test: `UpdateProfile_ShouldUpdateFieldsAndTimestamp`

**Scopo:** aggiornare nome/avatar/email e valorizzare `UpdatedAt`.

**Sintassi (estratto):**

```csharp
var user = new ApplicationUser(
    UserId.Create("user-1"),
    DisplayName.Create("Ada"),
    EmailAddress.Create("ada@example.com"),
    AvatarUrl.Empty);

var newName = DisplayName.Create("Ada Lovelace");
var newAvatar = AvatarUrl.Create("https://example.com/new.png");
var newEmail = EmailAddress.Create("ada.lovelace@example.com");

user.UpdateProfile(newName, newAvatar, newEmail);

user.DisplayName.Should().Be(newName);
user.AvatarUrl.Should().Be(newAvatar);
user.Email.Should().Be(newEmail);
user.UpdatedAt.Should().NotBeNull();
```

**Aspettativa di dominio:** una mutazione valida deve lasciare lo stato coerente e impostare `UpdatedAt`.

---

### 🧪 Test: `UpdateProfile_ShouldThrow_WhenDisplayNameIsEmpty`

**Scopo:** impedire update con `DisplayName.Empty`.

**Sintassi (estratto):**

```csharp
var act = () => user.UpdateProfile(DisplayName.Empty, AvatarUrl.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.DisplayNameRequired)
   .Which.MemberName.Should().Be("displayName");
```

---

### 🧪 Test: `UpdateProfile_ShouldThrow_WhenEmailIsProvidedAndEmpty`

**Scopo:** se il chiamante “fornisce” l’email nell’update, il valore non può essere vuoto.

**Sintassi (estratto):**

```csharp
var act = () => user.UpdateProfile(
    DisplayName.Create("New"),
    AvatarUrl.Empty,
    EmailAddress.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailRequired)
   .Which.MemberName.Should().Be("email");
```

**Aspettativa di dominio:** email esplicitamente passata ⇒ deve essere valida (non vuota).

---

## 5️⃣ Test `ChangeEmail`

### 🧪 Test: `ChangeEmail_ShouldUpdateEmailAndTimestamp`

**Scopo:** aggiornare l’email e impostare `UpdatedAt`.

**Sintassi (estratto):**

```csharp
var newEmail = EmailAddress.Create("ada@new.com");

user.ChangeEmail(newEmail);

user.Email.Should().Be(newEmail);
user.UpdatedAt.Should().NotBeNull();
```

---

### 🧪 Test: `ChangeEmail_ShouldThrow_WhenEmailIsEmpty`

**Scopo:** impedire cambio email con `EmailAddress.Empty`.

**Sintassi (estratto):**

```csharp
var act = () => user.ChangeEmail(EmailAddress.Empty);

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.EmailRequired)
   .Which.MemberName.Should().Be("newEmail");
```

**Nota critica:** qui `MemberName` è `"newEmail"` perché l’errore si riferisce al **parametro del metodo** `ChangeEmail`, non alla proprietà `Email`.

---

## 6️⃣ Test `SetAvatar`

### 🧪 Test: `SetAvatar_ShouldUpdateAvatarAndTimestamp`

**Scopo:** aggiornare l’avatar e impostare `UpdatedAt`.

**Sintassi (estratto):**

```csharp
var newAvatar = AvatarUrl.Create("https://example.com/avatar.png");

user.SetAvatar(newAvatar);

user.AvatarUrl.Should().Be(newAvatar);
user.UpdatedAt.Should().NotBeNull();
```

---

## 7️⃣ Test `ValidateIntegrity`

`ValidateIntegrity` è una rete di sicurezza: deve intercettare stati internamente invalidi (es. derivanti da mapping errati, materializzazione corrotta, bug) anche quando tali stati non sono ottenibili tramite l’API pubblica.

### 🧪 Test: `ValidateIntegrity_ShouldThrow_WhenStateIsInvalid`

**Scopo:** forzare uno stato invalido e verificare che `ValidateIntegrity` fallisca in modo deterministico.

**Sintassi (estratto):**

```csharp
SetPrivateProperty(user, nameof(ApplicationUser.Id), UserId.Empty);

var act = () => user.ValidateIntegrity();

act.Should()
   .Throw<DomainValidationException>()
   .WithMessage(ApplicationUserErrorMessages.UserIdNullOrEmpty)
   .Which.MemberName.Should().Be(nameof(ApplicationUser.Id));
```

**Nota:** qui `MemberName` è `nameof(ApplicationUser.Id)` (non una stringa hardcoded), quindi più robusto rispetto a refactor.

---

## 8️⃣ Helper Reflection (supporto ai test)

### 🧪 Helper: `SetPrivateProperty<T>`

**Scopo:** impostare proprietà (anche non pubbliche) tramite Reflection per simulare stati impossibili.

**Sintassi (estratto):**

```csharp
var property = typeof(ApplicationUser)
    .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

property.SetValue(target, value);
```

**Nota tecnica:** l’operatore `!` (null-forgiving) è una scelta consapevole nei test: se il nome proprietà cambia, il test può fallire a runtime, rendendo evidente la rottura del contratto interno.

---

## 9️⃣ Conclusione

La suite `ApplicationUserTests` verifica in modo completo e deterministico:

* invarianti di creazione (`Id`, `DisplayName`, `Email`);
* regole di update (`UpdateProfile`, `ChangeEmail`, `SetAvatar`);
* correttezza dei timestamp (`CreatedAt` UTC, `UpdatedAt` post-mutazione);
* resilienza tramite `ValidateIntegrity` contro stati corrotti.

---