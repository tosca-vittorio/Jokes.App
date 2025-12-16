# 📘 **01_DomainException.md** 

### *Radice delle eccezioni di dominio*

---

## 1.1 Contesto: perché esiste una eccezione di dominio dedicata

Nel modello architetturale adottato (Clean Architecture + DDD), il **Domain Layer** è il luogo in cui
vivono le regole di business, gli invarianti e la logica che definisce “che cosa è corretto” o “non
corretto” nel contesto applicativo.

In questo scenario, gli errori che avvengono nel dominio:

- non devono essere confusi con errori infrastrutturali (DB, rete, I/O, ecc.);
- non devono conoscere dettagli di presentazione (HTTP, status code, formati JSON, ecc.);
- devono poter essere espressi in modo **semantico**, leggibile e coerente con il linguaggio
  ubiquo del dominio.

Per questo motivo è stata introdotta una **gerarchia di eccezioni specifiche di dominio**, la cui radice
comune è `DomainException`.

`DomainException` è quindi il **tipo base astratto** da cui derivano tutte le eccezioni che
rappresentano violazioni delle regole di business o degli invarianti del Domain Layer.

---

## 1.2 Definizione della classe

```csharp
using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione base astratta per tutti gli errori del dominio.
    /// Utilizzata per rappresentare violazioni delle regole di business
    /// e degli invarianti definiti nel Domain Layer.
    /// </summary>
    public abstract class DomainException : Exception
    {
        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/>.
        /// </summary>
        protected DomainException()
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio di errore descrittivo.</param>
        protected DomainException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainException"/> con un messaggio descrittivo
        /// e un'eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio di errore descrittivo.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        protected DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Inner exception is preserved to keep the original stack trace.
        }

        #endregion
    }
}
```

---

## 1.3 Obiettivi progettuali di `DomainException`

La progettazione di `DomainException` risponde a obiettivi precisi:

1. **Separare nettamente gli errori di dominio dagli altri errori**

   * Tutte le eccezioni che indicano una violazione delle regole di business o degli invarianti
     devono derivare da un tipo comune e riconoscibile (`DomainException`).
   * Questo permette ai layer esterni (Application / API) di intercettare con un semplice
     `catch (DomainException ex)` tutti gli errori “funzionali”.

2. **Mantenere il dominio indipendente dalla tecnologia**

   * `DomainException` eredita direttamente da `System.Exception` e non introduce alcun riferimento
     a HTTP, database, framework web o librerie di infrastruttura.
   * La traduzione verso dettagli tecnici (es. HTTP 400/403/409) viene demandata ad altri layer.

3. **Offrire una base standard per la gerarchia di eccezioni**

   * I costruttori seguono lo schema tipico delle eccezioni .NET:

     * costruttore vuoto,
     * costruttore con `message`,
     * costruttore con `message` + `innerException`.
   * Questo rende naturale, nei layer esterni (Application/Infrastructure), mantenere un’eventuale `innerException` quando si traduce un errore tecnico in un errore con semantica di dominio, preservando la stack trace

4. **Esplicitare il ruolo semantico nel dominio**

   * Il commento XML in italiano chiarisce che si tratta della **eccezione base astratta**
     per tutti gli errori del dominio e che rappresenta violazioni di regole di business
     e invarianti.

---

## 1.4 Scelte di design e motivazioni

**Classe astratta**

* `DomainException` è dichiarata `abstract` perché non ha senso lanciare una eccezione generica
  “di dominio” senza un significato più specifico.
* Questo obbliga a definire **sottoclassi semantiche**, ad esempio:

  * `DomainValidationException` per errori di validazione e invarianti,
  * `DomainOperationException` per operazioni non valide rispetto allo stato corrente,
  * eventuali eccezioni più specifiche in futuro.

**Costruttori `protected`**

* I costruttori sono `protected` perché `DomainException` non va istanziata direttamente.
* Solo le classi derivate (le vere eccezioni di dominio) possono decidere quale costruttore
  esporre come `public`.
* In questo modo si guida l’uso corretto del tipo e si evita di avere `throw new DomainException(...)`
  sparsi nel codice, che sarebbero poco informativi.

**Nessuna proprietà custom aggiuntiva**

