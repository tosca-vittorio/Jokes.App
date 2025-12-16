# 📘 **01_DomainOperationException.md**

### *Operazioni di dominio non valide*

---

## 1.9 Ruolo nel contesto del Domain Layer

All’interno della gerarchia di eccezioni di dominio, `DomainOperationException` rappresenta
il tipo dedicato a segnalare **operazioni di dominio non valide** rispetto:

- allo **stato corrente dell’aggregato** (o dell’entità),
- alle **regole di business** che governano il ciclo di vita di quell’oggetto.

Se `DomainException` è la radice astratta per tutti gli errori di dominio, `DomainOperationException`
è la sua specializzazione pensata per catturare tutti i casi in cui:

> “L’operazione richiesta *potrebbe essere sintatticamente corretta*, ma **non è coerente** con
> lo stato del modello o con le regole del dominio.”

Esempi tipici:

- un utente tenta di mettere “like” a una joke che ha già messo “like” in passato;
- si prova a modificare o cancellare una joke che, secondo le regole di business, è ormai
  considerata “bloccata” o “archiviata”;
- si richiede un’azione che viola una pre-condizione di stato (es. eseguire un’operazione
  che presuppone un determinato flag o transizione già avvenuta).

In termini DDD, si tratta sempre di operazioni **semantiche** del dominio che non possono
essere portate a termine perché infrangono la logica del modello, pur non essendo errori
di validazione superficiale (per quelli esiste `DomainValidationException`) né puri errori
di autorizzazione (gestibili con `UnauthorizedDomainOperationException`).   

---

## 1.10 Definizione della classe

```csharp
using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione che rappresenta un'operazione di dominio non valida
    /// rispetto allo stato corrente dell'aggregato o alle regole di business.
    /// </summary>
    public class DomainOperationException : DomainException
    {
        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainOperationException"/>.
        /// </summary>
        public DomainOperationException()
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainOperationException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di operazione di dominio.</param>
        public DomainOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainOperationException"/> con un messaggio descrittivo
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di operazione di dominio.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public DomainOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Preserve original exception details and stack trace.
        }

        #endregion
    }
}
```

Questa implementazione è perfettamente allineata alla gerarchia già definita per le eccezioni di dominio (`DomainException` come radice astratta). 

---

## 1.11 Obiettivi progettuali di `DomainOperationException`

La progettazione di `DomainOperationException` ha diversi obiettivi specifici:

1. **Esplicitare semanticamente gli errori di “operazione non valida”**

   * Distingue in modo chiaro:

     * errori di **validazione strutturale / invarianti** (→ `DomainValidationException`),
     * errori di **autorizzazione** (→ `UnauthorizedDomainOperationException`),
     * errori di **operazione non valida rispetto allo stato** (→ `DomainOperationException`).
   * Questo rende il codice molto più leggibile: chi legge un `throw new DomainOperationException(...)`
     capisce immediatamente che la richiesta “non è consentita in questo stato”.

2. **Permettere una gestione omogenea dei fallimenti di business**

   * I layer superiori (Application / API) possono intercettare tutti i casi di operazioni di dominio
     non valide con un semplice:

     ```csharp
     catch (DomainOperationException ex)
     {
         // Gestione generica di tutte le operazioni di dominio non valide
     }
     ```

   * Eventuali sottotipi più specifici (es. `UnauthorizedDomainOperationException`) restano comunque
     intercettabili come `DomainOperationException`, mantenendo un modello coerente di gestione errori.

3. **Offrire i costruttori standard (vuoto, message, message + innerException)**

   * Il costruttore vuoto è utile per compatibilità e per scenari in cui il messaggio viene impostato
     altrove (anche se l’uso principale resta quello con `message`).
   * Il costruttore con `innerException` consente di preservare dettagli diagnostici quando l’eccezione viene creata nei layer applicativi/adapters come traduzione di un fallimento tecnico. Il Domain Layer non effettua wrapping di eccezioni infrastrutturali.

4. **Rimanere coerente con la filosofia di Clean Architecture**

   * La classe non conosce nulla di HTTP, database, EF Core, né di concetti di presentazione.
   * Viene usata esclusivamente nel Domain Layer; la mappatura in termini di status code (400 / 409, ecc.)
     avviene più esternamente, in Application/API, come definito dall’architettura complessiva.

