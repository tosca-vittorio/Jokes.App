# 📚 Documentazione Completa: `JokesDbContext`

### *DbContext, Identity e mappatura del dominio con Entity Framework Core*

---

## 1. Contesto e ruolo di `JokesDbContext` nel progetto

Nel backend di **JokesApp** (`JokesApp.Server`) il `DbContext` rappresenta il **ponte tra il dominio C#** (le tue classi `Joke`, `ApplicationUser`, ecc.) e il **database PostgreSQL**.

In concreto, `JokesDbContext` si occupa di:

* tradurre operazioni **LINQ** in **SQL**;
* materializzare righe del database in **entità .NET tracciate**;
* gestire il **ciclo di vita delle entità** (tracking, insert, update, delete);
* applicare le **regole di mapping** (chiavi primarie, chiavi esterne, relazioni, vincoli);
* integrare **ASP.NET Core Identity** (utenti, ruoli, tabelle Identity) con i tuoi modelli di dominio personalizzati.

All’interno della soluzione, la struttura è:

```text
JokesApp.Server/
├── Domain/
│   ├── Errors/
│   │   ├── JokeErrorMessages.cs
│   │   ├── ApplicationUserErrorMessages.cs
│   │   └── ErrorMessages.cs
│   └── (eventuali altre classi di dominio in futuro)
│
├── Models/
│   ├── Joke.cs
│   └── ApplicationUser.cs
│
├── Data/
│   └── JokesDbContext.cs     ← DbContext principale dell’applicazione
│
├── Controllers/
│   └── ...
│
└── Program.cs
```

Il file `JokesDbContext.cs` si trova nella cartella `Data/` ed è il **cuore dell’accesso ai dati** dell’intera applicazione.

---

## 2. Evoluzione storica: dal DbContext semplice a quello integrato con Identity

### 2.1 Fase 1 – Primo `DbContext` minimale (solo `Joke`)

In una **prima versione** del progetto, `JokesDbContext` era una classe che ereditava direttamente da `DbContext` e gestiva una sola entità principale: `Joke`.

Struttura semplificata della prima versione:

```csharp
using Microsoft.EntityFrameworkCore;
using JokesApp.Server.Models;

namespace JokesApp.Server.Data
{
    public class JokesDbContext : DbContext
    {
        public JokesDbContext(DbContextOptions<JokesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Joke> Jokes { get; set; }
    }
}
```

**Caratteristiche di questa fase:**

* `JokesDbContext : DbContext` → il contesto conosceva solo EF Core “puro”.
* Un solo `DbSet<Joke>` → una sola tabella `Jokes` generata nel database.
* Relazioni semplici o assenti (nessun legame ancora con `ApplicationUser`).
* L’obiettivo principale era **mettere in piedi il flusso base**:

  * configurazione connection string;
  * `AddDbContext` in `Program.cs`;
  * prima migration;
  * creazione tabella `Jokes`.

Questa fase ti ha permesso di:

* capire EF Core e il concetto di `DbContext`;
* verificare la connessione con PostgreSQL;
* fare le prime CRUD sulle barzellette.

---

### 2.2 Fase 2 – Introduzione di ASP.NET Core Identity e `ApplicationUser`

Successivamente hai deciso di introdurre:

* un modello utente personalizzato (`ApplicationUser : IdentityUser`);
* il sistema di **autenticazione e gestione utenti** di ASP.NET Core Identity;
* la relazione uno-a-molti tra **utente** e **barzellette**.

A questo punto, il DbContext semplice non era più sufficiente.

È stato necessario:

1. **Integrare Identity** nel DbContext.
2. Utilizzare `IdentityDbContext<ApplicationUser>` come base.
3. Mappare le tabelle Identity (AspNetUsers, AspNetRoles, ecc.) insieme alle tabelle di dominio (`Jokes`).

La classe è quindi passata da:

```csharp
public class JokesDbContext : DbContext
```

a:

```csharp
public class JokesDbContext : IdentityDbContext<ApplicationUser>
```

Questo cambiamento ha avuto un impatto strutturale importante:

* il DbContext non gestisce più solo le entità di dominio, ma anche:

  * utenti (`ApplicationUser`);
  * ruoli (`IdentityRole`);
  * claim, logins, token, ecc.;
* tutte le tabelle Identity vengono generate ed aggiornate **nello stesso database** e nello stesso schema logico delle tue tabelle applicative.

---

### 2.3 Fase 3 – Relazione `ApplicationUser` ↔ `Joke` via Fluent API

Con la definizione di:

* `ApplicationUser` (con la proprietà `public ICollection<Joke> Jokes { get; set; }`)
* `Joke` (con `ApplicationUserId` e `Author`)