* In questa fase non sono state aggiunte proprietà come “ErrorCode”, “Severity” o simili.
* Questo è intenzionale (YAGNI): il dominio resta concentrato sulle regole di business e sulle
  invarianti, mentre eventuali codifiche o livelli di severità possono essere introdotti in modo
  mirato se e quando serviranno davvero.
* Il punto di estensione principale non è la `DomainException` stessa, ma le **sottoclassi**
  che la specializzano.

---

## 1.5 Coerenza con DDD, Clean Architecture e SOLID

`DomainException` è coerente con i principi che guidano l’intera soluzione:

* **DDD (Domain-Driven Design)**

  * Gli errori legati al dominio sono modellati come primi cittadini, con un tipo
    dedicato e riconoscibile.
  * Il linguaggio ubiquo è rispettato: le eccezioni comunicheranno concettualmente
    cosa è andato storto dal punto di vista del business.

* **Clean Architecture**

  * Il Domain Layer non dipende da niente e nessuno. Anche il modo in cui viene
    segnalato un errore (un’eccezione) è indipendente da protocolli e tecnologie.
  * I layer esterni sono responsabili di “tradurre” `DomainException` in risposte HTTP,
    log tecnici, notifiche, ecc.

* **SOLID (in particolare SRP)**

  * `DomainException` ha una responsabilità unica: essere la radice comune delle
    eccezioni di dominio.
  * Non valida, non logga, non decide cosa fare a livello di interfaccia: il suo unico
    ruolo è quello di rappresentare un errore semantico di dominio.

---

## 1.6 Modalità d’uso nei layer superiori

Una volta che tutte le eccezioni di dominio deriveranno da `DomainException`, i layer superiori
potranno gestirle in modo uniforme.

Esempio tipico a livello Application/API:

```csharp
try
{
    // Invocazione di un caso d’uso / handler / servizio applicativo che coinvolge il dominio.
    // Il dominio può lanciare DomainException o una sua derivata (es. DomainValidationException).
    jokeService.LikeJoke(jokeId, currentUserId);
}
catch (DomainException domainEx)
{
    // Mapping coerente dell’errore di dominio verso la risposta applicativa (HTTP/UI/log, fuori dal dominio).
    // (es: 400, 403, 409, a seconda del tipo concreto di eccezione).
}
```

In questo modo:

* il Domain Layer rimane pulito e focalizzato solo sulle regole di business;
* Application / API decide **come** esporre al mondo esterno gli errori di dominio,
  senza che il dominio conosca i dettagli di tale esposizione.

---

## 1.7 Linee guida per l’estensione

Quando si definiscono nuove eccezioni di dominio, la regola generale è:

1. **Derivare sempre da `DomainException`** (direttamente o tramite altre sottoclassi di dominio).
2. **Dare un nome che esprima chiaramente il contesto**: usare una nomenclatura coerente che richiami l’area/aggregate coinvolto e il tipo di violazione.
   – es: `JokeCreationDomainException`, `UserProfileDomainException`, ecc.
3. **Mantenere la logica di dominio nel dominio**
   – l’eccezione non deve contenere logica tecnica, ma solo dati essenziali per descrivere
   la violazione (messaggio, eventuali nomi di membri, ecc.).
4. **Evitare di propagare concetti di presentazione o infrastruttura**
   – niente status code HTTP, niente classi di response, niente tipi di database.

---

## 1.8 Ruolo di `DomainException` nella chiusura del Domain Layer

Dal punto di vista della roadmap del progetto, `DomainException` rappresenta il **primo tassello**
per la chiusura del Domain Layer:

* fornisce la radice comune che verrà utilizzata da:

  * Value Objects (es. `QuestionText`, `AnswerText`, `JokeId`, `UserId`) per segnalare violazioni
    di invarianti tramite eccezioni di validazione;
  * entità e aggregate per segnalare operazioni non valide rispetto allo stato corrente;
  * eventuali logiche di autorizzazione di dominio (es. “questo utente non può modificare questa joke”).

Una volta stabilizzata `DomainException`, tutte le altre eccezioni specifiche possono essere
progettate in modo coerente e allineato, assicurando che l’intero ecosistema del dominio
gestisca gli errori in maniera uniforme, leggibile e aderente ai principi architetturali scelti.
