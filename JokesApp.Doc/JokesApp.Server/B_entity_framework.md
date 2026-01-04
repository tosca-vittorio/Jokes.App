# 📘📚 JokesApp.Doc/JokesApp.Server/B_entity_framework.md — Preflight Entity Framework Core per PostgreSQL

**Documentazione tecnica dello step B del server JokesApp: preflight di Entity Framework Core per PostgreSQL senza creazione di DbContext o migrazioni.**

## Introduzione

La fase **B** si concentra sull'integrazione di **Entity Framework Core** per PostgreSQL, che permetterà al progetto di interagire facilmente con il database tramite un ORM (Object-Relational Mapper). Questo passo è cruciale per semplificare la gestione dei dati e favorire un’evoluzione agile della struttura del database. EF Core, tramite il provider **Npgsql.EntityFrameworkCore.PostgreSQL**, permetterà di eseguire query, aggiornamenti e migrazioni senza dover scrivere manualmente codice SQL.

Dopo aver completato le fasi A1 e A2, che hanno configurato il sistema e la connessione al database, la fase B prepara l’ambiente per l’adozione di EF Core senza però implementare ancora i modelli o le migrazioni. Si tratta di un passo preliminare che crea le basi per le future evoluzioni.

> **Chiarimento (scope B):** in questa fase si installano e si verificano esclusivamente tool e pacchetti. Non viene ancora registrato EF Core nel container DI, non viene creato alcun `DbContext` e non viene eseguita alcuna migrazione.

---

## 1. **Obiettivo della fase B (EF Preflight)**

La milestone **B** segue il completamento del bootstrap (A1–A2) e prepara il backend all’adozione di Entity Framework Core (EF Core) senza ancora introdurre modelli o migrazioni. Lo scopo è:

- Verificare che l’ambiente di sviluppo disponga degli strumenti necessari (`dotnet ef`).
- Installare il provider `Npgsql.EntityFrameworkCore.PostgreSQL` nel progetto `JokesApp.Server`.
- (Facoltativo) Aggiungere il pacchetto design-time `Microsoft.EntityFrameworkCore.Design` per semplificare future migrazioni.

**Nota:** in questa fase non si genera il `DbContext`, né si creano migrazioni. Si lavora solo sull’infrastruttura e sulle dipendenze, lasciando la definizione del modello dati alla milestone 07a.

---

## 2. 🔍 **Prerequisiti e verifica iniziale**

Prima di installare qualsiasi pacchetto, assicurati di:

- **Aprire il progetto corretto:** Tutte le operazioni devono essere eseguite sul progetto `JokesApp.Server`, che contiene il backend ASP.NET Core. Il frontend React (`JokesApp.Client`) non deve mai ricevere pacchetti EF Core.
- **Verificare la presenza di `dotnet ef`:** Dal terminale o dalla Package Manager Console (PMC), esegui:
  
  ```powershell
  dotnet ef --version
  ```

Se il comando restituisce un numero di versione, l’utility è installata. In caso contrario, installala globalmente con:

```powershell
dotnet tool install --global dotnet-ef
```

**Suggerimento:** dopo l’installazione, potrebbe essere necessario aprire una nuova sessione del terminale perché il comando `dotnet ef` venga riconosciuto.

### 2.1 **Perché usare Entity Framework Core con PostgreSQL?**

Nel contesto del progetto, si è scelto PostgreSQL come database per la sua stabilità, scalabilità e supporto completo per transazioni e funzioni avanzate. Entity Framework Core verrà adottato come ORM (Object-Relational Mapper) per semplificare le operazioni di accesso ai dati, riducendo la complessità di interazione con il database e permettendo una gestione automatica delle migrazioni.

**Provider Npgsql**: PostgreSQL è supportato in .NET tramite il provider `Npgsql`. Questo pacchetto NuGet permette di utilizzare EF Core con PostgreSQL in modo efficiente, consentendo di eseguire operazioni come inserti, aggiornamenti, e query senza scrivere SQL esplicito.