---

## 1.12 Differenza rispetto ad altre Domain Exceptions

Per evitare ambiguità, è utile chiarire la **responsabilità specifica** di `DomainOperationException`
rispetto alle altre eccezioni di dominio:

* **`DomainValidationException`**

  * Segnala violazioni di invarianti e regole di validazione *intrinseche* ai dati
    (es. stringa vuota, lunghezza eccessiva, formato non valido).
  * Tipicamente usata nei Value Objects e nelle factory delle entità.
  * Risponde alla domanda: *“Questo oggetto è valido di per sé?”*

* **`DomainOperationException`**

  * Segnala che l’operazione richiesta non è coerente con lo **stato corrente** o con il **ciclo di vita**
    dell’aggregato.
  * Risponde alla domanda: *“Posso fare QUESTA cosa ADESSO su questo oggetto?”*

* **`UnauthorizedDomainOperationException`**

  * È un caso particolare di `DomainOperationException` in cui la causa dell’impossibilità di eseguire
    l’operazione è legata al **soggetto che la richiede** (identità/ruolo/permessi), non allo stato puro
    dell’oggetto.
  * Risponde alla domanda: *“Questo utente è autorizzato a fare questa operazione?”*

Questa distinzione rende il dominio:

* più espressivo,
* più facile da testare (si possono verificare scenari specifici per ogni tipo di errore),
* più semplice da mappare in logica applicativa e in risposte HTTP diverse (400, 403, 409, ecc.).

---

## 1.13 Esempi pratici d’uso nel dominio

**1. Operazione non valida per stato (es. “like” duplicato)**

```csharp
public void Like(UserId userId)
{
    if (HasAlreadyBeenLikedBy(userId))
    {
        // Business rule: a user cannot like the same joke twice.
        throw new DomainOperationException("The joke has already been liked by this user.");
    }

    _likes.Add(userId);
}
```

Qui non si tratta di un problema di formato (la `UserId` è valida) né di autorizzazione (l’utente
è legittimamente autenticato), ma di una **regola di business** legata allo stato dell’aggregato:
non si può ripetere l’operazione in quella condizione.

---

## 1.14 Coerenza con SOLID, DDD e le linee guida del progetto

`DomainOperationException` rispetta i principi stabiliti per l’intero backend:

* **SRP (Single Responsibility Principle)**

  * Ha una responsabilità chiara e unica: rappresentare un’operazione di dominio non valida.
  * Non contiene logica aggiuntiva, non fa logging, non decide come l’errore sarà comunicato all’esterno.

* **DDD / Ubiquitous Language**

  * Il nome e la posizione nel namespace (`JokesApp.Server.Domain.Exceptions`) riflettono il ruolo nel
    linguaggio del dominio.
  * È naturale leggere nel codice che un’operazione è fallita a causa di una `DomainOperationException`.

* **Clean Architecture / Hexagonal**

  * È confinata nel Domain Layer, che resta indipendente da dettagli infrastrutturali e tecnologici.
  * I layer esterni traducono questa eccezione in comportamenti concreti (HTTP, log, metrics, ecc.).

---

## 1.15 Linee guida per l’estensione futura

Nel caso in cui il dominio cresca e richieda una modellazione ancora più fine degli errori di
operazione, `DomainOperationException` fornisce una base chiara da cui estendere:

* È **non sealed**, quindi è possibile creare sottoclassi come:

  * `UnauthorizedDomainOperationException` (già prevista),
  * `JokeLifecycleOperationException`,
  * `UserAccountOperationException`, ecc. (solo se realmente necessari).
* Queste sottoclassi mantengono la possibilità di essere intercettate unitariamente come
  `DomainOperationException`, preservando la coerenza del modello di errore.

La regola pratica resta in linea con le tue linee guida generali:

> Non introdurre nuove eccezioni “tanto per”, ma solo quando aggiungono **valore reale** in termini di
> espressività del dominio, chiarezza del codice e manutenibilità nel lungo periodo. 

---
