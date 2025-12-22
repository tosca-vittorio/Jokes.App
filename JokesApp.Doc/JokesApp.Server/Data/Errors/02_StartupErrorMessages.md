# 📘 **02_StartupErrorMessages.md**

### *Messaggi di errore di avvio (Data Layer)*

## 2.1 Ruolo nel contesto dell’architettura

Nel modello architetturale adottato, il backend è organizzato in layer con responsabilità ben
distinte:

- **Domain Layer** → regole di business, invarianti, domain events, domain exceptions;
- **Application Layer** → orchestrazione dei casi d’uso;
- **Data/Infrastructure Layer** → persistenza, accesso al database, integrazioni tecniche, startup.

`StartupErrorMessages` appartiene chiaramente al **Data/Infrastructure Layer**: non descrive
errori di dominio o di business, ma **errori tecnici legati alla fase di avvio dell’applicazione**,
in particolare:

- la connessione al database,
- il test del `DbContext` (health check iniziale, migrazioni, inizializzazione dati, ecc.).

L’idea è avere un **contenitore centralizzato di messaggi di errore testuali** relativi allo startup
del sistema, così da:

- evitare stringhe “magiche” sparse nel codice,
- mantenere i messaggi coerenti, traducibili e modificabili in un solo punto,
- separare chiaramente la semantica tecnica di startup dalle eccezioni di dominio.

---

## 2.2 Definizione della classe

```csharp
namespace JokesApp.Server.Data.Errors
{
    /// <summary>
    /// Contiene i messaggi di errore utilizzati durante la fase di avvio (startup)
    /// dell'applicazione, principalmente legati alla connessione e al test del database.
    /// </summary>
    public static class StartupErrorMessages
    {
        #region Database

        /// <summary>
        /// Messaggio di errore generico quando l'applicazione non riesce a connettersi
        /// al database durante la fase di avvio.
        /// </summary>
        public const string DatabaseConnectionErrorOnStartup =
            "Impossibile connettersi al database all'avvio dell'applicazione.";

        /// <summary>
        /// Messaggio di errore generato quando il test del DbContext fallisce
        /// durante la fase di avvio dell'applicazione.
        /// </summary>
        public const string DbContextTestErrorOnStartup =
            "Errore durante il test del DbContext all'avvio dell'applicazione.";

        #endregion
    }
}
```

Gli elementi chiave di design sono:

* **namespace**: `JokesApp.Server.Data.Errors` → colloca chiaramente il tipo nel Data Layer;
* **classe `static`** → indica che si tratta di un puro contenitore di costanti, senza stato né istanze;
* **costanti `const string`** → messaggi immutabili, utilizzabili in qualunque punto del codice
  di startup (log, eccezioni tecniche, wrapping, ecc.);
* **XML doc in italiano** → documentazione integrata direttamente nel codice, utile sia a livello
  IDE che per eventuali generatori di documentazione.

---

## 2.3 Obiettivi progettuali di `StartupErrorMessages`

La scelta di introdurre una classe dedicata come `StartupErrorMessages` segue tre obiettivi principali:

1. **Centralizzazione dei messaggi tecnici di startup**

   Tutti i messaggi di errore relativi alla fase iniziale di avvio (bootstrap) dell’applicazione
   sono raccolti in un unico punto:

   * errori di connessione al database all’avvio,
   * errori durante il test del `DbContext`,
   * eventuali futuri errori di inizializzazione infrastrutturale.

   Questo rende il codice più leggibile e manutenibile: chi legge uno `throw` o un log
   sa esattamente dove trovare e modificare il messaggio.

2. **Separazione netta tra errori di dominio ed errori infrastrutturali**

   A differenza delle eccezioni di dominio, che vivono nel **Domain Layer** e rappresentano
   violazioni di regole di business, questi messaggi:

   * appartengono al **Data/Infrastructure Layer**,
   * descrivono problemi puramente tecnici (DB non raggiungibile, test del contesto fallito),
   * non espongono concetti di business.

   In questo modo, i due livelli di errore non si confondono: il dominio resta puro e indipendente
   dalla tecnologia, mentre l’infrastruttura espone i propri problemi attraverso messaggi tecnici
   ben definiti.

3. **Preparazione a internazionalizzazione / localizzazione futura**

   Pur usando attualmente messaggi in italiano, il fatto di centralizzarli in una classe dedicata
   consente in futuro di:

   * sostituirli con risorse localizzate (resource file, sistemi di i18n),
   * cambiare la lingua o il registro in un solo punto,
   * agganciarli a sistemi di configurazione più complessi.

