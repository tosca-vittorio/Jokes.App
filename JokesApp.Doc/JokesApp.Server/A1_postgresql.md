# 📚 JokesApp.Doc/JokesApp.Server/A1_postgresql.md: Creazione e configurazione di PostgreSQL su Windows (JokesApp)

*Documentazione tecnica di progetto — setup locale di PostgreSQL e inizio integrazione con backend ASP.NET Core*

---

## 1. Scopo e contesto

Questo documento descrive la procedura eseguita per:

1. installare/verificare PostgreSQL su Windows (lato client `psql`);
2. creare **database** e **ruolo** dedicati;
3. configurare i **permessi** necessari affinché **EF Core** possa creare e gestire lo schema (incluse le migration);
4. collegare l’applicazione ASP.NET Core al database in modo **sicuro**, evitando il commit di credenziali in chiaro.

> Nota di principio: l’applicazione **non** deve connettersi con l’utente amministratore `postgres`, ma con un **ruolo di progetto** dedicato (principio del minimo privilegio).

---

## 2. Prerequisiti

- PostgreSQL installato su Windows e servizio in esecuzione.
- Accesso al client `psql` (path variabile in base all’installazione; es. `C:\PostgreSQL18\bin\psql.exe`).
- Credenziali dell’utente amministratore `postgres` (solo per operazioni iniziali: creazione database/ruoli/permessi).
- (Opzionale ma consigliato) pgAdmin per ispezione visuale.

> Nota “truth-first”: i prerequisiti relativi a EF Core (tool `dotnet-ef`, provider Npgsql EF, migrations) **non** fanno parte di A1.
> Verranno gestiti negli step successivi:
> - **B** (preflight tooling/provider, senza DbContext e senza migrations)
> - **07a** (DbContext + mapping + migrations su PostgreSQL reale)

---

## 3. Parametri di riferimento (setup di progetto)

| Elemento | Valore |
|---|---|
| Database | `jokes` |
| Ruolo EF Core (migrations) | `jokes_migrator` |
| Password (locale) | `<DB_PASSWORD>` |
| Host | `localhost` |
| Porta | `5432` |
| Schema target | `public` |

---

## 4. Creazione e configurazione del database (via `psql`)

### 4.1 Verifica versione di PostgreSQL (client)

Da prompt dei comandi (o PowerShell), nella cartella `bin` di PostgreSQL:

```cmd
"C:\PostgreSQL18\bin\psql.exe" --version
psql (PostgreSQL) 18.1
```

Questa verifica accerta che il client `psql` sia disponibile e consente di tracciare la versione usata nel setup locale.

---

### 4.2 Accesso come amministratore (`postgres`)

```cmd
cd "C:\PostgreSQL18\bin"
psql -U postgres
Inserisci la password per l'utente postgres:
psql (18.1)
```

> L’utente `postgres` viene usato **solo** per operazioni amministrative (creazione database/ruoli e assegnazione permessi).

---

### 4.3 Creazione ruolo di progetto e database

Eseguire in `psql`:

```sql
-- 1) Crea il ruolo di progetto con login (usato da EF Core per le migrations)
postgres=# CREATE USER jokes_migrator WITH PASSWORD '<DB_PASSWORD>';

-- 2) Crea il database
postgres=# CREATE DATABASE jokes;
```

> `CREATE USER` è equivalente (in pratica) a `CREATE ROLE ... WITH LOGIN`. È sufficiente per un ambiente locale.

---

### 4.4 Assegnazione privilegi di base sul database

```sql
postgres=# GRANT ALL PRIVILEGES ON DATABASE jokes TO jokes_migrator;
```

**Raccomandazione pratica (molto utile con EF Core):** rendere il ruolo di progetto proprietario del database, così da semplificare la gestione dei permessi (specialmente in ambiente di sviluppo).

> Consente all’utente `jokes_migrator` di gestire completamente il database `jokes`.

```sql
postgres=# ALTER DATABASE jokes OWNER TO jokes_migrator;
```

> L’ownership del database riduce significativamente la probabilità di errori di autorizzazione durante la creazione di tabelle/migration.

---

### 4.5 Uscita dalla sessione amministrativa e accesso come ruolo di progetto (`jokes_migrator`)

Dopo aver creato database e ruolo con l’utente amministratore `postgres`, si prosegue con il ruolo di progetto `jokes_migrator`.
Questo rende i passaggi coerenti con l’identità effettivamente usata da EF Core per migrations e creazione/aggiornamento dello schema.

```cmd
\q
C:\PostgreSQL18\bin> psql -U jokes_migrator -d jokes
Inserisci la password per l'utente jokes_migrator:
psql (18.1)
```

