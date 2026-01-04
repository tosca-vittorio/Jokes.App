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
