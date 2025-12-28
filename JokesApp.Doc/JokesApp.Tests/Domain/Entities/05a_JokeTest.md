# 📘 **03_JokeTest.md — Documentazione Tecnica dei Test del Modello Joke**

## *Verifica del dominio, delle invarianti e del comportamento evolutivo dell'entità Joke*

---

# 1️⃣ Introduzione generale

Il modello `Joke` rappresenta una delle entità fondamentali del dominio applicativo di **JokesApp**: una barzelletta associata a un utente registrato, dotata di invarianti, regole di validazione e comportamenti specifici (creazione, aggiornamento, assegnazione dell’autore).

I test dedicati al modello hanno un ruolo essenziale nella verifica:

* della **correttezza delle logiche di dominio**,
* della **coerenza delle validazioni interne**,
* dell'**integrità degli invarianti**,
* della **precisione dei messaggi di errore**,
* della **resistenza del modello in scenari limite**,
* della **stabilità del comportamento nel tempo**,
* dell’allineamento del modello con i documenti di riferimento:
  `03_Joke.md` e `03_JokeErrorMessages.md`.

Questa documentazione analizza in modo formale, descrittivo e approfondito l’insieme dei test contenuti nel file `JokeTests.cs`, illustrando finalità, principi architetturali, categorie di verifica e corrispondenze con il modello.

---

# 2️⃣ Obiettivo del testing

Gli unit test del modello Joke perseguono i seguenti obiettivi:

### 🎯 **1. Validare il costruttore di dominio**

Assicurare che:

* question, answer, userId vengano normalizzati (trim)
* il costruttore imponga le invarianti
* `CreatedAt` sia correttamente impostato
* `UpdatedAt` sia inizialmente null
* nessuna proprietà obbligatoria resti in stato inconsistente

### 🎯 **2. Verificare la logica di validazione interna**

Inclusa la funzione privata di dominio *ValidateQuestionAnswer*, che stabilisce:

* non-null
* non-empty
* non-whitespace
* rispetto dei limiti massimi
* uniformità dei messaggi di errore

### 🎯 **3. Garantire la corrispondenza con i messaggi di errore centralizzati**

Tutti i test negativi controllano:

* tipo di eccezione sollevata
* nome del parametro associato
* messaggio d’errore specifico definito in `JokeErrorMessages`

### 🎯 **4. Verificare il comportamento del metodo Update**

Inclusi:

* aggiornamento coerente delle proprietà
* preservazione dell’invariante CreatedAt
* impostazione di UpdatedAt
* trimming
* validazione identica a quella del costruttore
* ripetibilità nel tempo (test multipli)

### 🎯 **5. Testare la relazione con l’autore**

Con particolare attenzione a:

* coerenza tra ApplicationUserId e Author.Id
* impedire l’assegnazione multipla dell’autore
* gestione corretta degli errori dominio
* stabilità dell’autore dopo aggiornamenti

### 🎯 **6. Verificare le DataAnnotations**

Tramite riflessione, assicurando che:

* MaxLength sia coerente con le costanti statiche di dominio
* Required sia presente sulle proprietà critiche

### 🎯 **7. Analizzare edge case e scenari limite**

Come:

* Unicode
* grande numerosità di aggiornamenti
* thread safety
* confronto dei timestamp
* stringhe ai limiti del dominio

---

# 3️⃣ Architettura del test e strumenti utilizzati

Il progetto `JokesApp.Tests` utilizza una combinazione di strumenti moderni:

### ✔ **xUnit**

Framework di testing per .NET, adottato per la sua linearità e integrazione naturale con Visual Studio e CLI.

### ✔ **FluentAssertions**

Utilizzato intensivamente per:

* verifiche su stringhe e proprietà
* confronti temporali con tolleranze configurabili
* analisi esplicita delle eccezioni (tipo, parametro, messaggio)
* test più leggibili e semanticamente chiari

Esempio tipico:

```csharp
act.Should().Throw<ArgumentException>()
   .WithParameterName(nameof(question))
   .WithMessage($"*{JokeErrorMessages.QuestionNullOrEmpty}*");
```

### ✔ **Reflection & DataAnnotations.Validator**

Usati per documentare e verificare:

* MaxLength
* Required
* coerenza strutturale delle proprietà con EF Core

### ✔ **Parallel.For**

Per generare scenari concorrenziali e verificare stabilità dei metodi sotto carico minimo.

---

# 4️⃣ Evoluzione storica della suite di test

L’insieme dei test attuali è il risultato di tre fasi evolutive:

---

## 🟦 **Fase 1 — Test basilari**

Comprendeva:

* verifica delle proprietà iniziali
* creazione semplice dell’oggetto
* controllo dei valori assegnati

Questi test costituivano la base per validare la struttura iniziale del modello.

---

## 🟦 **Fase 2 — Introduzione degli invarianti e messaggi centralizzati**

A seguito della maturazione del dominio:

* sono stati introdotti i test sulle eccezioni
* sono stati verificati tutti i limiti stringa
* è stato incluso il controllo del `ParameterName`
* è stata inserita la validazione su Required e MaxLength

---

## 🟦 **Fase 3 — Test avanzati sul comportamento dinamico**