### 2.2 Terminale vs Package Manager Console (PMC)

In questa documentazione compaiono due famiglie di comandi:

- **.NET CLI (terminale / PowerShell / Git Bash):** comandi `dotnet ...` (es. `dotnet ef --version`).
- **NuGet PMC (Visual Studio → Package Manager Console):** comandi `Install-Package`, `Get-Package`, `Uninstall-Package`.

Per evitare errori, esegui:
- `dotnet ...` in un terminale (consigliato) oppure in PMC se supportato;
- `Install-Package` / `Get-Package` solo in **PMC**.

---

## 3. ⚙ **Installazione del provider PostgreSQL**

### 3.1 Pacchetti minimi richiesti (e nota sulle versioni)

La verifica corretta (“truth-first”) avviene tramite `Get-Package` (PMC) oppure controllando il `.csproj`.

**Minimo necessario per EF Core + PostgreSQL (server):**
- `Npgsql.EntityFrameworkCore.PostgreSQL` (provider EF Core per PostgreSQL)
- `Microsoft.EntityFrameworkCore` (runtime EF Core)

**Tooling consigliato (sviluppo / comandi EF):**
- `Microsoft.EntityFrameworkCore.Tools` (integrazione tooling, utile in Visual Studio/PMC)
- `Microsoft.EntityFrameworkCore.Design` (design-time: richiesto/utile per migrazioni e scaffolding in step successivi)

> **Nota versioni:** in questo repository, .NET ed EF Core sono allineati a **major 10** (es. pacchetti `10.0.0`). Mantieni provider e pacchetti EF allineati alla stessa major per evitare incompatibilità.

### 3.2 **Comando di installazione**

Esegui il seguente comando nella PMC per installare EF Core con il provider PostgreSQL:

```powershell
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
```

Questo comando aggiunge automaticamente le dipendenze di EF Core al progetto e aggiorna il file `.csproj` di `JokesApp.Server`. Non è necessario installare separatamente `Microsoft.EntityFrameworkCore`.

### 3.3 **Pacchetto design-time (facoltativo)**

