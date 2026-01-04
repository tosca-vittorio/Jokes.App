# 📘📚 JokesApp.Doc/JokesApp.Server/**A2b_Program.md — Program.cs (Bootstrap & Health)**

*Documentazione tecnica del file `Program.cs` nello stato A2b del progetto: bootstrap del backend senza Entity Framework, configurazione DB, pipeline minima e endpoint di diagnostica.*

---

## 1. **Obiettivo di questa fase**

La milestone **A2b** si concentra sull'avvio corretto dell'applicazione backend, gestendo la configurazione, la connessione al database, e l'esposizione di endpoint di diagnostica. Il nostro obiettivo è dimostrare che l'applicazione:

* Si avvia correttamente.
* Legge la configurazione da diverse fonti (variabili d'ambiente, file di configurazione, ecc.).
* Risolve la stringa di connessione al database in modo robusto, senza usare **Entity Framework** (EF).
* Espone endpoint di diagnostica (health e ping) senza coinvolgere EF Core.
* Funziona in modo sicuro con un fail-fast se la configurazione è errata.

Questa documentazione spiega come `Program.cs` implementa queste funzionalità, le scelte progettuali e come verificare il funzionamento.

---

## 2. **Caricamento delle variabili d’ambiente (DEV-only .env + env vars sempre in IConfiguration)**

### 2.1 Obiettivo (Timeline A2)

In A2 la configurazione deve seguire un principio preciso:

- **In Development** posso usare un file `.env` (local-first) per comodità e per non committare segreti.
- **In NON-DEV** (staging/production) **non devo** caricare `.env`: i valori devono arrivare dall’host (CI/CD, container, VM, cloud).
- **In ogni caso** le variabili d’ambiente devono essere visibili a `IConfiguration` (perché la risoluzione DB avviene tramite config + env).

### 2.2 Implementazione reale in `Program.cs`

Nel codice attuale:

1) viene dichiarato `envPath` (diagnostico) che resta `null` in NON-DEV;
2) in Development si cercano più percorsi possibili per `.env` (monorepo / working directory variabile);
3) se trovato, viene caricato con `DotNetEnv.Env.Load(envPath)`;
4) **sempre** viene chiamato `builder.Configuration.AddEnvironmentVariables()` per inserire le env vars nella pipeline di configurazione .NET.

```csharp
string? envPath = null;

if (builder.Environment.IsDevelopment())
{
    var candidatePaths = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        Path.Combine(builder.Environment.ContentRootPath, "..", ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), ".env")
    };

    envPath = candidatePaths.FirstOrDefault(File.Exists);

    if (envPath is not null)
    {
        Env.Load(envPath);
    }
}

builder.Configuration.AddEnvironmentVariables();
```

### 2.3 Perché tre percorsi diversi?

Questa scelta evita “falsi negativi” quando l’app viene avviata da directory diverse:

* `ContentRootPath` → tipico avvio dal progetto server
* `..` → tipico avvio dal root del monorepo con server in sottocartella
* `Directory.GetCurrentDirectory()` → copre avvii non standard / tool

### 2.4 Considerazioni di sicurezza

* `.env` è **ammesso solo in Development** (timeline A2).
* In produzione si assume che segreti e connection string arrivino dall’infrastruttura.
* `envPath` viene usato solo a scopo diagnostico (log “envFileLoaded” senza esporre contenuto).

### 2.5 Startup logger “early” (prima di `builder.Build()`)

Il `Program.cs` crea un logger anticipato tramite `LoggerFactory.Create(...)` perché:

- prima di `builder.Build()` non esiste ancora `app.Logger`;
- in A2 vogliamo loggare in modo controllato e “safe” la configurazione DB risolta (source + host/port/db/user, mai password).

```csharp
var startupLogger = LoggerFactory
    .Create(logging =>
    {
        logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        logging.AddConsole();
    })
    .CreateLogger("Startup");
```

Nota: l’uso di `AddConfiguration(builder.Configuration.GetSection("Logging"))` permette di rispettare eventuali impostazioni di logging presenti in `appsettings*`.

