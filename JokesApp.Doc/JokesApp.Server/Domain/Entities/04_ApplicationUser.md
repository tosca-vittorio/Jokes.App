# 📘 **04_ApplicationUser.md**

## *Evoluzione completa del modello utente e integrazione con ASP.NET Core Identity*

---

# 📚 **Introduzione generale**

Il modello `ApplicationUser` rappresenta l'entità utente del sistema *JokesApp*.
A differenza di un semplice modello personalizzato, `ApplicationUser` estende `IdentityUser` della libreria **ASP.NET Core Identity**, acquisendo così tutte le funzionalità fondamentali per:

* autenticazione
* autorizzazione
* gestione credenziali
* sicurezza
* gestione utenti e ruoli
* flusso completo login / registrazione / gestione profilo

Questo file documenta l’intera **evoluzione storica** del modello, dalla prima bozza essenziale alle versioni avanzate presenti oggi nel progetto, seguendo un percorso logico, architetturale e tecnico.

La documentazione è suddivisa in fasi, ognuna delle quali descrive:

* cosa è stato introdotto
* perché è stato introdotto
* impatto architetturale
* come ha influenzato gli sviluppi successivi
* eventuali revisioni o miglioramenti

---

---

# 🧩 **FASE 1 — Prima versione (bozza iniziale)**

### *Obiettivo: creare un’entità utente minima integrata con Identity*

In questa fase il progetto richiedeva semplicemente:

* avere utenti registrabili tramite ASP.NET Identity
* aggiungere un campo personalizzato (`DisplayName`)
* stabilire la relazione 1-a-molti con `Joke`

Il modello iniziale era quindi molto semplice:

```csharp
public class ApplicationUser : IdentityUser
{
    [MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;

    public ICollection<Joke> Jokes { get; set; } = new List<Joke>();
}
```

### 🔍 Analisi della Fase 1

**Perché estendere IdentityUser?**
Perché permette di avere già pronte tutte le funzionalità essenziali per l’autenticazione, senza reinventare password hashing, token, lockout, e così via.

**Perché aggiungere DisplayName?**
Serve a mostrare un nome leggibile e amichevole all’interno dell’applicazione.

**Perché inizializzare a string.Empty?**
Per evitare problemi di *nullability*, soprattutto in .NET 6+.

**Relazione con Joke:**
L'applicazione prevede che ogni utente possa creare molte barzellette → relazione 1:N.

Questa prima bozza era corretta, semplice e perfettamente conforme alle best practices iniziali.

---

---

# 🧩 **FASE 2 — Integrazione Identity + DbContext + Relazioni con Joke**

Nella seconda fase è stata introdotta la configurazione infrastrutturale:

* Identity è stata aggiunta ai servizi
* `JokesDbContext` è stato configurato per utilizzare Identity
* è stata creata la relazione *ApplicationUser → Joke* a livello ORM

Il DbContext è diventato:

```csharp
public class JokesDbContext : IdentityDbContext<ApplicationUser>
{
    public JokesDbContext(DbContextOptions<JokesDbContext> options) : base(options) { }

    public DbSet<Joke> Jokes { get; set; }
}
```

### 🔍 Impatto architetturale

1. Identity diventa parte integrante del *domain model*, non un modulo esterno.
2. `ApplicationUser` diventa una vera entità persistita nel database.
3. EF Core genera automaticamente la relazione *utente → joke*.
4. `ApplicationUserId` viene aggiunta a `Joke` come foreign key.

Questa fase costituisce il **fondamento dell’intero sistema utenti**.

---

---

# 🧩 **FASE 3 — Introduzione di CreatedAt, UpdatedAt e AvatarUrl**

Col progredire del progetto, emerge la necessità di:

* salvare la data di creazione dell’utente
* tracciare eventuali modifiche profilo
* permettere un’immagine avatar pubblica

In questa fase vengono introdotte nuove proprietà:

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }

[MaxLength(2048)]
[Url]
public string? AvatarUrl { get; set; }
```

### 🔍 Motivazioni

* **Audit log interno** → sapere *quando* è stato creato un account.
* **User Experience** → supporto immagine profilo.
* **Manutenibilità** → UpdatedAt utile per rilevare modifiche profilo lato UI.
* **URI standard** → limite 2048 caratteri, convenzione comune nei browser.

### 🔍 Scelta architetturale corretta

* Timestamps in **UTC** → unico standard per applicazioni distribuite.
* AvatarUrl opzionale (nullable) → coerente con la UX.

---

---

# 🧩 **FASE 4 — Validazioni più forti e trimming automatico**

A questo punto l’applicazione necessita di:

* impedire che DisplayName o AvatarUrl vengano assegnati in modo incoerente
* evitare whitespace superflui
* centralizzare i messaggi di errore
* aggiornare automaticamente UpdatedAt

Vengono introdotti:

### ✔️ backing fields (`_displayName`, `_avatarUrl`)

→ Per intercettare e controllare l’assegnazione.

### ✔️ validazioni più composte

→ MaxLength, Url, Required/Nullable.

### ✔️ trimming automatico

→ Evita valori come `"  Vittorio  "` o `"   "`.

### ✔️ UpdatedAt automatico

→ Qualunque modifica a email, avatar o nome aggiorna la data modificata.

Esempio:

```csharp
private string _displayName = string.Empty;

