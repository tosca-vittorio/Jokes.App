# 📘 **06_Migrations.md**

## *Evoluzione delle Migrazioni in JokesApp con Entity Framework Core*

---

## 🧩 **0. Introduzione generale**

Le **migrazioni** in Entity Framework Core rappresentano il meccanismo attraverso cui:

* l’evoluzione delle **classi di dominio** (entity)
* viene tradotta in **modifiche strutturali nel database**.

Ogni migration è un *fotogramma* del modello dati in un preciso momento, e una sequenza ordinata di esse racconta la storia evolutiva dell’applicazione.

Nel tuo progetto **JokesApp**, la gestione delle migrazioni è stata inizialmente orientata alla sperimentazione, e infatti hai creato **una sola migration reale: `InitialCreate`**, che rifletteva il dominio *nella sua forma originaria* (solo la tabella `Jokes`).

Successivamente il dominio è cresciuto molto, introducendo:

* `ApplicationUser`
* ASP.NET Core Identity
* relazioni uno-a-molti
* validazioni
* Fluent API
* aggiornamenti strutturali importanti

…ma nessuna migration è stata generata da allora, per tua scelta consapevole: **stabilizzare il codice prima di creare una nuova migration completa e definitiva**.

Questo documento spiega **l’evoluzione reale e teorica** delle migrazioni nel tuo progetto.

---

# 🟦 **1. Fase 1 — La migration reale: `InitialCreate`**

La migrazione `20251201135759_InitialCreate` è l’unica migration effettivamente creata tramite:

```bash
dotnet ef migrations add InitialCreate
```

Essa rifletteva il dominio nella sua forma più semplice:
solo l’entità `Joke`, senza Identity, senza ApplicationUser, senza relazioni.

---

## 🔍 **1.1 Contesto della fase**

Al momento della migration, il modello `Joke` era composto da:

* `Id`
* `Question`
* `Answer`
* `CreatedAt`

Non esistevano:

* ApplicationUser
* IdentityDbContext
* foreign key
* relazioni
* proprietà evolute come `UpdatedAt`

Pertanto la migration ha generato **una sola tabella**:

```
Jokes
```

---

## 🧱 **1.2 Struttura tabella generata**

### Tabella *Jokes* (ASCII Diagram)

```
┌─────────────────────┬───────────────────────────────┐
│       COLUMN         │            TYPE                │
├─────────────────────┼───────────────────────────────┤
│ Id                  │ integer (PK, auto-increment)   │
│ Question            │ character varying(200)         │
│ Answer              │ character varying(500)         │
│ CreatedAt           │ timestamp with time zone       │
└─────────────────────┴───────────────────────────────┘
```

---

## 🧬 **1.3 Analisi del file `InitialCreate.cs`**

La migration effettua due operazioni:

### **UP()**

```csharp
migrationBuilder.CreateTable(
    name: "Jokes",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("Npgsql:ValueGenerationStrategy", 
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
        Question = table.Column<string>(maxLength: 200, nullable: false),
        Answer = table.Column<string>(maxLength: 500, nullable: false),
        CreatedAt = table.Column<DateTime>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Jokes", x => x.Id);
    });
```

**Cosa significa:**

* EF genera automaticamente la chiave primaria.
* I limiti di lunghezza provengono dai DataAnnotations.
* `CreatedAt` viene mappato come timestamp UTC.
* Nessuna FK perché non esisteva alcuna relazione utente → Joke.

### **DOWN()**

```csharp
migrationBuilder.DropTable(name: "Jokes");
```

---

## 📸 **1.4 Model Snapshot (`JokesDbContextModelSnapshot.cs`)**

Questo file rappresenta *il modello dati completo* al momento della migration.

Conferma che:

* esiste UN SOLO DbSet: `Jokes`
* nessuna traccia di ApplicationUser
* nessun riferimento a Identity
* lo schema era minimal

---

# 🟩 **2. Fase 2 — Evoluzione del dominio DOPO la migration**

Dopo `InitialCreate`, il progetto è passato da una struttura semplice a un modello molto più ricco.

## 👤 2.1 Introduzione di `ApplicationUser`

Hai creato:

* un modello utente personalizzato (`ApplicationUser`)
* proprietà avanzate (`DisplayName`, `AvatarUrl`, `CreatedAt`, `UpdatedAt`)
* validazioni tramite attributi
* override delle proprietà Identity

## 🔐 2.2 Introduzione di ASP.NET Core Identity

Il DbContext è diventato:

```csharp
public class JokesDbContext : IdentityDbContext<ApplicationUser>
```

Questo cambiamento ha enorme impatto architetturale:

* vengono introdotte **tutte** le tabelle Identity:

```
AspNetUsers
AspNetRoles
AspNetUserClaims
AspNetRoleClaims
AspNetLogins
AspNetUserRoles
AspNetUserTokens
```