Per gestire in futuro la generazione di migrazioni da linea di comando o PMC, installa anche il pacchetto design-time:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Design
```

Sebbene non obbligatorio per il preflight, facilita la generazione di migrazioni e la gestione di scaffolding in step successivi.

---

## 4. **Verifica post-installazione**

Dopo l’installazione, verifica che i pacchetti siano presenti nel progetto backend. Esegui:

```powershell
Get-Package
```

Nel risultato dovresti vedere voci simili a:

```
Npgsql.EntityFrameworkCore.PostgreSQL {versione} JokesApp.Server
Microsoft.EntityFrameworkCore.Design {versione} JokesApp.Server (se installato)
```

Se compaiono, **EF Core** e il provider PostgreSQL sono configurati correttamente e pronti per essere utilizzati in 07a.

### 4.1 **Interpretazione dei Log di Configurazione**

Dopo aver risolto la stringa di connessione, il logger fornirà dettagli sulla configurazione del database senza rivelare informazioni sensibili. Un log di esempio potrebbe apparire come segue:

```bash
info: DB connection source selected: EnvConnectionString. EnvFileLoaded=True. Host=localhost, Port=5432, Database=jokes, Username=jokes_migrator, PasswordSet=True
```

### 4.2 Esempio reale (repo attuale)

Esempio (ottenuto in PMC):

```text
PM> dotnet ef --version
Entity Framework Core .NET Command-line Tools
10.0.0
```

Esempio (estratto da `Get-Package` in `JokesApp.Server`):
```text
Microsoft.EntityFrameworkCore             {10.0.0}  JokesApp.Server
Microsoft.EntityFrameworkCore.Tools       {10.0.0}  JokesApp.Server
Microsoft.EntityFrameworkCore.Design      {10.0.0}  JokesApp.Server
Npgsql.EntityFrameworkCore.PostgreSQL     {10.0.0}  JokesApp.Server
```

---

## 5. 🛠 **Problemi comuni e soluzioni**

### **Pacchetto installato nel progetto sbagliato**

**Errore:**
Non è stato possibile installare il pacchetto … Si sta tentando di eseguire l'installazione in un progetto destinato a 'netX.Y'...

> Nota: Nel repository attuale il target è .NET 10, ma l’errore può riportare qualsiasi TFM a seconda del progetto selezionato.

**Causa:**
Il pacchetto è stato installato su `JokesApp.Client` o un progetto non compatibile.

**Risoluzione:**
Assicurati che **Default project** sia `JokesApp.Server` prima di eseguire `Install-Package`. Se l’errore persiste, disinstalla i pacchetti dal progetto errato con `Uninstall-Package` e reinstalla su `JokesApp.Server`.

### **Comando `dotnet ef` non riconosciuto**

**Errore:**
'dotnet-ef' is not recognized as an internal or external command

**Soluzione:**
Installa l’utility con:

```powershell
dotnet tool install --global dotnet-ef
```

Chiudi e riapri la shell o Visual Studio per ricaricare le variabili d’ambiente.

### 5.1 Step successivo (07a): migrazioni e DbContext (fuori scope B)

La fase B **non** include:
- creazione del `DbContext`
- registrazione di EF Core nel DI container
- generazione/esecuzione migrazioni

Queste attività saranno affrontate **solo** nello step **07a**, quando il modello sarà pronto e coerente con il dominio.  
In B ci limitiamo a garantire che **tooling e pacchetti** siano presenti e verificabili.

---

## 6. **Best practice e raccomandazioni**

* **Isola EF Core nel backend:** Non installare mai EF Core o il provider in progetti front-end (React). Il client non ha necessità di conoscere il database.
* **Versione coerente:** Mantieni allineate le versioni di EF Core al framework .NET (es. **.NET 10** nel repository attuale) per evitare incompatibilità.
* **Aggiornamenti controllati:** Aggiorna EF Core e i provider in ambienti separati (es. branch dedicati) e testa sempre prima di rilasciare.
* **Pacchetti non globali:** utilizza NuGet a livello di progetto anziché installazioni globali (eccetto l’utility `dotnet ef`).

### 6.1 **Integrazione dei Controller nella Fase Successiva**

In questa fase, non sono presenti controller "di prodotto", ma in futuro, nel passo 10 della timeline, si implementeranno i controller reali per gestire le operazioni di business. I controller verranno mappati nel punto in cui abbiamo registrato il servizio `AddControllers()`.

Nel prossimo passo, il progetto si espanderà per includere i controller di dominio, e il sistema inizierà a rispondere a richieste più complesse legate alla logica applicativa.

---

## 7. **Conclusione** e **Riepilogo della Fase B (EF Preflight)**

Il preflight di Entity Framework Core in questa fase consiste esclusivamente nell’installazione e nella verifica degli strumenti necessari. Una volta che i pacchetti `Npgsql.EntityFrameworkCore.PostgreSQL` (e opzionalmente `Microsoft.EntityFrameworkCore.Design`) sono presenti nel progetto `JokesApp.Server`, il backend è pronto per definire il `DbContext`, aggiungere i modelli e creare le migrazioni nella milestone 07a. Fino a quel momento non introdurre classi di dominio, mapping o migrazioni: l’obiettivo di B è garantire che l’infrastruttura sia pronta senza modificare la struttura del database.

In questa fase, abbiamo:

- Installato e configurato **Entity Framework Core** con il provider **Npgsql** per PostgreSQL.
- Verificato che l'ambiente di sviluppo fosse correttamente configurato con gli strumenti necessari (`dotnet ef`).
- Preparato l'infrastruttura per l'adozione di EF Core senza ancora implementare il `DbContext` o migrazioni, rimandando la definizione dei modelli e la creazione delle migrazioni alla fase successiva.

**Prossimi passi:**

Nel prossimo step (07a), implementeremo il `DbContext`, aggiungeremo i modelli e inizieremo a creare le migrazioni per il database.

---