---

## 3. **Risoluzione della Connection String (priorità A2 + fail-fast + logging safe)**

### 3.1 Obiettivo (Timeline A2)

A2 richiede una risoluzione **deterministica** e **trasparente** della connection string verso PostgreSQL, con:

- **priorità esplicita** tra le fonti
- **fail-fast** in startup se la configurazione è assente o placeholder
- **logging safe**: niente segreti nei log, mai password in chiaro
- tracciamento della sorgente (`DbConnectionSource`) per diagnostica

### 3.2 Fonti e priorità (ordine reale del codice)

Il metodo `ResolveConnectionString(builder)` implementa esattamente questa catena:

**A) Env var preferita (massima priorità)**
- `ConnectionStrings__JokesDb`
- Motivo: perfetta per CI/CD e hosting (un solo valore atomico, facile da gestire).

**B) Fallback da config**
- `builder.Configuration.GetConnectionString("JokesDb")`
- È accettata **solo se non è un placeholder**.

**C) Composizione da variabili DB_* (ultima chance)**
- `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- Se uno di questi manca → configurazione “Missing”.

### 3.3 Perché la notazione `ConnectionStrings__JokesDb`?

In .NET le env vars possono rappresentare gerarchie usando `__` (doppio underscore).
Quindi:

- `ConnectionStrings__JokesDb` ↔ chiave gerarchica equivalente a `ConnectionStrings:JokesDb`.

Questo evita di dover definire file di configurazione diversi in produzione.

### 3.4 Placeholder detection (coerente col codice)

Il codice considera “placeholder/non valido” un valore che:

- è vuoto / null / whitespace
- contiene `<DB_PASSWORD>` oppure `${DB_PASSWORD}` (case-insensitive)

Questo è fondamentale: `appsettings.json` può contenere un valore volutamente “finto” e non sensibile, ma l’app **non deve** avviarsi se quello è l’unico valore disponibile.

### 3.5 Implementazione reale

```csharp
var (connectionString, source) = ResolveConnectionString(builder);
EnsureConnectionStringIsValid(connectionString);
LogSafeDbConfiguration(startupLogger, connectionString, source, envFileLoaded: envPath is not null);
```

* `ResolveConnectionString` restituisce anche `DbConnectionSource` per sapere da dove arriva il valore.
* `EnsureConnectionStringIsValid` applica la regola A2 di fail-fast.
* `LogSafeDbConfiguration` stampa solo campi non sensibili e indica se una password è presente.

### 3.6 Fail-fast: perché è obbligatorio in A2

Senza fail-fast, l’app potrebbe:

* avviarsi “a metà”
* fallire più tardi in modo confuso (errori runtime)
* rendere ambigua la diagnostica (non si capisce se il problema è DB/config/EF)

In A2 invece vogliamo: **errore subito, messaggio chiaro, nessun segreto**.

### 3.7 Logging safe: cosa si logga e cosa NON si logga

Il logging usa `NpgsqlConnectionStringBuilder` per parsare i campi in modo robusto:

* ✅ Host / Port / Database / Username
* ✅ `PasswordSet=true/false`
* ✅ `DbConnectionSource` e `EnvFileLoaded`
* ❌ Password (sempre mascherata)

Esempio concettuale (coerente con l’implementazione):

```text
DB connection source selected: EnvConnectionString. EnvFileLoaded=True. Host=localhost, Port=5432, Database=jokes, Username=jokes_migrator, PasswordSet=True
```

### 3.8 `DbConnectionSource`: perché esiste

L’enum rende il comportamento osservabile e documentabile:

* `EnvConnectionString` → env var `ConnectionStrings__JokesDb`
* `AppsettingsFallback` → `GetConnectionString("JokesDb")` non-placeholder
* `ComposedFromDbVars` → costruita da `DB_*`
* `Missing` → impossibile determinare una connessione valida

Questo migliora la diagnosi in DEV e mantiene sicurezza in NON-DEV.

---

## 4. **Registrazione dei servizi minimi (DI container)**

### 4.1 Obiettivo (Timeline A2)

In A2 registriamo solo ciò che serve per:

- tenere il backend avviabile
- esporre endpoint tecnici (health/ping)
- preparare l’app all’evoluzione futura (Step 10: controller “di prodotto”)

### 4.2 Servizi registrati

**Controllers (preparazione Step 10):**

```csharp
builder.Services.AddControllers();
```

Nota: in A2 non è necessario avere controller reali. La registrazione non espone nulla “di prodotto”, ma evita di dover rifare la pipeline più avanti.

**OpenAPI in Development (diagnostica DEV-only):**

Nel codice attuale viene usato il modello “minimal OpenAPI”:

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}
```

