# 📘 Installazione e Verifica di Entity Framework Core per PostgreSQL

*(Documentazione tecnica ufficiale del progetto)*

## 1. Introduzione

Entity Framework Core (EF Core) è l’**Object Relational Mapper (ORM)** di Microsoft che consente di interagire con il database utilizzando **classi C#**, evitando la necessità di scrivere SQL esplicito per le operazioni più comuni.

Per utilizzare PostgreSQL, EF Core richiede un **provider specifico**, ovvero:

```
Npgsql.EntityFrameworkCore.PostgreSQL
```

Questo documento descrive in modo strutturato:

1. Come verificare la presenza di EF Core nel progetto.
2. Come installarlo correttamente.
3. Come evitare gli errori più comuni.
4. Le best practice da seguire.

---

## 2. Verifica della presenza di EF Core nel progetto

Per controllare se EF Core è già installato nel progetto backend:

1. Apri **Visual Studio**.
2. Vai su **Tools → NuGet Package Manager → Package Manager Console**.
3. Esegui il comando:

```powershell
Get-Package
```

Se nell’elenco compare un pacchetto come:

* `Microsoft.EntityFrameworkCore`
* `Npgsql.EntityFrameworkCore.PostgreSQL`

significa che EF Core è già configurato.

Se non compaiono, procedi con l’installazione.

---

## 3. Installazione di EF Core con provider PostgreSQL

### 3.1 Prerequisito fondamentale

EF Core deve essere installato **esclusivamente nel progetto backend** (es. `JokesApp.Server`), perché:

* solo il backend contiene il codice C# che gestisce DbContext e migrazioni;
* il frontend React non può utilizzare ORM o librerie server-side.

L’errore più comune deriva dall’aver selezionato per sbaglio **JokesApp.Client** come progetto di destinazione.

---

### 3.2 Installazione tramite Package Manager Console

Assicurati che il progetto selezionato sia **JokesApp.Server**:

```
Default project:  JokesApp.Server
```

Poi esegui:

```powershell
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
```

Il pacchetto installa automaticamente:

* EF Core,
* il provider PostgreSQL,
* le dipendenze necessarie.

Questa operazione aggiornerà anche il file `.csproj`.

---

## 4. Verifica post-installazione

Per confermare che il pacchetto sia stato installato correttamente:

```powershell
Get-Package
```

Dovresti ottenere una voce simile a:

```
Npgsql.EntityFrameworkCore.PostgreSQL  {versione}  JokesApp.Server
```

Se appare, EF Core è **pronto all’uso**: puoi creare il `DbContext`, definire i modelli e lavorare con migrazioni e CRUD.

---

## 5. Errori comuni e loro risoluzione

### Errore tipico:

```
Non è stato possibile installare il pacchetto ...
Si sta tentando di eseguire l'installazione in un progetto destinato a 'net6.0'...
```

### Significato:

Il pacchetto è stato installato **nel progetto sbagliato** (tipicamente il client).
EF Core non è compatibile con progetti React o front-end.

### Soluzione:

* Imposta come `Default project` **JokesApp.Server**.
* Ripeti il comando di installazione.

---

## 6. Best Practice e Considerazioni di Sicurezza

* **Installa EF Core solo nel backend.**
  Mai nel progetto client (React).
* **Mantieni la versione di EF Core compatibile con la versione di .NET del progetto** (es. .NET 8).
* **Aggiorna EF Core e il provider solo in ambienti controllati**, poiché influiscono direttamente sulla struttura del database.
* **Non installare pacchetti globali**: utilizza sempre NuGet per progetto.

---

## 7. Conclusione

Se il comando `Get-Package` restituisce:

```
Npgsql.EntityFrameworkCore.PostgreSQL {versione}  JokesApp.Server
```

allora:

* EF Core è configurato correttamente;
* il provider PostgreSQL è attivo;
* puoi procedere alla creazione dei modelli (`Joke.cs`, `ApplicationUser.cs`), del `DbContext` e delle migrazioni.

L’infrastruttura dati è ora pronta per lo sviluppo delle funzionalità applicative.

---