è stato naturale passare alla **configurazione esplicita** della relazione uno-a-molti tramite **Fluent API** nel `DbContext`.

Questo è avvenuto attraverso l’override di `OnModelCreating(ModelBuilder modelBuilder)`.

Obiettivo della terza fase:

* rendere **esplicita e chiara** la relazione:

  * un `ApplicationUser` può avere molte `Joke`;
  * ogni `Joke` ha un solo `Author` (`ApplicationUser`);
* definire la **foreign key** (`ApplicationUserId`) in modo coerente;
* specificare il comportamento in caso di **cancellazione dell’utente** (cascade delete).

---

## 3. Codice attuale di `JokesDbContext` (versione finale)

Questa è la versione **aggiornata, pulita e coerente** del tuo `JokesDbContext.cs`:

```csharp
// ============================================================================
// IMPORTAZIONI
// ============================================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JokesApp.Server.Models;

namespace JokesApp.Server.Data
{
    /// <summary>
    /// DbContext principale dell'applicazione.
    /// Integra ASP.NET Core Identity e i modelli di dominio (Joke, ApplicationUser).
    /// </summary>
    public class JokesDbContext : IdentityDbContext<ApplicationUser>
    {
        // --------------------------------------------------------------------
        // COSTRUTTORE
        // --------------------------------------------------------------------
        //
        // Le opzioni (connection string, provider, ecc.) vengono fornite
        // tramite Dependency Injection (AddDbContext in Program.cs).
        //
        public JokesDbContext(DbContextOptions<JokesDbContext> options)
            : base(options)
        {
        }

        // --------------------------------------------------------------------
        // TABELLE DI DOMINIO (DbSet)
        // --------------------------------------------------------------------

        /// <summary>
        /// Rappresenta la tabella delle barzellette nel database.
        /// </summary>
        public DbSet<Joke> Jokes { get; set; } = null!;

        // --------------------------------------------------------------------
        // CONFIGURAZIONE AVANZATA (FLUENT API)
        // --------------------------------------------------------------------
        //
        // Qui configuriamo:
        // - le relazioni tra entità
        // - vincoli aggiuntivi
        // - mapping avanzato oltre le DataAnnotations.
        //
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Importante: lasciare questa chiamata per permettere a Identity
            // di configurare tutte le proprie tabelle (AspNetUsers, AspNetRoles, ecc.).
            base.OnModelCreating(modelBuilder);

            // Configurazione relazione 1:N tra ApplicationUser (Author) e Joke
            modelBuilder.Entity<Joke>()
                .HasOne(j => j.Author)                    // Navigazione Joke → ApplicationUser
                .WithMany(u => u.Jokes)                   // Navigazione ApplicationUser → ICollection<Joke>
                .HasForeignKey(j => j.ApplicationUserId)  // Foreign key sulla Joke
                .IsRequired()                             // Ogni Joke deve avere un autore
                .OnDelete(DeleteBehavior.Cascade);        // Se l'utente viene eliminato, elimina anche le sue Joke
        }
    }
}
```

### Note principali sulla versione finale

* **Base class**: `IdentityDbContext<ApplicationUser>`

  * integra tutta l’infrastruttura di ASP.NET Core Identity;
  * evita la configurazione manuale di tutte le tabelle utente/ruoli.

* **DbSet di dominio**:

  * `DbSet<Joke> Jokes` → tabella `Jokes` in PostgreSQL;
  * naming coerente: entità singolare (`Joke`), DbSet plurale (`Jokes`).

* **`OnModelCreating`**:

  * `base.OnModelCreating(modelBuilder)` è fondamentale per non rompere la configurazione di Identity;
  * la relazione `HasOne` / `WithMany` rende esplicito il legame tra Joke e ApplicationUser;
  * `OnDelete(DeleteBehavior.Cascade)` specifica che **la cancellazione di un utente comporta la cancellazione delle sue barzellette** (scelta di dominio deliberata).

---

## 4. Il costruttore e `DbContextOptions<JokesDbContext>`

Il costruttore:

```csharp
public JokesDbContext(DbContextOptions<JokesDbContext> options)
    : base(options)
{
}
```

segue il **pattern standard EF Core**:

* `DbContextOptions<JokesDbContext>` contiene:

  * connection string;
  * provider (PostgreSQL tramite `UseNpgsql`);
  * impostazioni di logging;
  * eventuali configurazioni avanzate;
* è registrato in **Dependency Injection** in `Program.cs` tramite `AddDbContext<JokesDbContext>(...)`;
* ASP.NET Core si occupa di:

  * creare l’istanza del DbContext per ogni richiesta HTTP (`Scoped`);
  * iniettarla nei controller;
  * rilasciarla a fine richiesta.