Questa scelta è coerente con A2: documentazione tecnica disponibile solo in DEV, non in ambienti reali.

---

## 5. **Pipeline HTTP environment-aware (DEV vs NON-DEV) + baseline HTTPS + static files**

### 5.1 Costruzione dell’app

Dopo aver configurato servizi e configurazione:

```csharp
var app = builder.Build();
```

Da questo punto in poi si configura la pipeline HTTP (middleware) e si mappano gli endpoint.

### 5.2 Gestione errori e sicurezza: DEV vs NON-DEV

**In Development:**

```csharp
app.UseDeveloperExceptionPage();
```

Motivo: stack trace e dettagli utili per debug locale.

**In NON-DEV (Production/Staging):**

```csharp
app.UseExceptionHandler("/error");
app.UseHsts();
```

* `UseExceptionHandler("/error")` evita di esporre dettagli tecnici a client/utenti.
* `UseHsts()` abilita HTTP Strict Transport Security (forza HTTPS nei client compatibili, tipico per ambienti reali).

### 5.3 Redirect HTTP → HTTPS

```csharp
app.UseHttpsRedirection();
```

Questo middleware tenta di reindirizzare automaticamente richieste HTTP verso HTTPS (coerente col fatto che il progetto usa profili di avvio HTTPS).

### 5.4 Static files e SPA hosting (baseline)

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();
```

* `UseDefaultFiles()` prova a risolvere documenti di default (es. `index.html`) senza specificarli esplicitamente.
* `UseStaticFiles()` abilita la servitura di file statici (tipicamente build del frontend).

Nota: questa parte è coerente con l’obiettivo di avere un backend che può ospitare anche contenuti SPA in futuro; in A2 resta una baseline “neutra” e non introduce logica di dominio.

---

## 6. **Endpoint tecnici (A2): health, readiness e ping DB (senza EF)**

### 6.1 Principio A2: endpoint tecnici, output minimale, sicurezza

In A2 esponiamo solo endpoint “tecnici” utili a:

- capire se il processo è vivo (liveness)
- capire se il DB è raggiungibile (readiness, senza EF)
- avere diagnostica più ricca *solo in DEV* (ping)

Gli endpoint **non devono** rivelare segreti (password, connection string completa, stack trace in NON-DEV).

---

### 6.2 `GET /health` — Liveness (sempre disponibile)

**Scopo:** conferma che il processo HTTP risponde.

**Implementazione:**

```csharp
app.MapGet("/health", () => Results.Ok(new { ok = true }));
```

**Contratto:**

* `200 OK`
* JSON minimale: `{ "ok": true }`

---

### 6.3 `GET /health/ready` — Readiness DB (sempre disponibile)

**Scopo:** verifica reachability DB eseguendo `SELECT 1` con `Npgsql`, senza introdurre EF Core.

**Implementazione (semplificata):**

```csharp
app.MapGet("/health/ready", async () =>
{
    try
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT 1;", conn);
        await cmd.ExecuteScalarAsync();

        return Results.Ok(new { ok = true });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});