---

## 2.4 Contenuto corrente: errori focalizzati sul database

Attualmente `StartupErrorMessages` contiene due messaggi principali, entrambi legati al database:

1. **`DatabaseConnectionErrorOnStartup`**

   ```csharp
   public const string DatabaseConnectionErrorOnStartup =
       "Impossibile connettersi al database all'avvio dell'applicazione.";
   ```

   Questo messaggio rappresenta un errore generico di **connessione al database**
   durante la fase di avvio. È tipicamente utilizzato in scenari quali:

   * fallimento nella creazione della connessione iniziale,
   * problemi di configurazione della connection string,
   * database non raggiungibile per motivi di rete o autenticazione.

   Può essere usato per loggare l’errore, per lanciare eccezioni tecniche o per fornire
   un messaggio chiaro nel caso in cui l’applicazione non riesca nemmeno a partire.

2. **`DbContextTestErrorOnStartup`**

   ```csharp
   public const string DbContextTestErrorOnStartup =
       "Errore durante il test del DbContext all'avvio dell'applicazione.";
   ```

   Questo messaggio è pensato per quelle fasi in cui, all’avvio, si esegue un:

   * test di reachability del database,
   * test di creazione del `DbContext`,
   * eventuale esecuzione automatica di migrazioni/seed.

   Se il test fallisce (eccezioni nel `DbContext`, migrazioni non applicabili, ecc.),
   questo messaggio viene utilizzato per loggare o segnalare il problema.

---

## 2.5 Esempi d’uso tipici nella fase di bootstrap

**1. Utilizzo in un blocco di startup (Program / host builder)**

```csharp
try
{
    // Esempio: test di connessione o applicazione migrazioni
    await DatabaseInitializer.EnsureDatabaseIsAvailableAsync(app.Services);
}
catch (Exception ex)
{
    logger.LogCritical(ex, StartupErrorMessages.DatabaseConnectionErrorOnStartup);
    throw;
}
```

In questo caso, `DatabaseConnectionErrorOnStartup` viene utilizzato come messaggio chiaro e centralizzato
da associare al log di errore critico che impedisce all’applicazione di avviarsi.

**2. Utilizzo in un componente dedicato di inizializzazione del DbContext**

```csharp
public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseIsAvailableAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Esempio: semplice test sul DbContext (migrate, can connect, ecc.)
            await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            // Messaggio specifico per il fallimento del test del DbContext
            throw new InvalidOperationException(StartupErrorMessages.DbContextTestErrorOnStartup, ex);
        }
    }
}
```

Qui `DbContextTestErrorOnStartup` viene usato per incapsulare e descrivere in modo coerente
il fallimento del test sul `DbContext` all’avvio.

---

## 2.6 Coerenza con l’architettura complessiva

L’esistenza di `StartupErrorMessages` rafforza alcuni principi architetturali adottati:

* **Separation of Concerns**

  * I messaggi di errore di startup, tecnici e infrastrutturali, sono confinati
    al **Data Layer**, separati dal dominio e dalla logica applicativa.
* **Clean Architecture**

  * Il Domain Layer resta privo di riferimenti a problemi tecnici di startup
    (connessione al DB, test del contesto, ecc.).
  * L’infrastruttura si occupa dei suoi problemi, e li comunica con messaggi
    propri, centralizzati e coerenti.
* **Manutenibilità**

  * Aggiungere nuovi messaggi di startup (es. per cache, message broker, servizi esterni)
    richiederà solo di estendere questa classe, mantenendo l’organizzazione per
    region/area (es. `#region Database`, `#region Cache`, ecc.).

---

## 2.7 Linee guida per l’estensione futura

Quando il sistema crescerà e introdurrà altre componenti infrastrutturali da inizializzare
in fase di avvio (es. cache distribuita, message broker, sistemi esterni), sarà naturale
estendere `StartupErrorMessages`:

* aggiungendo nuove costanti, organizzate per `#region` (es. `#region Cache`, `#region Messaging`),
* mantenendo la stessa filosofia:

  * messaggi **chiari**, in linguaggio naturale,
  * centralizzazione delle stringhe tecniche,
  * distinzione netta rispetto ai messaggi/exception del Domain Layer.

La regola di base resta:

> Tutto ciò che riguarda errori tecnici durante lo startup dell’applicazione appartiene al Data/Infrastructure Layer e trova in `StartupErrorMessages` il punto di riferimento testuale principale.

---