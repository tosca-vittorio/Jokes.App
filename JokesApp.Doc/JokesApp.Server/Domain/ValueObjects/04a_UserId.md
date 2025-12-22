# 📘 **04_UserId.md**

### *Value Object per l’identificatore utente*

---

## 4.24 Ruolo nel dominio

`UserId` rappresenta l’identificatore **tipizzato** di un utente all’interno del dominio.

Pur essendo, tecnicamente, una stringa (in linea con ASP.NET Core Identity), dal punto di vista
del dominio non vogliamo trattarlo come una `string` generica, ma come un concetto esplicito:

- identifica un utente in modo univoco,
- deve rispettare specifici vincoli di validazione (non nullo, non vuoto, lunghezza massima),
- viene usato per associare una `Joke` al suo autore (tramite `ApplicationUserId`), per controlli di autorizzazione, ecc.

Modellarlo come **Value Object** (`UserId`) invece che come `string`:

- rende le API di dominio più espressive (`UserId` comunica chiaramente che si tratta di un ID utente),
- riduce il rischio di errori dovuti a scambi di parametri (`string` passate nel posto sbagliato),
- centralizza in un solo punto le **regole di validazione** sull’identificatore utente.

`UserId` è immutabile, auto-validante e allineato ai vincoli di lunghezza tipici di Identity Core
(`MaxLength = 450`).

---

## 4.25 Definizione della struttura

```csharp
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Identificatore tipizzato dell'utente, conforme alle regole del dominio
    /// e ai vincoli di lunghezza di Identity Core.
    /// Immutabile, auto-validante e non può rappresentare un valore invalido.
    /// </summary>
    public readonly record struct UserId
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita per un identificativo utente (Identity Core).
        /// </summary>
        public const int MaxLength = 450;

        #endregion

        #region Properties

        /// <summary>
        /// Valore stringa dell'identificativo utente.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il valore corrente è vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato per garantire che la creazione passi sempre tramite
        /// <see cref="Create(string?)"/> o tramite <see cref="Empty"/>.
        /// Come per tutti gli struct in C#, esiste comunque un costruttore di default
        /// che produce uno stato equivalente a <see cref="Empty"/> (Value nullo o vuoto).
        /// </summary>
        /// <param name="value">Valore stringa già validato o placeholder.</param>
        private UserId(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un identificativo coerente con le regole del dominio.
        /// </summary>
        /// <param name="value">Valore stringa da utilizzare come identificativo utente.</param>
        /// <returns>Un'istanza valida di <see cref="UserId"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static UserId Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(UserId));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdTooLong,
                    nameof(UserId));
            }

            // At this point, the identifier is a valid domain value.
            return new UserId(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Rappresenta un identificativo vuoto o non inizializzato.
        /// Usato come placeholder per EF Core e scenari di default.
        /// </summary>
        public static UserId Empty { get; } = new UserId(string.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce la rappresentazione testuale dell'identificativo utente.
        /// </summary>
        public override string ToString() => Value;

        #endregion
    }
}
```

Punti chiave:

* `readonly record struct` → Value Object leggero, immutabile, confrontabile per valore;
* proprietà `Value` → incapsula la stringa usata come identificatore;
* costruttore privato → impedisce istanziazioni non validate (`new UserId("...")`),
  forzando il passaggio da `Create` o da `Empty`;
* `MaxLength = 450` → allineato ai vincoli di ASP.NET Core Identity e alle scelte di persistenza.

---

## 4.26 Invarianti e regole di validazione

Gli invarianti principali di `UserId` sono:

1. **Non nullo e non vuoto**

   ```csharp
   if (string.IsNullOrWhiteSpace(value))
   {
       throw new DomainValidationException(
           ApplicationUserErrorMessages.UserIdNullOrEmpty,
           nameof(UserId));
   }
   ```

   Il dominio non accetta un identificatore utente nullo, vuoto o composto solo da spazi:
   chi costruisce il VO deve fornire un identificativo significativo.

2. **Lunghezza massima**

   ```csharp
   var trimmed = value.Trim();

   if (trimmed.Length > MaxLength)
   {
       throw new DomainValidationException(
           ApplicationUserErrorMessages.UserIdTooLong,
           nameof(UserId));
   }
   ```

   L’input viene normalizzato rimuovendo spazi iniziali/finali e viene imposto
   un limite di lunghezza (`MaxLength = 450`), compatibile con Identity Core e
   con i vincoli del database.

Se una di queste regole fallisce, `UserId.Create` lancia una `DomainValidationException`:

* con messaggio proveniente da `ApplicationUserErrorMessages` (`UserIdNullOrEmpty`, `UserIdTooLong`),
* con `MemberName` impostato a `nameof(UserId)`.

Questo è perfettamente allineato alla strategia di validazione usata per gli altri VO (`QuestionText`, `AnswerText`, `JokeId`).