```

**Contratto:**

* DB OK → `200 OK` con `{ "ok": true }`
* DB KO → `503 Service Unavailable` (output volutamente minimale)

---

### 6.4 `GET /api/db/ping` — Diagnostica DB (DEV-only)

**Scopo:** endpoint di diagnostica più ricca per distinguere:

* problemi di configurazione/credenziali
* problemi di rete DB
* future problematiche EF (che in A2 non esistono ancora)

**Disponibilità:** mappato **solo** quando `app.Environment.IsDevelopment()` è true.

**Contratto DEV:**

* `200 OK` con oggetto dettagliato che include:

  * `ok`, `result` (tipicamente 1)
  * `connectionSource`
  * `envFileLoaded`
  * `database`, `host`, `port`, `username`
* Password sempre mascherata (mai esposta)

**Contratto NON-DEV:**

* endpoint non mappato → `404 Not Found`

---

### 6.5 OpenAPI endpoint (DEV-only)

In Development il codice mappa anche OpenAPI:

```csharp
app.MapOpenApi();
```

Questo è coerente con A2: diagnostica disponibile localmente, non in produzione.

---

### 6.6 `/error` — endpoint “safe” per `UseExceptionHandler`

In NON-DEV, `UseExceptionHandler("/error")` richiede un endpoint che restituisca una risposta controllata:

```csharp
app.Map("/error", () =>
    Results.Problem(
        title: "Unhandled server error",
        statusCode: StatusCodes.Status500InternalServerError))
   .ExcludeFromDescription();
```

* niente stack trace
* niente dettagli tecnici
* escluso dalla descrizione OpenAPI (non è un endpoint di prodotto)

---

### 6.7 Controllers mapping e SPA fallback

Anche se in A2 non esistono ancora controller “di prodotto”, il mapping è già presente:

```csharp
app.MapControllers();
```

Infine, per routing client-side (SPA):

```csharp
app.MapFallbackToFile("index.html");
```

Se una route non viene gestita dal backend, viene servito `index.html` (utile per React Router e simili).

---

## 7. **Testing degli endpoint (PowerShell) + risultati attesi (DEV vs NON-DEV)**

### 7.1 Nota sulle porte

Le porte dipendono dal profilo in `launchSettings.json` (es. `7215` o `5129`) e dal protocollo (HTTP/HTTPS).  
Nei comandi sotto si assume HTTPS su `7215`. Adatta se necessario.

### 7.2 Test in Development (profilo `server-dev`)

```powershell
Invoke-RestMethod -Uri https://localhost:7215/health
Invoke-RestMethod -Uri https://localhost:7215/health/ready
Invoke-RestMethod -Uri https://localhost:7215/api/db/ping
```

**Risultati attesi (DEV):**

* `/health` → `200 OK` con `{ "ok": true }`
* `/health/ready` → `200 OK` con `{ "ok": true }` se DB raggiungibile, altrimenti `503`
* `/api/db/ping` → `200 OK` con oggetto dettagliato:
 ```bash
    ok               : True
    result           : 1
    connectionSource : EnvConnectionString
    envFileLoaded    : True
    database         : jokes
    host             : localhost
    port             : 5432
    username         : jokes_migrator
 ```

### 7.3 Test in NON-DEV (profilo `server-prod` o ambiente Production)

```powershell
Invoke-RestMethod -Uri https://localhost:7215/health
Invoke-RestMethod -Uri https://localhost:7215/health/ready
Invoke-RestMethod -Uri https://localhost:7215/api/db/ping
```

**Risultati attesi (NON-DEV):**

* `/health` → `200 OK` con `{ "ok": true }`
* `/health/ready` → `200 OK` con `{ "ok": true }` se DB raggiungibile, altrimenti `503`
* `/api/db/ping` → **404 Not Found** (endpoint non mappato fuori da Development)

Questa differenza è intenzionale e fa parte dei requisiti A2: diagnostica ricca solo in DEV.

---

## 8. **Conclusioni (stato A2b)**

Nel suo stato **A2b**, `Program.cs` realizza un bootstrap robusto, minimale e verificabile del backend **senza Entity Framework**, aderendo ai requisiti della timeline:

- separazione netta **DEV vs NON-DEV**
- `.env` **solo in Development** (local-first) con ricerca multi-path e flag diagnostico
- env vars sempre disponibili in `IConfiguration` (`AddEnvironmentVariables`)
- risoluzione connection string con priorità esplicita e tracciamento sorgente (`DbConnectionSource`)
- **fail-fast** se la configurazione DB è assente o placeholder
- logging “safe” tramite parsing (`NpgsqlConnectionStringBuilder`): niente password in chiaro
- pipeline con:
  - DeveloperExceptionPage in DEV
  - ExceptionHandler + HSTS in NON-DEV
  - HTTPS redirection sempre
  - static files + default document
- endpoint tecnici:
  - `/health` (liveness, sempre)
  - `/health/ready` (readiness DB via `SELECT 1`, sempre, output minimale)
  - `/api/db/ping` (DEV-only, output più ricco)
  - `/error` per gestione sicura errori in NON-DEV
- predisposizione per:
  - controller futuri (`MapControllers`)
  - routing SPA (`MapFallbackToFile("index.html")`)

**Cosa non è ancora parte di A2b (volutamente):**
- Entity Framework Core e migrazioni
- autenticazione/autorizzazione
- controller di dominio (“feature”)
- policy CORS, rate limiting, ecc.

Questa milestone è una base solida e documentabile sulla quale innestare gli step successivi della timeline (es. Step 10).

---

## Appendice A — Snapshot `Program.cs` (milestone A2b)

Questa appendice contiene il **codice completo** di `Program.cs` nello stato A2b, considerato milestone stabile e funzionante.

> Regola di manutenzione: quando `Program.cs` evolverà (es. Step 10), questa appendice **non viene riscritta**.  
> Verrà creata una nuova appendice o un nuovo documento (es. **A10_Program.md**) con le differenze e il nuovo snapshot.

```csharp
using DotNetEnv;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

