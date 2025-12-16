# 📘 **01_UnauthorizedDomainOperationException.md** 

### *Operazioni di dominio non autorizzate*

---

## 1.16 Ruolo nel contesto del Domain Layer

Nel modello a eccezioni di dominio, `UnauthorizedDomainOperationException` rappresenta
un caso **speciale** di operazione di dominio non valida: quella in cui la violazione
non è dovuta allo stato dell’aggregato in sé, ma ai **permessi** del soggetto che
tenta l’operazione.

A differenza di altri errori di business:

- i dati possono essere formalmente corretti,
- lo stato dell’aggregato può essere compatibile con l’operazione,
- ma l’utente (o il contesto di sicurezza corrente) **non è autorizzato** a eseguirla.

Esempi tipici nel contesto di JokesApp:

- un utente tenta di modificare o cancellare una joke che non gli appartiene;
- un utente prova ad accedere a risorse o funzionalità riservate a un certo ruolo;
- si prova a eseguire un’azione che le regole di dominio associano esplicitamente
  al “proprietario” dell’entità o a un sottoinsieme di utenti.

Per esprimere in modo pulito questo scenario, `UnauthorizedDomainOperationException`
è modellata come una **specializzazione di `DomainOperationException`**: è a tutti gli effetti
un’operazione di dominio non valida, ma con causa specifica “authorization/permissions”.

---

## 1.17 Definizione della classe

```csharp
using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione generata quando un utente tenta una operazione di dominio
    /// senza possedere i permessi necessari.
    /// </summary>
    public class UnauthorizedDomainOperationException : DomainOperationException
    {
        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="UnauthorizedDomainOperationException"/>.
        /// </summary>
        public UnauthorizedDomainOperationException()
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="UnauthorizedDomainOperationException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di autorizzazione.</param>
        public UnauthorizedDomainOperationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="UnauthorizedDomainOperationException"/> con un messaggio descrittivo
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di autorizzazione.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public UnauthorizedDomainOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Preserve original exception details and stack trace.
        }

        #endregion
    }
}
```

Questa definizione la colloca esplicitamente:

* nel namespace del dominio (`JokesApp.Server.Domain.Exceptions`),
* come sottoclasse di `DomainOperationException`,
* con i costruttori standard già previsti per le altre eccezioni di dominio.

---

## 1.18 Obiettivi progettuali di `UnauthorizedDomainOperationException`

La progettazione di questa eccezione persegue alcuni obiettivi chiari:

1. **Esplicitare il concetto di “operazione non autorizzata” a livello di dominio**

   Il dominio deve poter esprimere, con un tipo dedicato, che l’operazione richiesta
   è concettualmente vietata per il soggetto corrente, indipendentemente da come
   questo si tradurrà poi a livello di protocollo (es. HTTP 403).

   Il nome `UnauthorizedDomainOperationException` rende immediato il significato
   sia per chi legge il codice, sia per chi gestirà il mapping negli strati esterni.

2. **Mantenere la coerenza con il modello di “operazioni non valide”**

   Essendo una sottoclasse di `DomainOperationException`, `UnauthorizedDomainOperationException`:

   * è intercettabile come “errore operativo di dominio”:

     ```csharp
     catch (DomainOperationException ex)
     {
         // Gestione generica di tutte le operazioni di dominio non valide
     }
     ```

   * può essere distinta quando serve una gestione più specifica:

     ```csharp
     catch (UnauthorizedDomainOperationException ex)
     {
         // Gestione specifica di operazioni non autorizzate (es. mappare su 403)
     }
     ```

   In questo modo, il modello di errori rimane **fortemente gerarchico ma flessibile**.

3. **Supportare i pattern standard di gestione delle eccezioni**

   Grazie ai costruttori disponibili:

   * è possibile creare un’eccezione con solo un messaggio esplicativo,
   * è possibile wrappare un’eccezione tecnica dentro un contesto di dominio
     specifico di autorizzazione, preservando la stack trace (`innerException`).

---