* ApplicationUser diventa l’entità centrale del dominio utente
* il DbContext ora gestisce due grandi categorie:

  * dominio (Joke)
  * sicurezza/autenticazione (Identity)

---

# 🟨 **3. Fase 3 — Relazione 1:N tra ApplicationUser e Joke**

Quando hai introdotto la proprietà:

```csharp
public ICollection<Joke> Jokes { get; set; }
```

e nel modello `Joke`:

```csharp
public string ApplicationUserId { get; private set; }
public ApplicationUser? Author { get; private set; }
```

si è stabilita la relazione:

```
1 ApplicationUser → N Jokes
```

---

## 🔧 **3.1 Configurazione Fluent API (attuale)**

Nel DbContext:

```csharp
modelBuilder.Entity<Joke>()
    .HasOne(j => j.Author)
    .WithMany(u => u.Jokes)
    .HasForeignKey(j => j.ApplicationUserId)
    .IsRequired()
    .OnDelete(DeleteBehavior.Cascade);
```

---

## 🧩 **3.2 ASCII Diagram del modello attuale**

```
            ┌───────────────────────────────┐
            │        AspNetUsers            │
            │ (ApplicationUser esteso)      │
            ├───────────────┬───────────────┤
            │ Id (PK)       │ Email         │
            │ DisplayName   │ AvatarUrl     │
            │ CreatedAt     │ UpdatedAt     │
            └───────┬───────┴───────────────┘
                    │ 1
                    │
                    │
                    ▼ N
          ┌───────────────────────────────┐
          │             Jokes             │
          ├───────────────────────────────┤
          │ Id (PK)                       │
          │ Question                      │
          │ Answer                        │
          │ CreatedAt                     │
          │ ApplicationUserId (FK)        │
          └───────────────────────────────┘
```

Questo schema rappresenta fedelmente:

* modello attuale
* relazione
* tabella utenti Identity
* tabella `Jokes` aggiornata

---

# 🟪 **4. Perché NON hai generato altre migrazioni (e perché è corretto)**

Hai preso una decisione **architettonicamente intelligente**:

⚠️ Non creare una migration ogni volta che modifichi i modelli.

Hai aspettato di:

* definire completamente `Joke`
* definire completamente `ApplicationUser`
* integrare Identity
* consolidare il DbContext
* stabilizzare le validazioni
* decidere la struttura finale

Solo quando **il modello reale è stabile** vale la pena generare la nuova migration definitiva.

Questo evita:

❌ proliferazione di migrations inutili
❌ migrazioni parziali o incoerenti
❌ rollback complicati
❌ difficoltà a mantenere un progetto pulito

E soprattutto evita la cosa più pericolosa:

👉 il database desincronizzato rispetto al dominio.

Hai fatto bene.
Questa è una decisione da sviluppatore maturo.

---

# 🟥 **5. 📌 *Promemoria compatto e chiaro***

---

## 🔔 **Nota importante per lo sviluppo futuro**

La migration `InitialCreate` **NON riflette più il modello attuale**, che oggi include:

* ApplicationUser avanzato
* Identity
* relazione 1–N
* modifiche a Joke
* Fluent API
* nuove proprietà e vincoli

Quando (e solo quando):

* avremo completato i prossimi moduli del backend,
* avrai verificato la struttura finale delle entità,
* il dominio sarà completamente definito,

👉 **sarà necessario generare UNA NUOVA migration completa**, che materializzerà nel database:

* tutte le tabelle Identity
* la tabella Jokes aggiornata
* la relazione e le foreign key
* gli indici
* i vincoli corretti

Esempio del comando futuro:

```bash
dotnet ef migrations add FinalModel
dotnet ef database update
```

---

# 🟫 **6. Procedura Tecnica: Gestione degli Strumenti, Installazione dei Pacchetti, Creazione della Migrazione e Operazioni di Verifica**

Questa sezione fornisce una guida operativa completa e consolidata che descrive, in modo professionale e approfondito, tutti i passaggi necessari per:

* installare gli strumenti corretti (`dotnet-ef` CLI e pacchetti NuGet),
* verificare la configurazione del progetto,
* creare la migrazione iniziale,
* applicarla al database PostgreSQL,
* eseguire test di connessione,
* realizzare backup e snapshot del progetto.

È un riferimento pratico universale da utilizzare ogni volta che si interviene sul database o sull’infrastruttura del backend.

---

## **6.1 Verifica e installazione della CLI EF Core (`dotnet-ef`)**

La CLI `dotnet-ef` è indispensabile per poter generare migrations, aggiornare il database e ispezionare lo stato del modello EF Core.

### **1️⃣ Verifica della presenza dello strumento**

Apri **PowerShell** (NON la Package Manager Console di Visual Studio) ed esegui:

```powershell
dotnet tool list -g
```

Se nello strumento globale non compare `dotnet-ef`, installalo con:

```powershell
dotnet tool install --global dotnet-ef
```

### **2️⃣ Verifica del corretto funzionamento**

```powershell
dotnet ef
```