#nullable enable

/// <summary>
/// Entry point dell'applicazione server.
///
/// Questo Program.cs implementa lo step A2 della tua timeline:
/// - bootstrap della configurazione (DEV vs NON-DEV)
/// - caricamento .env SOLO in Development (local-first)
/// - risoluzione connection string con priorità esplicita
/// - fail-fast se la config DB manca o è placeholder
/// - logging "safe" (mai password in chiaro)
/// - endpoints tecnici:
///   - /health (liveness, sempre)
///   - /health/ready (readiness DB via Npgsql SELECT 1, sempre, output minimale)
///   - /api/db/ping (DEV-only, diagnostica più ricca)
///
/// Scopo didattico: il file è volutamente molto commentato per spiegare la sintassi e le scelte.
/// </summary>
public static class Program
{
    /// <summary>
    /// Metodo di avvio (punto di ingresso) del processo.
    ///
    /// In .NET moderno (6+), potresti usare anche i "top-level statements",
    /// ma qui manteniamo una struttura classica (classe Program + Main)
    /// perché:
    /// - è più leggibile per imparare
    /// - consente helper methods separati e testabili mentalmente
    /// - si sposa bene con documentazione XML + regioni.
    /// </summary>
    public static void Main(string[] args)
    {
        #region Host builder (creazione del "builder")

        // WebApplication.CreateBuilder(args) crea:
        // - configurazione (Configuration)
        // - servizi DI container (Services)
        // - info ambiente (Environment: Development/Staging/Production)
        // - logging (Logging)
        //
        // In pratica: è la fase "pre-build", dove si registrano servizi e si prepara la config.
        var builder = WebApplication.CreateBuilder(args);

        #endregion

        #region A2.1 - .env DEV-only + env vars in IConfiguration

        // envPath: ci serve SOLO a scopo diagnostico (loggare se .env è stato trovato/caricato).
        // In NON-DEV resterà null, perché NON vogliamo caricare file .env in ambienti reali.
        string? envPath = null;

        // Regola A2: .env si carica solo in Development.
        // In staging/production le variabili devono arrivare dall'host (GitHub Actions, VM, container, cloud, ecc.).
        if (builder.Environment.IsDevelopment())
        {
            // Cerchiamo .env in più posizioni perché in un monorepo puoi avviare l'app
            // dalla root o dalla cartella del progetto.
            var candidatePaths = new[]
            {
                // 1) content root del progetto server
                Path.Combine(builder.Environment.ContentRootPath, ".env"),

                // 2) repo root (se il server è in sottocartella)
                Path.Combine(builder.Environment.ContentRootPath, "..", ".env"),

                // 3) working directory attuale (da dove lanci dotnet run)
                Path.Combine(Directory.GetCurrentDirectory(), ".env")
            };

            // FirstOrDefault(File.Exists) => prende il primo percorso che esiste davvero.
            envPath = candidatePaths.FirstOrDefault(File.Exists);

            // Se esiste, lo carichiamo con DotNetEnv.
            // DotNetEnv popola Environment.GetEnvironmentVariable("...") per la sessione corrente.
            if (envPath is not null)
            {
                Env.Load(envPath);
            }
        }

        // Importantissimo:
        // Anche se .env non è stato caricato, vogliamo che IConfiguration includa SEMPRE le env vars.
        // Questo consente:
        // - in DEV: usare variabili caricate da .env
        // - in NON-DEV: usare variabili fornite dal sistema di hosting
        builder.Configuration.AddEnvironmentVariables();

        #endregion

        #region Startup logger (logging "safe" prima di builder.Build)

        // Qui creiamo un logger "early" per loggare la configurazione risolta.
        // Usiamo LoggerFactory.Create(...) perché prima di builder.Build() non abbiamo ancora app.Logger.
        var startupLogger = LoggerFactory
            .Create(logging =>
            {
                // Se hai una sezione "Logging" in appsettings, la rispettiamo.
                logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
                logging.AddConsole();
            })
            .CreateLogger("Startup");

        #endregion

        #region A2.1 - Connection string: risoluzione + fail-fast + log safe

        // Risolviamo la connection string seguendo la priorità A2.
        // Torniamo anche la "source" (da dove arriva) per debug e trasparenza.
        var (connectionString, source) = ResolveConnectionString(builder);

        // Fail-fast: se non c'è configurazione DB valida, l'app deve fermarsi subito.
        // Questo evita di "partire male" e scoprire dopo errori confusi.
        EnsureConnectionStringIsValid(connectionString);

        // Logging safe: tracciamo source + host/port/db/user, MA MAI la password in chiaro.
        LogSafeDbConfiguration(
            startupLogger,
            connectionString,
            source,
            envFileLoaded: envPath is not null
        );

        #endregion

        #region Servizi minimi (DI container)

        // AddControllers è ammesso in A2:
        // - non "obbliga" ad avere controllers reali
        // - prepara il terreno per Step 10
        // In A2 però esponiamo solo endpoint tecnici via Minimal API.
        builder.Services.AddControllers();

        // OpenAPI: diagnostica "ricca" solo DEV.
        // Nota: tu stai usando AddOpenApi/MapOpenApi (ASP.NET 8 minimal OpenAPI).
        // Se nel tuo progetto non esistono, commenta queste righe
        // o sostituisci con Swagger classico (AddEndpointsApiExplorer + AddSwaggerGen).
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddOpenApi();
        }