> Da questo punto in avanti i comandi vengono eseguiti come `jokes_migrator`.

---

### 4.6 Permessi sullo schema `public` (risoluzione “permission denied for schema public”)

Se EF Core tenta di creare oggetti (es. `__EFMigrationsHistory`) e il ruolo non ha privilegi sullo schema, si verifica tipicamente l’errore:

* `permission denied for schema public`

Per risolvere in modo definitivo, eseguire:

```sql
-- Cambio il proprietario dello schema public
jokes=> ALTER SCHEMA public OWNER TO jokes_migrator;

-- Concedo tutti i privilegi sullo schema
jokes=> GRANT ALL ON SCHEMA public TO jokes_migrator;

-- Concedo tutti i privilegi su tutte le tabelle esistenti
jokes=> GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO jokes_migrator;

-- Concedo tutti i privilegi su tutte le sequenze esistenti
jokes=> GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO jokes_migrator;

-- Imposto i privilegi di default per le tabelle create in futuro
jokes=> ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO jokes_migrator;

-- Imposto i privilegi di default per le sequenze create in futuro
jokes=> ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO jokes_migrator;
```

**Effetto atteso:** il ruolo `jokes_migrator` può creare e gestire tabelle e sequenze in `public`, e EF Core può creare correttamente `__EFMigrationsHistory` e tutte le entità mappate.

---

### 4.7 Verifiche psql (opzionali)

In questa fase l’obiettivo principale è validare la **correttezza infrastrutturale** (database/ruolo/schema), senza assumere che lo strato di persistenza e le migrations EF Core siano già presenti. Dopo la configurazione di ruoli e permessi, in particolare, si verifica che:

- la connessione come `jokes_migrator` sia operativa;
- il ruolo disponga dei privilegi necessari sullo schema `public` per **creare e rimuovere oggetti** (condizione indispensabile per EF Core in seguito).

**Verifica permessi (senza EF Core, eseguire come `jokes_migrator`):**

```sql
-- Idempotenza: se esiste già, la rimuovo
jokes=> DROP TABLE IF EXISTS public.__perm_test;

-- Deve riuscire: creazione oggetto nello schema target (public)
jokes=> CREATE TABLE public.__perm_test (id int);

-- Pulizia
jokes=> DROP TABLE public.__perm_test;
```

**Verifica successiva (quando lo strato di persistenza e le migrations saranno pronte):**
una volta introdotto il `DbContext`, completato il mapping e create le migrations, il comando:

```bash
dotnet ef database update
```

dovrà generare almeno:

* la tabella `__EFMigrationsHistory`;
* le tabelle definite dalle migrations del progetto.

#### Comandi psql utili

```psql
-- Schemi
\dn+

-- Tabelle nello schema public
\dt

-- Database presenti
\l

-- Ruoli/utenti
\du

-- Info connessione corrente
\conninfo
```

---

## 5. Configurazione in pgAdmin (opzionale)

Per aggiungere un server in pgAdmin:

* **Name:** PostgreSQL 18 (o nome descrittivo)
* **Host:** `localhost`
* **Port:** `5432`

Per attività amministrative puoi usare `postgres`, ma per verificare l’esperienza reale dell’applicazione è consigliato collegarsi con:

* **Username:** `jokes_migrator`
* **Password:** `<DB_PASSWORD>`

> In questo modo validi che il ruolo di progetto abbia effettivamente i permessi necessari.

---

## 6. Configurazione connessione in ASP.NET Core (senza segreti nel repository)

### 6.1 Obiettivo di sicurezza

* evitare password in chiaro in `appsettings.json`;
* evitare commit accidentali di credenziali;
* mantenere una configurazione semplice per sviluppo locale e distribuibile in ambienti diversi (dev/test/prod).

---

### 6.2 Configurazione locale tramite variabili d’ambiente (segreti fuori dal repository)

In ambiente di sviluppo locale, i parametri sensibili (in particolare **password** e/o **connection string**) devono essere mantenuti **fuori dal controllo versione**, evitando qualunque inserimento accidentale in `appsettings.json` o in file tracciati da Git.

La strategia adottata è l’uso di un file `.env` locale:

* **non versionato** (incluso in `.gitignore`);
* caricato all’avvio del backend per popolare le **variabili d’ambiente** del processo;
* utilizzato dal sistema di configurazione di ASP.NET Core per **sovrascrivere** i valori presenti nei file `appsettings.json`.

Questo approccio garantisce:

* separazione netta tra **configurazione** e **codice**;
* portabilità su ambienti diversi (dev/test/prod) dove le variabili possono essere fornite dal sistema di deploy;
* riduzione del rischio di leakage di credenziali (repository pubblico/privato, log, PR, ecc.).

> Nota tecnica (convenzione ASP.NET Core): la configurazione da variabili d’ambiente supporta chiavi gerarchiche usando `__` (doppio underscore) come separatore.
> Esempio: `ConnectionStrings__JokesDb` viene interpretata come `ConnectionStrings:JokesDb`.

Nel progetto sono documentati due approcci concettuali:

* **Approccio A (consigliato / standard .NET):** fornire direttamente la connection string tramite `ConnectionStrings__JokesDb` (override nativo della configurazione).
* **Approccio B (parametrico):** definire variabili atomiche `DB_*` e comporre la connection string nel codice.

Nello **setup effettivamente adottato**, i due approcci coesistono nel file `.env` per finalità diverse:

* `ConnectionStrings__JokesDb` è la **sorgente di verità operativa** (runtime + EF Core).
* Le variabili `DB_*` sono mantenute come **parametri atomici di supporto** (leggibilità, debug, riuso manuale con tool), ma **non costituiscono un secondo canale di configurazione** finché la stringa non viene composta nel codice.

---

#### Approccio A (standard .NET): override della connection string completa

Definire la variabile:

```env
ConnectionStrings__JokesDb=Host=localhost;Port=5432;Database=jokes;Username=jokes_migrator;Password=<DB_PASSWORD>
```

Caratteristiche:

* è l’approccio più allineato alle convenzioni di ASP.NET Core;
* consente all’applicazione di leggere la stringa con `GetConnectionString("JokesDb")`;
* è compatibile con l’esecuzione delle migrations EF Core, purché la variabile sia disponibile al processo che esegue `dotnet ef`.

---

#### Approccio B (parametrico): variabili `DB_*` e composizione nel codice

Definire variabili atomiche:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=jokes
DB_USER=jokes_migrator
DB_PASSWORD=<DB_PASSWORD>
```

Caratteristiche:

* rende espliciti i singoli parametri e può risultare più leggibile/manutenibile;
* richiede però un passaggio applicativo aggiuntivo: la connection string deve essere **costruita a runtime** (o in fase di startup) leggendo `DB_*`, altrimenti queste variabili rimangono “informative” e non influenzano la connessione.

---

#### Esempio reale adottato nel progetto (source of truth + supporto)

Nel progetto si utilizza un `.env` coerente con entrambi gli approcci, stabilendo esplicitamente una **sorgente di verità** per evitare ambiguità:

```env
# PostgreSQL (parametri atomici di supporto / leggibilità)
DB_HOST=localhost
DB_PORT=5432
DB_NAME=jokes
DB_USER=jokes_migrator
DB_PASSWORD=<DB_PASSWORD>

# Sorgente di verità operativa per ASP.NET Core (override di ConnectionStrings:JokesDb)
ConnectionStrings__JokesDb=Host=localhost;Port=5432;Database=jokes;Username=jokes_migrator;Password=<DB_PASSWORD>
```
> Questo file `.env` è locale e **gitignored**: non va mai committato.

**Regola di coerenza:** se sono presenti sia `DB_*` sia `ConnectionStrings__JokesDb`, i valori devono rimanere **allineati** (stesso host/porta/database/utente/password). In caso contrario si introduce un rischio concreto di configurazioni divergenti e diagnostica più complessa (l’app “sembra” configurata, ma si connette usando l’altro set di valori).

**Riduzione della ridondanza (opzionale):**

* Se non componi la stringa nel codice, puoi scegliere di mantenere **solo** `ConnectionStrings__JokesDb` e rimuovere `DB_*`.
* Se vuoi invece usare solo `DB_*`, allora devi implementare in modo esplicito la composizione della stringa e assegnarla alla configurazione/DI, rendendo `DB_*` l’unica sorgente di verità.

---

### 6.3 `appsettings.json` (valore di fallback)

È possibile mantenere un fallback in `appsettings.json` (senza segreti reali), sapendo che in locale potrà essere sovrascritto dalle env vars:

```json
{
  "ConnectionStrings": {
    "JokesDb": "Host=localhost;Port=5432;Database=jokes;Username=jokes_migrator;Password=<DB_PASSWORD>"
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

In pratica:

* in locale: `.env` può sovrascrivere `JokesDb` (es. `ConnectionStrings__JokesDb=...`);
* in altri ambienti: si imposta la variabile `ConnectionStrings__JokesDb` nel sistema di deploy (CI/CD, container, hosting, ecc.).

---