Se appare l’elenco dei comandi disponibili, l’installazione è corretta.

> ⚠️ Senza `dotnet-ef` i comandi di migration NON funzionano, anche se i pacchetti NuGet sono installati.

---

## **6.2 Installazione dei pacchetti NuGet necessari**

Il progetto deve includere sia il provider PostgreSQL, sia gli strumenti EF Core lato build, sia i metapacchetti necessari al runtime.

Puoi installarli tramite NuGet Package Manager GUI oppure tramite CLI.

### **PowerShell (metodo raccomandato):**

```powershell
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### **Descrizione dei pacchetti**

| Pacchetto                                   | Funzione                                  |
| ------------------------------------------- | ----------------------------------------- |
| **Npgsql.EntityFrameworkCore.PostgreSQL**   | Provider EF Core per PostgreSQL           |
| **Microsoft.EntityFrameworkCore.Tools**     | Necessario per `dotnet ef`                |
| **Microsoft.EntityFrameworkCore.Design**    | Supporto alle migrations in fase di build |
| **AspNetCore.Identity.EntityFrameworkCore** | Integrazione Identity ↔ EF Core           |

### **Verifica installazione pacchetti**

```powershell
dotnet list package
```

Controlla che tutti siano presenti.

---

## **6.3 Creazione della migrazione iniziale**

Portati nella cartella del progetto server (dove si trova il `.csproj`):

```powershell
cd .\JokesApp.Server\
```

Genera la prima migration:

```powershell
dotnet ef migrations add InitialCreate
```

Output atteso:

```
Done. To undo this action, use 'dotnet ef migrations remove'
```

La migration verrà salvata in:

```
JokesApp.Server/Migrations/
```

> 🔹 Puoi annullare l’ultima migration (se non applicata) con:

```powershell
dotnet ef migrations remove
```

---

## **6.4 Applicare la migrazione al database**

Esegui:

```powershell
dotnet ef database update
```

Questo:

* crea il database se non esiste,
* crea tutte le tabelle definite nella migration,
* sincronizza schema DB ↔ modello EF Core.

Se la connessione è errata, EF genererà un errore leggibile in console.

---

## **6.5 Verifica manuale della struttura tramite PostgreSQL**

Apri `psql`, `pgAdmin` o DBeaver, seleziona il database (`jokesdb`) ed esegui:

### Lista tabelle:

```sql
\dt
```

### Struttura tabella:

```sql
\d "Jokes"
```

### Contenuto tabella:

```sql
SELECT * FROM "Jokes";
```

---

## **6.6 Inserimento dati di test (opzionale)**

Per verificare la scrittura nel database:

```sql
INSERT INTO "Jokes" ("Question", "Answer", "CreatedAt")
VALUES ('Perché il programmatore odia la natura?', 'Troppi bug!', NOW());
```

Controlla:

```sql
SELECT * FROM "Jokes";
```

---

## **6.7 Verifica runtime della connessione dal backend**

Dopo aver registrato correttamente `JokesDbContext` in *Program.cs*, puoi aggiungere un test di connessione:

```csharp
var app = builder.Build();

// Test della connessione al database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JokesDbContext>();
    var jokeCount = context.Jokes.Count();
    Console.WriteLine($"Numero di barzellette presenti: {jokeCount}");
}
```

### **Esecuzione in PowerShell**

```powershell
dotnet run
```

Output atteso:

```
Numero di barzellette presenti: 1
```

Se appare un errore, significa che la connessione non è configurata correttamente.

---

## **6.8 Backup e checkpoint del database**

Prima di continuare lo sviluppo, è buona pratica creare un backup.

### **Backup tramite `pg_dump` (percorso relativo)**

```powershell
pg_dump -U postgres -d jokesdb -F c -f ".\Server_Backup\BackupSQL\jokesdb_checkpoint.backup"
```

### **Backup con timestamp automatico**

```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
pg_dump -U postgres -d jokesdb -F c -f ".\Server_Backup\BackupSQL\jokesdb_$timestamp.backup"
```

---

## **6.9 Snapshot del progetto (backup del codice)**

Script PowerShell che crea uno snapshot completo del progetto:

```powershell
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupBase = ".\Server_Backup\ProjectSnapshot"

if (-not (Test-Path $backupBase)) {
    New-Item -ItemType Directory -Path $backupBase
}

$snapshotFolder = Join-Path $backupBase "Snapshot_$timestamp"
New-Item -ItemType Directory -Path $snapshotFolder

$itemsToCopy = @(
    ".\Program.cs",
    ".\Data",
    ".\Models",
    ".\appsettings.json",
    ".\appsettings.Development.json"
)

foreach ($item in $itemsToCopy) {
    if (Test-Path $item) {
        Copy-Item $item $snapshotFolder -Recurse -Force
    }
}

Write-Host "Snapshot creata in: $snapshotFolder"
```

Lo snapshot permette di ripristinare facilmente il progetto in caso di errori critici.

---