        #endregion

        #region Build (creazione dell'app)

        // builder.Build() congela la configurazione e costruisce la WebApplication.
        // Da qui in poi:
        // - config e services sono "chiusi"
        // - si configura la pipeline HTTP (middleware) e si mappano gli endpoint.
        var app = builder.Build();

        #endregion

        #region A2.2 - Pipeline DEV vs NON-DEV (middleware)

        if (app.Environment.IsDevelopment())
        {
            // DEV: errori dettagliati per debug locale.
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // NON-DEV: gestione errori pulita e baseline di sicurezza.
            // UseExceptionHandler crea una pipeline che non espone stack trace agli utenti.
            app.UseExceptionHandler("/error");

            // HSTS: forza HTTPS (utile in ambienti reali).
            app.UseHsts();
        }

        // Reindirizza HTTP -> HTTPS (se disponibile).
        app.UseHttpsRedirection();

        // Static files (React build): UseDefaultFiles cerca index.html come default document.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        #endregion

        #region A2.3 - Endpoints tecnici (senza EF)

        // /health = Liveness
        // - Non tocca il DB
        // - Serve a capire se il processo è vivo e risponde
        // - Deve essere "safe" e stabile in tutti gli ambienti.
        app.MapGet("/health", () => Results.Ok(new { ok = true }));