---

## 4.27 Placeholder `Empty` e semantica di `IsEmpty`

`UserId` espone un placeholder statico:

```csharp
public static UserId Empty { get; } = new UserId(string.Empty);
```

`Empty` non rappresenta un identificativo valido nel dominio, ma uno **stato tecnico**:

* utile per EF Core (prima del popolamento effettivo dei dati),
* utile in scenari di binding iniziale o test,
* evita l’uso disordinato di `null` o stringhe vuote sparse nel codice.

La proprietà:

```csharp
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
```

permette di verificare rapidamente se un `UserId` è in questo stato “non inizializzato”
o “placeholder”. In combinazione:

* `UserId.Create(...)` → crea solo istanze valide secondo il dominio,
* `UserId.Empty` → fornisce l’unico “caso speciale” ammesso, gestito e riconoscibile tramite `IsEmpty`.

Regola di utilizzo:

* per rappresentare un utente reale nel dominio → usare sempre `UserId.Create(...)`;
* per placeholder tecnici → usare `UserId.Empty` e, dove necessario, verificare `IsEmpty`.

---

## 4.28 Utilizzo tipico nel dominio

**1. All’interno dell’entità `Joke` (autore della barzelletta)**

```csharp
public class Joke
{
    public UserId ApplicationUserId { get; private set; }

    public void ChangeAuthor(UserId newAuthorId)
    {
        if (newAuthorId.IsEmpty)
        {
            throw new DomainValidationException(
                ApplicationUserErrorMessages.UserIdNullOrEmpty,
                nameof(ApplicationUserId));
        }

        ApplicationUserId = newAuthorId;
    }
}
```

In questo scenario, l’entità non lavora con stringhe anonime, ma con un VO tipizzato che
garantisce già a monte la validità del valore. La logica di validazione del formato/lunghezza
resta nel VO; qui si intercetta solo il caso “placeholder/non inizializzato”.

**2. Mapping da input (route, DTO, claims)**

```csharp
public async Task<List<JokeDto>> GetUserJokesAsync(string rawUserId)
{
    var userId = UserId.Create(rawUserId);

    var jokes = await _jokeRepository.GetByUserIdAsync(userId);
    // ...
}
```

Se `rawUserId` è nullo, vuoto o troppo lungo, la creazione fallisce immediatamente con
una `DomainValidationException`, lasciando al layer applicativo il compito di tradurre
l’errore in una risposta adeguata (HTTP 400, ad esempio).

**3. Integrazione con Identity Core**

Nella pratica, il valore di `UserId` deriverà spesso da:

* `UserManager<TUser>.GetUserId(...)`,
* `ClaimsPrincipal`,
* token JWT, ecc.

Il fatto di incapsulare questo valore in `UserId` non cambia la logica di Identity,
ma aggiunge un livello di sicurezza e chiarezza a livello di dominio:

```csharp
var userIdString = _userManager.GetUserId(User); // string
var userId = UserId.Create(userIdString);        // Value Object

// Da qui in poi, il dominio usa sempre UserId, non string.
```

---

## 4.29 Coerenza con DDD, Clean Architecture e SOLID

`UserId` si allinea perfettamente con l’impostazione generale del progetto:

* **DDD**

  * l’identificatore dell’utente è un concetto del dominio, non una semplice stringa;
  * le regole di validazione vengono collocate dove appartengono: nel VO stesso.

* **Clean Architecture**

  * `UserId` vive nel Domain Layer, senza dipendere da framework (Identity Core è un vincolo implicito solo sulla lunghezza);
  * la traduzione tra `UserId` e il tipo stringa utilizzato dall’infrastruttura (Identity/DB) avviene nei layer esterni.

* **SOLID (SRP)**

  * `UserId` ha una responsabilità unica: rappresentare in modo sicuro e valido l’ID di un utente;
  * non si occupa di logica applicativa, persistenza, mapping DTO, o policy di sicurezza:
    si limita a garantire la correttezza del suo valore.

---

## 4.30 Evoluzione e allineamento con altri Value Object

`UserId` segue gli stessi pattern di:

* `JokeId` (identificatore tipizzato numerico),
* `QuestionText` e `AnswerText` (VO testuali).

Questo porta diversi vantaggi:

* uno stile uniforme su tutti i VO del dominio,
* una strategia di validazione coerente basata su `DomainValidationException` e messaggi centralizzati
  (`ApplicationUserErrorMessages` per l’utente, `JokeErrorMessages` per la joke),
* la possibilità, in futuro, di cambiare il formato dell’ID (ad esempio da string a GUID) intervenendo principalmente sul VO, mantenendo inalterate le firme che usano `UserId`.

In sintesi:

> `UserId` è il punto di verità per tutto ciò che riguarda l’identificativo utente nel Domain Layer.
> Se un valore non supera `UserId.Create`, non è un ID valido per il dominio.

---