## 1.19 Differenza rispetto a `DomainOperationException` e `DomainValidationException`

Per capire dove usare `UnauthorizedDomainOperationException`, è utile metterla
a confronto con le altre principali eccezioni di dominio:

* **`DomainValidationException`**

  * Errori legati a **invarianti** e **validazione dei dati**:

    * formati non validi,
    * stringhe vuote o eccessivamente lunghe,
    * valori fuori range, ecc.
  * Tipicamente lanciata da Value Objects e factory di entità.

* **`DomainOperationException`**

  * Errori dovuti a **operazioni non valide rispetto allo stato dell’oggetto**:

    * stato dell’aggregato che non consente una certa transizione,
    * regole di business legate al lifecycle (“non puoi fare X dopo che Y è successo”).

* **`UnauthorizedDomainOperationException`**

  * Errori dovuti a **mancanza di permessi** da parte del soggetto che tenta l’operazione:

    * utente non proprietario che tenta di modificare una joke,
    * ruolo non sufficiente per eseguire una determinata azione.

In sintesi:

* *Cosa stai facendo* non è consentito → `DomainOperationException`.
* *Tu non puoi farlo* (anche se l’operazione in sé sarebbe valida) → `UnauthorizedDomainOperationException`.
* I dati con cui stai facendo l’operazione non sono validi → `DomainValidationException`.

---

## 1.20 Esempi pratici d’uso nel dominio

**1. Controllo di ownership su una joke**

```csharp
public void UpdateContent(UserId currentUserId, QuestionText newQuestion, AnswerText newAnswer)
{
    if (!currentUserId.Equals(OwnerId))
    {
        // Business rule: only the owner can update this joke.
        throw new UnauthorizedDomainOperationException(
            "The current user is not allowed to update this joke."
        );
    }

    Question = newQuestion;
    Answer = newAnswer;
}
```

In questo caso:

* l’operazione di update è legittima nella logica del dominio,
* lo stato della joke consente la modifica,
* ma l’utente corrente non rispetta la regola di autorizzazione definita dal dominio
  (solo il proprietario può modificare).

---

## 1.21 Coerenza con DDD, Clean Architecture e SOLID

`UnauthorizedDomainOperationException` è perfettamente allineata ai principi
architetturali del progetto:

* **DDD**

  * Esprime un concetto preciso del linguaggio ubiquo: “operazione di dominio non autorizzata”.
  * Rende le regole di sicurezza/autorizzazione parte esplicita del modello di dominio,
    anziché lasciare la responsabilità solo all’infrastruttura.

* **Clean Architecture**

  * Rimane confinata nel Domain Layer, senza introdurre dipendenze verso framework di sicurezza,
    identity provider, protocolli di autenticazione o layer di presentazione.
  * La decisione di mappare questa eccezione su un determinato status code (es. HTTP 403) o
    su un determinato payload JSON è responsabilità dei layer esterni.

* **SOLID (Single Responsibility Principle)**

  * Ha una responsabilità singola e chiara: rappresentare l’errore di autorizzazione
    di un’operazione di dominio.
  * Non contiene logica di validazione, persistenza o presentazione; è un puro
    oggetto di segnalazione semantica.

---

## 1.22 Linee guida per l’estensione

In linea con il principio YAGNI, non è necessario introdurre sottoclassi ulteriori
di `UnauthorizedDomainOperationException` finché non esiste un valore chiaro nel farlo.

Tuttavia, il fatto che **non sia sealed** lascia aperte le seguenti possibilità:

* definire eccezioni più specifiche in domini molto complessi
  (es. `OwnerRequiredDomainOperationException`),
* creare sottotipi per scenari di sicurezza particolari, mantenendo sempre la
  possibilità di intercettare tutto come `DomainOperationException` o come `DomainException`.

La linea guida generale resta:

> usare `UnauthorizedDomainOperationException` ogni volta che un’operazione, altrimenti valida,
> viene negata esclusivamente per motivi di permessi/autorizzazione nel dominio.

---