Questo garantisce:

* **thread safety** (un contesto per richiesta);
* gestione corretta delle connessioni;
* integrazione naturale nel ciclo di vita dell’applicazione.

---

## 5. Configurazione della relazione `Joke` ↔ `ApplicationUser`

### 5.1 Il modello `Joke` (lato dominio)

Nella classe `Joke` sono presenti:

* la foreign key esplicita:

```csharp
[Required]
[StringLength(450)]
public string ApplicationUserId { get; private set; } = string.Empty;
```

* la proprietà di navigazione:

```csharp
[ForeignKey(nameof(ApplicationUserId))]
[JsonIgnore]
public ApplicationUser? Author { get; private set; }
```

Questo lato del dominio dichiara l’esistenza della relazione.

### 5.2 Il modello `ApplicationUser` (lato utente)

In `ApplicationUser` è presente la collezione:

```csharp
public ICollection<Joke> Jokes { get; set; } = new List<Joke>();
```

Questo definisce la relazione **uno-a-molti**:
*un utente può avere molte barzellette*.

### 5.3 Il ruolo del DbContext (Fluent API)

Nel `JokesDbContext`, la Fluent API:

```csharp
modelBuilder.Entity<Joke>()
    .HasOne(j => j.Author)
    .WithMany(u => u.Jokes)
    .HasForeignKey(j => j.ApplicationUserId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

serve a:

* rendere la relazione **esplicita e chiara**;
* evitare ambiguità nella convenzione di EF Core;
* specificare in modo netto il comportamento alla cancellazione;
* centralizzare la logica di mapping **in un unico punto** (il DbContext).

---

## 6. Integrazione con `Program.cs` e Dependency Injection

La registrazione del DbContext avviene in `Program.cs` (lato server):

```csharp
using JokesApp.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddDbContext<JokesDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Identity + ApplicationUser + JokesDbContext
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<JokesDbContext>()
    .AddDefaultTokenProviders();

// ...
```

Punti importanti:

* `UseNpgsql(...)` → provider PostgreSQL;
* connection string letta da `appsettings.json`, **mai hardcoded**;
* `AddEntityFrameworkStores<JokesDbContext>()` → Identity utilizza proprio questo DbContext per persistere utenti, ruoli, ecc.

---

## 7. Best practices applicate

1. **Single DbContext principale**
   Un unico `JokesDbContext` per:

   * Identity;
   * entità di dominio (`Joke`, in futuro altre);
   * relazioni tra utenti e dati applicativi.

2. **Naming coerente**

   * Classe: `JokesDbContext` (PascalCase, suffisso `DbContext`);
   * DbSet: `Jokes` (plurale);
   * Entità: `Joke`, `ApplicationUser` (singolare).

3. **DataAnnotations + Fluent API**

   * Regole di validazione di dominio e vincoli base via attributi (`[Required]`, `[MaxLength]`, `[StringLength]`, ecc.);
   * relazioni e mapping più complessi via Fluent API in `OnModelCreating`.

4. **IdentityDbContext come base**

   * evita duplicazioni;
   * si integra perfettamente con Identity;
   * facilita migrazioni e manutenzione.

5. **Cascade delete consapevole**

   * `OnDelete(DeleteBehavior.Cascade)` è una precisa scelta di dominio:

     * cancellando un utente si cancellano anche tutte le sue barzellette;
     * mantiene il database coerente (nessuna Joke orfana).

6. **Dependency Injection e scope**

   * `AddDbContext` con lifetime *Scoped* (default per web app);
   * un’istanza di DbContext per richiesta HTTP, che è la configurazione raccomandata da Microsoft.

---

## 8. Riepilogo concettuale

* `JokesDbContext` è il **centro nevralgico** di EF Core nel progetto.
* Ha **evoluto** la sua responsabilità:

  * da semplice `DbContext` solo per `Joke`
  * a `IdentityDbContext<ApplicationUser>` che integra utenti, ruoli e dominio.
* Mappa in modo esplicito la relazione:

  * `ApplicationUser (1)` → `Joke (N)`
* È registrato in `Program.cs` tramite `AddDbContext` e `AddIdentity`.
* Rappresenta una **implementazione allineata alle best practice** per app ASP.NET Core + EF Core + Identity + PostgreSQL.

---

Se vuoi, nel prossimo passo possiamo:

* documentare in un file separato **06_Migrations.md** la prima migration che ha materializzato questo modello nel database (tabelle Identity + tabella `Jokes` + relazione),
  oppure
* passare alla documentazione dei **test di integrazione/unit testing** che verificano il corretto funzionamento del `JokesDbContext`.