Con l’incremento delle funzionalità:

* test su `Update`
* test su autore (`SetAuthor`)
* test su Unicode
* test ThreadSafe
* test multipli di aggiornamento
* edge-case su timestamp

Questa fase ha portato la suite di test a un livello **enterprise**, coprendo completamente il ciclo di vita di una barzelletta.

---

# 5️⃣ Struttura e categorie dei test

La seguente sezione descrive le categorie fondamentali testate.
Ogni categoria corrisponde a un blocco logico del modello.

---

## 🔷 A. Test del Costruttore

### 1. Inizializzazione corretta

Verifica che:

* tutte le proprietà siano impostate come previsto
* CreatedAt sia in UTC
* UpdatedAt sia null
* nessun autore venga associato automaticamente

### 2. Supporto a CreatedAt personalizzato

Il modello accetta un valore custom per CreatedAt.
Questo permette:

* migrazioni di dati
* test deterministici
* ricostruzioni storiche

I test verificano che:

* il valore passato venga mantenuto
* UpdatedAt resti null
* Author resti null

### 3. Trimming automatico

Tutte le stringhe vengono normalizzate tramite:

```
value.Trim()
```

I test lo verificano sistematicamente.

### 4. Validazione parametri null, vuoti o whitespace

Con differenze semantiche:

* null → ArgumentNullException
* "" / "   " → ArgumentException

Per ogni parametro (question, answer, userId) sono previsti test dedicati.

---

## 🔷 B. Test sui limiti di lunghezza

I limiti sono:

* Question: 200 caratteri
* Answer: 500 caratteri

I test verificano:

* accettazione dei valori limite
* rifiuto dei valori fuori limite
* messaggi di errore corretti

---

## 🔷 C. Test sul metodo Update

### Comportamenti verificati:

* aggiornamento delle proprietà
* trimming
* validazione identica al costruttore
* mantenimento di CreatedAt
* impostazione di UpdatedAt
* coerenza temporale → `UpdatedAt > CreatedAt`

Test avanzati includono:

* Unicode
* aggiornamenti ripetuti
* test di concorrenza con Parallel.For
* tolleranza sui timestamp

---

## 🔷 D. Test sul metodo SetAuthor *(sezione aggiornata)*

### Invarianti testati:

* l’autore non può essere `null`;
* l’autore può essere impostato **una sola volta**: i test verificano l’errore `AuthorAlreadySet`;
* l’ID dell’autore deve coincidere con `ApplicationUserId`: in caso contrario viene sollevata l’eccezione `AuthorIdMismatch`;
* l’assegnazione dell’autore **non viene alterata** da successive chiamate a `Update`.

### Nota di coerenza con il dominio:

Il dominio richiede obbligatoriamente un `ApplicationUserId` valido già nel costruttore dell’entità `Joke`.
Per questo motivo:

* non sono presenti test che simulano `ApplicationUserId` vuoto o non inizializzato;
* tali scenari non rappresentano stati validi dell’entità e non possono essere prodotti tramite i costruttori pubblici;
* la suite di test include solo comportamenti che rispettano gli invarianti del modello.

---

## 🔷 E. Test su IsAuthoredBy

Verificano i seguenti casi:

* match esatto
* mismatch
* null / empty
* whitespace

Tutti devono comportarsi in modo deterministico.

---

## 🔷 F. Test delle DataAnnotations

Usando reflection, si verifica che:

* Question abbia MaxLength(200)
* Answer abbia MaxLength(500)
* ApplicationUserId abbia Required

---

## 🔷 G. Edge Case

Comprendono:

* CreatedAt immutabile
* Unicode in input
* concorrenza semplice
* coerenza dei timestamp dopo multiple update

---

# 6️⃣ Diagramma logico del comportamento testato

### Flusso di validazione del costruttore

```
          ┌────────────────────────────────┐
          │  new Joke(question, answer, id)│
          └────────────────────────────────┘
                        │
                        ▼
         ValidateQuestionAnswer(question, answer)
                        │
                        ├──────── invalid → throw ArgumentException/NullException
                        ▼
                ValidateUserId(userId)
                        │
                        └──────── invalid → throw ArgumentException/NullException
                        ▼
            Normalize strings (Trim)
                        │
                        ▼
            Set CreatedAt = UtcNow (unless custom)
                        │
                        ▼
                  Author = null
                        │
                        ▼
                 UpdatedAt = null
```

---

# 7️⃣ Best practice adottate

La suite di test aderisce pienamente alle best practice di testing:

* test altamente granulari
* nessuna dipendenza da database
* uso di FluentAssertions per espressività
* isolamento completo degli scenari
* naming chiaro e semantico
* AAA pattern rigoroso
* test negativi completi (tipo + parametro + messaggio)

---

# 8️⃣ Conclusioni

La suite `JokeTests.cs`:

* **copre in modo completo tutto il comportamento del dominio**
* verifica tutte le invarianti, tutti i vincoli e tutte le regole interne
* garantisce l’allineamento preciso con i documenti di dominio
* previene regressioni e anomalie future
* documenta formalmente l’architettura comportamentale del modello

Si tratta di una suite test robusta, corretta, completa e allineata ai criteri professionali enterprise.

---