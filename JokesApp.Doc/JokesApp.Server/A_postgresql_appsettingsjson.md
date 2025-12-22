# 📚 Creazione e Configurazione Database PostgreSQL su Windows

*(Documentazione tecnica ufficiale del progetto)*
---

## 1️⃣ Configurare Database per il progetto `ASP.NET` + `React` con `PostgreSQL 18`

### 1. Verifica della versione di PostgreSQL

```cmd
C:\PostgreSQL18\bin>psql --version
psql (PostgreSQL) 18.1
```

> Controlla quale versione del client `psql` è installata. Utile per assicurarsi che PostgreSQL sia presente e aggiornato.

---

### 2. Accesso al database come utente amministratore

```cmd
C:\PostgreSQL18\bin>psql -U postgres
Inserisci la password per l'utente postgres:
psql (18.1)
```

> Connettiti al database PostgreSQL come utente `postgres`.

---

### 3. Creazione di un nuovo database

```sql
postgres=# CREATE DATABASE jokesdb;
```

> Crea un database chiamato `jokesdb`.
> Ricorda di terminare sempre i comandi SQL con `;`.

---

### 4. Creazione di un nuovo utente (role)

```sql
postgres=# CREATE USER toscazuser WITH PASSWORD 'toscaz';
```

> Crea un nuovo utente (`toscazuser`) con password `toscaz`.

---

### 5. Assegnazione dei privilegi sul database

```sql
postgres=# GRANT ALL PRIVILEGES ON DATABASE jokesdb TO toscazuser;
```

> Consente all’utente `toscazuser` di gestire completamente il database `jokesdb`.

---

### 6. Chiusura della sessione `postgres` e login su `jokesdb`

```cmd
\q
C:\PostgreSQL18\bin>psql -U postgres -d jokesdb
Inserisci la password per l'utente postgres:
psql (18.1)
```

> Ora ci connettiamo **direttamente al database `jokesdb`**.
> Questo è fondamentale perché alcune operazioni sugli schemi devono essere eseguite all’interno del database stesso.

---

### 7. Assegnazione dei privilegi sullo schema `public`

```sql
-- Cambia il proprietario dello schema public
jokesdb=# ALTER SCHEMA public OWNER TO toscazuser;

-- Concedi tutti i privilegi sullo schema
jokesdb=# GRANT ALL ON SCHEMA public TO toscazuser;

-- Concedi tutti i privilegi su tutte le tabelle esistenti
jokesdb=# GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO toscazuser;

-- Concedi tutti i privilegi su tutte le sequenze esistenti
jokesdb=# GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO toscazuser;

-- Imposta i privilegi di default per le tabelle create in futuro
jokesdb=# ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO toscazuser;

-- Imposta i privilegi di default per le sequenze create in futuro
jokesdb=# ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO toscazuser;
```

> Questi passaggi **risolvono definitivamente l’errore “permesso negato per lo schema public”** che EF Core dava in precedenza.
> Ora l’utente `toscazuser` ha pieno controllo dello schema `public`, e EF Core può creare la tabella `__EFMigrationsHistory` e tutte le altre tabelle senza problemi.

---

### 8. Verifica delle tabelle e schemi

```sql
-- Lista degli schemi
\dn+

-- Lista delle tabelle nello schema public
\dt

-- Lista dei database
\l

-- Lista degli utenti/ruoli
\du
```

> Controlla che il database, gli utenti e le tabelle siano stati creati correttamente.
> In particolare, nello schema `public` dovresti vedere la tabella `__EFMigrationsHistory` dopo aver eseguito `dotnet ef database update`.

---

## 2️⃣ Configurazione in PgAdmin

* Aggiungi nuovo server:

  * **Name:** PostgreSQL 18
  * **Host:** localhost
  * **Port:** 5432
  * **Username:** postgres
  * **Password:** toscaz

---

## 3️⃣ Configurare la stringa di connessione in ASP.NET

### 3.1 Creazione di un file `.env`

Per **proteggere le credenziali sensibili**, come la password del database, possiamo utilizzare un file `.env`. Questo approccio permette di non salvare la password in chiaro nel codice o nei file JSON del progetto, rendendo l’applicazione più sicura e pronta anche per ambienti di produzione.

1. Crea un file nella root del progetto backend chiamato `.env`.
2. Aggiungi la tua password PostgreSQL come variabile di ambiente:

```
DB_PASSWORD=toscaz
```

> ⚠️ Non caricare mai `.env` nei repository pubblici. Aggiungi il file a `.gitignore` per sicurezza.

---

### 3.2 Aggiornare `appsettings.json` per leggere dal `.env`

Modifica il tuo `appsettings.json` così:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jokesdb;Username=toscazuser;Password=${DB_PASSWORD}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> Qui `${DB_PASSWORD}` verrà sostituito dalla variabile d’ambiente impostata nel file `.env`.

---

### 3.3 Caricare le variabili `.env` in ASP.NET Core

Per leggere le variabili d’ambiente dal file `.env`, aggiungi il pacchetto **DotNetEnv** (o simile) nel progetto:

```bash
dotnet add package DotNetEnv
```

Nel `Program.cs`, subito all’inizio della configurazione:

```csharp
using DotNetEnv;

Env.Load(); // Carica automaticamente le variabili dal file .env
```

Ora `builder.Configuration.GetConnectionString("DefaultConnection")` recupererà la password correttamente senza che sia scritta in chiaro nel progetto.

---

### 3.4 Quando e perché usare `.env`

* **Quando:** sempre che ci siano credenziali o informazioni sensibili (password, chiavi API) da non esporre direttamente nel codice.
* **Perché:** protezione della sicurezza, facilità di configurazione tra diversi ambienti (sviluppo, test, produzione), evita commit accidentali di password.
* **Come:** creare `.env`, leggere le variabili con `DotNetEnv` o il sistema integrato di ASP.NET Core (`Environment.GetEnvironmentVariable("DB_PASSWORD")`).

---

### 3.5 Verifica finale

1. Il backend deve riuscire a connettersi al database PostgreSQL.
2. La tabella `__EFMigrationsHistory` deve essere presente dopo `dotnet ef database update`.
3. La password non è visibile in chiaro nel repository o nel `appsettings.json`.


✅ Questo è corretto. Ora il backend ASP.NET sa come connettersi al database PostgreSQL.