        // /health/ready = Readiness
        // - Verifica se il DB è raggiungibile (Npgsql + SELECT 1)
        // - Output minimale per NON leakare dettagli in produzione.
        // - 200 se DB ok, 503 se DB non raggiungibile.
        app.MapGet("/health/ready", async () =>
        {
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT 1;", conn);
                await cmd.ExecuteScalarAsync();

                return Results.Ok(new { ok = true });
            }
            catch
            {
                // StatusCode(...) evita di esporre dettagli.
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        });

        // Diagnostica "ricca" SOLO in Development.
        // Qui mettiamo:
        // - OpenAPI endpoint
        // - /api/db/ping che mostra dettagli utili (host/db/user/source) SOLO in locale.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            // /api/db/ping (DEV-only)
            // - serve a distinguere problemi di credenziali/config (A2) da problemi di EF/mapping (07a)
            // - è volutamente più verboso, ma resta confinato al DEV.
            app.MapGet("/api/db/ping", async () =>
            {
                try
                {
                    await using var conn = new NpgsqlConnection(connectionString);
                    await conn.OpenAsync();

                    await using var cmd = new NpgsqlCommand("SELECT 1;", conn);
                    var result = await cmd.ExecuteScalarAsync();

                    // Prepariamo un connection string builder per leggere campi in modo robusto.
                    // Mascheriamo la password prima di restituire dati.
                    var csb = new NpgsqlConnectionStringBuilder(connectionString);
                    csb.Password = "****";

                    return Results.Ok(new
                    {
                        ok = true,
                        result,
                        connectionSource = source.ToString(),
                        envFileLoaded = envPath is not null,
                        database = csb.Database,
                        host = csb.Host,
                        port = csb.Port,
                        username = csb.Username
                    });
                }
                catch (Exception ex)
                {
                    // In DEV possiamo restituire il messaggio di errore per debug.
                    // In NON-DEV questo endpoint non esiste.
                    return Results.Problem(
                        title: "Database ping failed",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            });
        }

        // Endpoint usato da UseExceptionHandler("/error") in NON-DEV.
        // Deve restituire una risposta "safe" (niente dettagli, niente stack trace).
        app.Map("/error", () =>
            Results.Problem(
                title: "Unhandled server error",
                statusCode: StatusCodes.Status500InternalServerError))
           .ExcludeFromDescription();

        // In A2 è ammesso:
        // - non espone nulla se non hai Controllers reali
        // - quando li aggiungerai (Step 10), saranno mappati qui.
        app.MapControllers();

        // SPA fallback: se una route non è gestita dal backend, serviamo index.html
        // (utile per routing client-side in React).
        app.MapFallbackToFile("index.html");

        #endregion

        #region Run (avvio del server)

        // Avvio del loop HTTP: da qui l'app rimane in ascolto finché il processo non termina.
        app.Run();

        #endregion
    }

    #region Helper: risoluzione della connection string