public string DisplayName
{
    get => _displayName;
    set
    {
        if (_displayName != value?.Trim())
        {
            _displayName = value?.Trim() ?? string.Empty;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
```

### 🔍 Perché questa soluzione è professionale?

Perché trasforma `ApplicationUser` in un vero **domain model**, capace di:

* garantire coerenza interna
* impedire modifiche inconsistenti
* mantenere referential integrity
* tracciare aggiornamenti a livello entità

---

---

# 🧩 **FASE 5 — Validazione custom dell’e-mail + isolamento messaggi di errore**

Qui viene introdotto un miglioramento architetturale importante:

## ✔️ Validatore email personalizzato (`CustomEmailAttribute`)

Motivi:

* la validazione standard di `[EmailAddress]` è insufficiente → troppo permissiva
* il dominio richiede una validazione più severa
* necessità di consolidare un comportamento uniforme in tutta l’applicazione

### Inoltre:

## ✔️ Introduzione di `ApplicationUserErrorMessages.cs`

```csharp
public static class ApplicationUserErrorMessages
{
    public const string DisplayNameMaxLength = "DisplayName exceeds maximum length of 50.";
    public const string AvatarUrlMaxLength = "AvatarUrl exceeds maximum length of 2048.";
    public const string AvatarUrlInvalid = "AvatarUrl is not a valid URL.";
    public const string EmailInvalid = "Email is not a valid email address.";
    public const string EmailRequired = "Email is required.";
}
```

### 🔍 Vantaggi della centralizzazione dei messaggi

* consistenza UX
* manutenzione semplificata
* test più affidabili
* riduzione ripetizioni
* codice più pulito e più professionale

---

---

# 🧩 **FASE 6 — Versione attuale e definitiva del modello (stato del progetto)**

Questa è la versione completa, pulita e finale del modello utente:

```csharp
public class ApplicationUser : IdentityUser
{
    private string _displayName = string.Empty;

    [MaxLength(50, ErrorMessage = ApplicationUserErrorMessages.DisplayNameMaxLength)]
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (_displayName != value?.Trim())
            {
                _displayName = value?.Trim() ?? string.Empty;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private string? _avatarUrl;

    [MaxLength(2048, ErrorMessage = ApplicationUserErrorMessages.AvatarUrlMaxLength)]
    [Url(ErrorMessage = ApplicationUserErrorMessages.AvatarUrlInvalid)]
    public string? AvatarUrl
    {
        get => _avatarUrl;
        set
        {
            if (_avatarUrl != value)
            {
                _avatarUrl = value;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [Required(ErrorMessage = ApplicationUserErrorMessages.EmailRequired)]
    [CustomEmail(ErrorMessage = ApplicationUserErrorMessages.EmailInvalid)]
    [MaxLength(256)]
    public override string? Email
    {
        get => base.Email;
        set
        {
            if (base.Email != value?.Trim())
            {
                base.Email = value?.Trim() ?? string.Empty;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    [JsonIgnore]
    public override string? PasswordHash { get; set; }

    [JsonIgnore]
    public override string? SecurityStamp { get; set; }

    public ICollection<Joke> Jokes { get; set; } = new List<Joke>();
}
```

### 🔍 Commento architetturale finale

Questa versione è **maturo domain modeling**, non più semplice modellazione dati:

* proprietà coerenti e pulite grazie ai backing fields
* validazioni incoraggiano input puliti e consistenti
* UpdatedAt automatico → audit log interno
* CustomEmail → controllo totale sulle regole del sistema
* JSON ignore → protezione dei dati sensibili
* relazione uno-a-molti solida con `Joke`

Si tratta di una soluzione robusta, scalabile, professionale.

---

---

# 🧭 **FASE 7 — Decisioni architetturali e best practices adottate**

### ✔️ Estendere IdentityUser

Evita reinventare meccanismi complessi e delicati.

### ✔️ Validazioni su modello + validazioni custom

Garantiscono integrità prima ancora che i dati arrivino al database.

### ✔️ Use UTC always

Sistema coerente in scenari multi-fuso.

### ✔️ Backing fields

Permettono controllo evoluto sull’assegnazione delle proprietà.

### ✔️ Messaggi di errore centralizzati

Pattern altamente professionale, mantiene il dominio pulito.

### ✔️ Aggiornamento dei timestamps automatico

Supporta in modo naturale future funzionalità di audit.

---

---

# 📈 **Diagrama relazionale (semplificato)**

```
 ApplicationUser (AspNetUsers)
 ├── Id (string, PK)
 ├── Email
 ├── DisplayName
 ├── AvatarUrl
 ├── CreatedAt
 └── UpdatedAt
        │
        │ 1 ↦ N
        ▼
 Joke
 ├── Id (int, PK)
 ├── Question
 ├── Answer
 ├── CreatedAt / UpdatedAt
 └── ApplicationUserId (FK → AspNetUsers.Id)
```

---

# 🏁 **Conclusione**

Il modello `ApplicationUser` è passato da:

* una struttura semplice
  a
* un modello maturo con validazioni, audit, relazioni, attributi custom e ottimizzazioni progettuali.

Questa evoluzione documentata rappresenta un percorso professionale e accurato, perfettamente coerente con un sistema backend moderno e solido.

---
