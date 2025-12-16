# 📘 05b_ApplicationUser.md

## ApplicationUser — Entità di Dominio (stato attuale)

Documentazione **verità** del modello `ApplicationUser` nel Domain Layer di JokesApp.
Questa entità è **pura** (DDD/Clean): non dipende da ASP.NET Identity, EF Core, DataAnnotations o serializzazione.
Le regole di validazione sono principalmente garantite tramite Value Object.

---

## 1) Ruolo nel dominio

`ApplicationUser` rappresenta l’utente applicativo che:
- possiede un’identità tipizzata (`UserId`);
- espone informazioni di profilo tipizzate (Value Object);
- mantiene un audit minimo (`CreatedAt`, `UpdatedAt`);
- mantiene la relazione 1:N con le barzellette create (`Jokes`).

---

## 2) Struttura dell’entità

### 2.1 Proprietà principali (stato)

- `UserId Id`  
  Identificativo tipizzato dell’utente.

- `DisplayName DisplayName`  
  Nome visualizzato nell’applicazione.

- `EmailAddress Email`  
  Indirizzo email tipizzato (Value Object).

- `AvatarUrl AvatarUrl`  
  URL avatar tipizzato. È **opzionale**: `AvatarUrl.Empty` indica “nessun avatar”.

- `DateTime CreatedAt` (UTC)  
  Data creazione account.

- `DateTime? UpdatedAt` (UTC)  
  Data ultimo aggiornamento profilo (null se mai aggiornato).

- `ICollection<Joke> Jokes`  
  Collezione delle barzellette create dall’utente (relazione 1:N).

---

## 3) Invarianti di dominio

L’entità deve rispettare queste invarianti minime:

- `Id` **non** può essere `UserId.Empty`
- `DisplayName` **non** può essere `DisplayName.Empty`
- `Email` **non** può essere `EmailAddress.Empty`
- `AvatarUrl` **può** essere `AvatarUrl.Empty` (assenza avatar)

Queste invarianti sono imposte:
- nel costruttore pubblico (creazione valida nel dominio),
- nei metodi di aggiornamento (profilo/email),
- in `ValidateIntegrity()` (entry point esplicito utile per test/import/debug).

---

## 4) Costruzione e re-idratazione

### 4.1 Costruttore protetto (re-idratazione / strumenti di persistenza)

`protected ApplicationUser()` esiste per consentire la re-idratazione da strumenti di persistenza.
Inizializza placeholder `Empty` (stato temporaneamente non valido **nel dominio**) e demanda ai layer superiori / ORM l’assegnazione dei valori reali.

> Nota: in re-idratazione, lo strumento di persistenza deve impostare le proprietà effettive (Id, DisplayName, Email, AvatarUrl, CreatedAt, UpdatedAt) coerenti con lo storage.

### 4.2 Costruttore pubblico (creazione valida nel dominio)

`public ApplicationUser(UserId id, DisplayName displayName, EmailAddress email, AvatarUrl avatarUrl)`
crea un utente **immediatamente valido** nel dominio:
- blocca `id/displayName/email` vuoti;
- consente `avatarUrl.Empty`.

---

## 5) Behavior di dominio (metodi)

### 5.1 `ValidateIntegrity()`
Verifica esplicitamente che l’entità sia in uno stato consistente rispetto alle invarianti minime.

Uso tipico:
- test,
- import,
- debug,
- “assert” esplicito in punti sensibili dell’Application Layer.

### 5.2 `UpdateProfile(DisplayName displayName, AvatarUrl avatarUrl, EmailAddress? email = null)`
Aggiorna:
- `DisplayName` (obbligatorio, non può diventare `Empty`);
- `AvatarUrl` (può essere `Empty` per rimozione avatar);
- `Email` solo se fornita (`email != null`), ma se fornita non può essere `Empty`.

Aggiorna sempre `UpdatedAt` a `DateTime.UtcNow`.

### 5.3 `ChangeEmail(EmailAddress newEmail)`
Aggiorna l’email (obbligatoria, non può essere `Empty`) e aggiorna `UpdatedAt`.

### 5.4 `SetAvatar(AvatarUrl avatarUrl)`
Aggiorna l’avatar (consente `AvatarUrl.Empty` per “nessun avatar”) e aggiorna `UpdatedAt`.

---

## 6) Relazione con Joke

- `ApplicationUser` espone `Jokes` (1:N).
- `Joke` referenzia l’autore tramite `UserId ApplicationUserId`.
- L’eventuale navigazione `Joke.Author` (se presente) è un dettaglio di modello oggetti; la relazione di dominio fondamentale è l’FK tipizzata tramite `UserId`.

---

## 7) Errori ed eccezioni

Quando una regola/invariante viene violata, l’entità genera `DomainValidationException` con messaggi centralizzati in `ApplicationUserErrorMessages`.

Esempi tipici:
- `UserIdNullOrEmpty`
- `DisplayNameRequired`
- `EmailRequired`

---

## 8) Esempi illustrativi (non vincolanti)

> Gli snippet sono solo esempi: servono a chiarire l’intenzione, non a vincolare nomi o firme nei layer esterni.

### Creazione (dominio)
```csharp
var user = new ApplicationUser(
    UserId.Create("auth0|123"),
    DisplayName.Create("Vittorio"),
    EmailAddress.Create("vittorio@example.com"),
    AvatarUrl.Empty);
```

### Aggiornamento profilo
```csharp
user.UpdateProfile(
    DisplayName.Create("Vitto"),
    AvatarUrl.Create("https://cdn.example.com/avatar.png"));
```

### Cambio email

```csharp
user.ChangeEmail(EmailAddress.Create("new@example.com"));
```

---

## 9) Criteri DDD/Clean rispettati

* Entità di dominio **pura** (nessuna dipendenza da framework).
* Validazione “a monte” tramite Value Object.
* Invarianti enforce nel costruttore e nei metodi di modifica.
* Errori consistenti tramite messaggi centralizzati.
* Audit minimo (`CreatedAt`, `UpdatedAt`) mantenuto nel dominio (UTC).

---