    /// <summary>
    /// Determina la connection string del DB secondo la priorità A2:
    /// 1) Env var preferita: ConnectionStrings__JokesDb
    /// 2) Fallback config: ConnectionStrings:JokesDb (da appsettings, placeholder non sensibile)
    /// 3) Composizione da DB_* (HOST/PORT/NAME/USER/PASSWORD)
    ///
    /// Ritorna anche la "source" selezionata (utile per logging e debug).
    /// </summary>
    private static (string ConnectionString, DbConnectionSource Source) ResolveConnectionString(WebApplicationBuilder builder)
    {
        static string? EnvVar(string key) => Environment.GetEnvironmentVariable(key)?.Trim();

        static bool IsPlaceholder(string? cs) =>
            string.IsNullOrWhiteSpace(cs) ||
            cs.Contains("<DB_PASSWORD>", StringComparison.OrdinalIgnoreCase) ||
            cs.Contains("${DB_PASSWORD}", StringComparison.OrdinalIgnoreCase);

        var fromConfig = builder.Configuration.GetConnectionString("JokesDb")?.Trim();
        var fromEnvConnectionStrings = EnvVar("ConnectionStrings__JokesDb");

        // A) priorità massima
        if (!string.IsNullOrWhiteSpace(fromEnvConnectionStrings))
        {
            return (fromEnvConnectionStrings, DbConnectionSource.EnvConnectionString);
        }

        // B) fallback config SOLO se NON è placeholder
        if (!IsPlaceholder(fromConfig))
        {
            return (fromConfig!, DbConnectionSource.AppsettingsFallback);
        }

        // C) composizione da DB_*
        var host = EnvVar("DB_HOST");
        var port = EnvVar("DB_PORT");
        var name = EnvVar("DB_NAME");
        var user = EnvVar("DB_USER");
        var pass = EnvVar("DB_PASSWORD");

        var ok = !string.IsNullOrWhiteSpace(host)
                 && !string.IsNullOrWhiteSpace(port)
                 && !string.IsNullOrWhiteSpace(name)
                 && !string.IsNullOrWhiteSpace(user)
                 && !string.IsNullOrWhiteSpace(pass);

        if (!ok)
        {
            return (string.Empty, DbConnectionSource.Missing);
        }

        var composed = $"Host={host};Port={port};Database={name};Username={user};Password={pass};";
        return (composed, DbConnectionSource.ComposedFromDbVars);
    }

    /// <summary>
    /// Regola A2 di fail-fast:
    /// se manca la config DB o è un placeholder, l'app deve fermarsi in startup.
    ///
    /// Il messaggio dev'essere "safe" (niente segreti).
    /// </summary>
    private static void EnsureConnectionStringIsValid(string connectionString)
    {
        // Placeholder tipici che indicano "non configurato davvero".
        // Non vogliamo partire con una connection string fittizia.
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Contains("<DB_PASSWORD>", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("${DB_PASSWORD}", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Missing DB configuration. Provide ConnectionStrings__JokesDb (preferred) or DB_* variables. " +
                "appsettings.json must contain only a non-sensitive placeholder.");
        }
    }

    /// <summary>
    /// Logga i dettagli DB in modo sicuro:
    /// - host/port/database/username
    /// - source selezionata
    /// - indica solo se una password è impostata (true/false)
    /// - non stampa mai la password.
    /// </summary>
    private static void LogSafeDbConfiguration(
        ILogger logger,
        string connectionString,
        DbConnectionSource source,
        bool envFileLoaded)
    {
        // NpgsqlConnectionStringBuilder parsifica la connessione in campi.
        var csb = new NpgsqlConnectionStringBuilder(connectionString);

        // Segnale utile: password presente sì/no (ma non la stampiamo).
        var passwordWasSet = !string.IsNullOrWhiteSpace(csb.Password);

        // Blindiamo la password comunque, per evitare incidenti.
        csb.Password = "****";

        logger.LogInformation(
            "DB connection source selected: {Source}. EnvFileLoaded={EnvFileLoaded}. Host={Host}, Port={Port}, Database={Database}, Username={Username}, PasswordSet={PasswordSet}",
            source,
            envFileLoaded,
            csb.Host,
            csb.Port,
            csb.Database,
            csb.Username,
            passwordWasSet);
    }

    #endregion
}

/// <summary>
/// Origine della connection string del database.
/// Serve per tracciamento e debug (logging safe).
/// </summary>
public enum DbConnectionSource
{
    /// <summary>
    /// ConnectionStrings__JokesDb presente in ambiente (scelta preferita).
    /// </summary>
    EnvConnectionString,

    /// <summary>
    /// Composizione da variabili DB_*.
    /// </summary>
    ComposedFromDbVars,

    /// <summary>
    /// Fallback da appsettings (solo placeholder non sensibile).
    /// </summary>
    AppsettingsFallback,

    /// <summary>
    /// Nessuna configurazione sufficiente trovata.
    /// </summary>
    Missing
}
```

---
