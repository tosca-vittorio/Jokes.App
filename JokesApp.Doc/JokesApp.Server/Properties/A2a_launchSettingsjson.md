# 📚 JokesApp.Doc/JokesApp.Server/Properties/A2a_launchSettingsjson.md: Configurazione di `launchSettings.json` per gli ambienti DEV e PROD

*Documentazione tecnica di progetto — configurazione di `launchSettings.json` per la gestione degli ambienti di sviluppo e produzione.*

---

## 1. Scopo e contesto

Questo documento descrive la configurazione del file `launchSettings.json` per la gestione degli ambienti di avvio in **Development** e **Production** nell'applicazione JokesApp. Questo setup è essenziale per garantire che il progetto venga eseguito correttamente in ambiente locale durante lo sviluppo e in modalità di produzione durante la fase finale di deploy.

La configurazione contiene due profili principali:

* **`server-dev`**: ambiente di sviluppo, con configurazioni di debug e diagnostica arricchita.
* **`server-prod`**: ambiente di produzione, con configurazioni ottimizzate per l'uso in un ambiente live.

### 1.1 Principio di separazione degli ambienti

Il file `launchSettings.json` è progettato per separare i profili di avvio e le variabili d’ambiente specifiche per **Development** e **Production**. Questo approccio consente di gestire configurazioni differenziate senza rischiare di confondere le variabili di ambiente o di esporre credenziali sensibili in ambienti non sicuri.

---

## 2. Prerequisiti

Per configurare correttamente il file `launchSettings.json`, sono necessari i seguenti prerequisiti:

* Il progetto è configurato per utilizzare **ASP.NET Core** e supporta **Multiple Environments** (Development/Production).
* È configurata una connessione al database PostgreSQL, utilizzando variabili d’ambiente o un file `.env` (per l’ambiente di sviluppo).
* È necessario che il file `launchSettings.json` sia presente nella directory `Properties` del progetto.
* (Opzionale) Utilizzo di un proxy SPA in ambiente di sviluppo (`Microsoft.AspNetCore.SpaProxy`).

---

## 3. Parametri di riferimento (configurazione del profilo di avvio)

| Elemento                                          | Valore                                              |
| ------------------------------------------------- | --------------------------------------------------- |
| Profilo per ambiente di sviluppo                  | `server-dev`                                        |
| Profilo per ambiente di produzione                | `server-prod`                                       |
| URL di ascolto per sviluppo                       | `https://localhost:7215;http://localhost:5129`      |
| URL di ascolto per produzione                     | `http://localhost:5129`                             |
| Variabile `ASPNETCORE_ENVIRONMENT` per sviluppo   | `Development`                                       |
| Variabile `ASPNETCORE_ENVIRONMENT` per produzione | `Production`                                        |
| Variabile `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`   | `Microsoft.AspNetCore.SpaProxy` (solo per sviluppo) |

---

## 4. Configurazione di `launchSettings.json`

### 4.1 Struttura e profili definiti

Il file `launchSettings.json` contiene la configurazione per due ambienti di avvio distinti: **sviluppo** e **produzione**. Ogni ambiente ha un profilo separato con le proprie variabili d’ambiente e URL di ascolto.

### 4.2 Profilo `server-dev`

Il profilo `server-dev` è destinato all’ambiente di sviluppo. Quando il progetto è avviato con questo profilo, il backend sarà accessibile sia su **HTTP** che **HTTPS** (utilizzando le porte 5129 e 7215). Questo profilo abilita anche il proxy per le SPA, utile quando si sviluppa un’applicazione client in React, Angular, o Vue.js.

**Codice del profilo `server-dev`**:

```json
"server-dev": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": false,
  "applicationUrl": "https://localhost:7215;http://localhost:5129",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
  }
}
```

**Spiegazione**:

* `commandName`: indica che il progetto deve essere eseguito come applicazione.
* `dotnetRunMessages`: abilita i messaggi di output quando si esegue `dotnet run`.
* `launchBrowser`: disabilita l’apertura automatica del browser all’avvio.
* `applicationUrl`: definisce gli URL di ascolto per l’applicazione (HTTP e HTTPS).
* `environmentVariables`: imposta la variabile `ASPNETCORE_ENVIRONMENT` su `Development`, configurando l’ambiente per lo sviluppo. Inoltre, viene configurato il proxy per le SPA.

### 4.3 Profilo `server-prod`

Il profilo `server-prod` è destinato all’ambiente di produzione. Con questo profilo, il server ascolta solo su **HTTP** (porta 5129), in quanto in produzione generalmente si usano meccanismi esterni per la gestione di HTTPS (ad esempio, tramite un reverse proxy come Nginx o Apache).

**Codice del profilo `server-prod`**:

```json
"server-prod": {
  "commandName": "Project",
  "dotnetRunMessages": true,
  "launchBrowser": false,
  "applicationUrl": "http://localhost:5129",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Production"
  }
}
```

**Spiegazione**:

* `commandName`: come per lo sviluppo, il progetto è eseguito come applicazione.
* `dotnetRunMessages`: i messaggi di esecuzione vengono abilitati.
* `launchBrowser`: disabilita l’apertura automatica del browser.
* `applicationUrl`: definisce l’URL di ascolto per l’applicazione (solo HTTP).
* `environmentVariables`: imposta `ASPNETCORE_ENVIRONMENT` su `Production`, configurando l’ambiente per la produzione.

---

## 5. Configurazione e utilizzo in fase di sviluppo e produzione

### 5.1 Sviluppo

In ambiente di sviluppo, quando avvii l’applicazione con il profilo `server-dev`, la configurazione dei dettagli di errore, l’abilitazione di HTTPS e l’utilizzo del proxy per le SPA assicurano un’esperienza di sviluppo ottimizzata. Puoi avviare l’app con il comando:

```bash
dotnet run --project .\JokesApp.Server\JokesApp.Server.csproj --launch-profile "server-dev"
```

Questo comando farà sì che l’app sia disponibile su entrambi i protocolli HTTP e HTTPS, utile per testare il comportamento in vari scenari.

### 5.2 Produzione

Quando sei pronto per passare alla produzione, avvia l’app con il profilo `server-prod` utilizzando il comando:

```bash
dotnet run --project .\JokesApp.Server\JokesApp.Server.csproj --launch-profile "server-prod"
```

In questo caso, l’app sarà disponibile solo su HTTP, come si farebbe in un ambiente di produzione vero e proprio.

---

## 6. Conclusioni

La corretta configurazione di `launchSettings.json` consente di separare facilmente gli ambienti di sviluppo e produzione, gestendo le variabili d’ambiente e le URL di ascolto per ciascun contesto. Inoltre, l’approccio adottato consente una gestione centralizzata delle impostazioni di avvio senza duplicazioni, garantendo chiarezza e sicurezza.

### 6.1 Codice aggiornato per `launchSettings.json`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "server-dev": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7215;http://localhost:5129",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES": "Microsoft.AspNetCore.SpaProxy"
      }
    },
    "server-prod": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5129",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    }
  }
}
```

---