# 📘 **04a_JokeId.md**

### *Value Object per l’identificatore della barzelletta*

---

## 4.17 Ruolo nel dominio

Nel sottodominio **Joke**, l’identificatore della barzelletta non è gestito come un semplice `int`,
ma come un **Value Object tipizzato**: `JokeId`.

L’obiettivo è duplice:

- distinguere chiaramente, a livello di tipo, un “identificatore di Joke” da un qualsiasi `int`
  utilizzato per altri scopi;
- garantire che ogni identificatore che circola nel **Domain Layer** rispetti gli **invarianti**
  stabiliti dal modello (in questo caso: deve essere un intero strettamente positivo).

In questo modo:

- si riducono gli errori dovuti a scambi di parametri (`int` usati in modo invertito o errato),
- si rende più espressivo il codice (una firma con `JokeId` comunica immediatamente l’intento),
- si separano in modo netto le scelte di persistenza (PK nel database) dalla rappresentazione
  concettuale nel dominio.

---

## 4.18 Definizione della struttura

```csharp
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Identificatore tipizzato e immutabile per la barzelletta.
    /// Deve rappresentare sempre un intero positivo e valido nel dominio.
    /// </summary>
    public readonly record struct JokeId
    {
        #region Properties

        /// <summary>
        /// Valore numerico dell'identificatore.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Indica se l'identificatore rappresenta uno stato non inizializzato
        /// o non valido (0 o qualsiasi valore non positivo).
        /// </summary>
        public bool IsEmpty => Value <= 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato: nel codice applicativo la creazione dovrebbe passare
        /// tramite <see cref="Create(int)"/> oppure tramite <see cref="Empty"/>.
        /// Come per tutti gli struct in C#, esiste comunque un costruttore di default
        /// che produce uno stato equivalente a <see cref="Empty"/> (Value &lt;= 0).
        /// </summary>
        
        private JokeId(int value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Crea un identificatore di barzelletta valido, garantendo che sia strettamente positivo.
        /// </summary>
        /// <param name="value">Valore numerico da utilizzare come identificatore.</param>
        /// <returns>Un'istanza valida di <see cref="JokeId"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è minore o uguale a zero.
        /// </exception>
        public static JokeId Create(int value)
        {
            if (value <= 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdInvalid,
                    nameof(JokeId));
            }

            // At this point, the identifier is a valid domain value.
            return new JokeId(value);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Identificatore "vuoto", utilizzato come placeholder iniziale
        /// (ad esempio prima che Entity Framework assegni il valore reale).
        /// </summary>
        public static JokeId Empty { get; } = new JokeId(0);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce una rappresentazione testuale dell'identificatore.
        /// </summary>
        public override string ToString() => Value.ToString();

        #endregion
    }
}
```

> 🔎 Nota sul costruttore di default (struct)
> JokeId è implementato come readonly record struct. Questo implica che, oltre al costruttore privato usato internamente, esiste anche il costruttore di default di C# (default(JokeId) / new JokeId()), che inizializza Value a 0.
> Nel modello di dominio, qualsiasi valore Value <= 0 viene considerato come stato “vuoto/non inizializzato” (IsEmpty == true), equivalente alla costante JokeId.Empty. Per questo motivo, anche gli eventuali default(JokeId) sono trattati correttamente come identificatori non inizializzati.


Elementi chiave di design

* `readonly record struct` → Value Object leggero, immutabile, confrontabile per valore;
* proprietà `Value` readonly → incapsula l’intero usato come identificatore;
* costruttore privato → impedisce la creazione arbitraria, imponendo il passaggio da `Create` o da `Empty`;
* metodo statico `Create` → unica via “normale” per ottenere un `JokeId` valido;
* membro statico `Empty` → placeholder controllato per scenari tecnici (EF, binding, inizializzazioni).

---

## 4.19 Invarianti e regole di validazione

L’invariante principale di `JokeId` è molto chiaro:

> “Un identificatore di joke valido deve essere un intero strettamente positivo.”

Questa regola viene applicata nella factory `Create`:

```csharp
public static JokeId Create(int value)
{
    if (value <= 0)
    {
        throw new DomainValidationException(
            JokeErrorMessages.JokeIdInvalid,
            nameof(JokeId));
    }

    // At this point, the identifier is a valid domain value.
    return new JokeId(value);
}
```

Qualunque tentativo di istanziare un `JokeId` con un valore:

* pari a 0,
* oppure negativo,

viene respinto con una `DomainValidationException`, utilizzando il messaggio
`JokeErrorMessages.JokeIdInvalid` e specificando `nameof(JokeId)` come `MemberName`.

In questo modo:

* il Domain Layer non può mai trovarsi con un `JokeId` “valido” che violi l’invariante,
* gli strati superiori (Application/API) possono identificare chiaramente l’origine
  dell’errore (il membro `JokeId`).

La proprietà:

```csharp
public bool IsEmpty => Value <= 0;
```

fornisce inoltre un controllo rapido per distinguere tra:

* identificatori validi (`Value > 0`),
* stati “vuoti” o non inizializzati (`Value <= 0`), tipicamente collegati a `Empty`
  o a valori tecnici di placeholder.

---

## 4.20 Placeholder `Empty` e semantica di `IsEmpty`

`JokeId` espone un membro statico:

```csharp
public static JokeId Empty { get; } = new JokeId(0);
```

`Empty` non rappresenta un identificatore valido nel senso del dominio, ma un **segnaposto tecnico**:

* può essere utilizzato prima che un ORM (es. EF Core) assegni l’identificatore reale alla joke,
* può fungere da valore di default in binding o test,
* evita l’uso di “magic numbers” come `0` direttamente nel codice applicativo.

La proprietà:

```csharp
public bool IsEmpty => Value <= 0;
```

permette di verificare velocemente se un `JokeId` si trova in questo stato “non inizializzato”
(o comunque non valido per il dominio). È un approccio difensivo che tratta qualunque valore
non strettamente positivo come “vuoto” o non utilizzabile come identificatore reale.

Regola pratica:

* per creare identificatori **validi di dominio** → usare sempre `JokeId.Create(int)`;
* per placeholder tecnici → usare `JokeId.Empty` e verificare con `IsEmpty`.

---

## 4.21 Utilizzo tipico nel dominio e con la persistenza

**1. All’interno dell’entità `Joke`**

```csharp
public class Joke
{
    public JokeId Id { get; private set; }

    // Costruttore di dominio, ad esempio usato da una factory
    private Joke(JokeId id, QuestionText question, AnswerText answer, UserId userId)
    {
        Id       = id;
        Question = question;
        Answer   = answer;
        UserId   = userId;
    }
}
```

Il fatto di usare `JokeId` al posto di `int` rende immediatamente più leggibile e sicura
l’API dell’entità/aggregate.

**2. Mapping da/perso DTO o layer applicativo**

Quando l’Application Layer riceve un identificatore come `int` (ad esempio da una route HTTP),
può convertirlo in `JokeId` tramite la factory:

```csharp
public async Task<JokeDto> GetJokeAsync(int id)
{
    var jokeId = JokeId.Create(id);

    var joke = await _jokeRepository.GetByIdAsync(jokeId);
    // ...
}
```

In questo punto, eventuali valori non validi (0, negativi) vengono immediatamente respinti
come `DomainValidationException`, semplificando la logica di gestione errori a valle.

**3. Integrazione con EF Core**

In scenari con Entity Framework Core è comune:

* mappare la proprietà `Value` come chiave primaria (`Key`) della tabella,
* utilizzare `JokeId` come wrapper tipizzato nella parte di dominio.

Ad esempio (pseudo-configurazione):

```csharp
builder.Property(j => j.Id)
       .HasConversion(
           id => id.Value,
           value => JokeId.Create(value));
```

L’uso di `Empty` può essere utile nelle fasi di costruzione dell’oggetto prima che il
database assegni un identificatore definitivo.

---

## 4.22 Coerenza con DDD, Clean Architecture e SOLID

`JokeId` è pienamente allineato ai principi che guidano l’architettura:

* **DDD**

  * Modella esplicitamente un concetto del dominio (“identificatore di Joke”)
    anziché utilizzare un tipo primitivo generico.
  * L’invariante (intero strettamente positivo) è codificato direttamente nel Value Object.

* **Clean Architecture**

  * Vive nel Domain Layer e non dipende da framework o dettagli infrastrutturali.
  * La traduzione da/verso tipi primitivi (int, chiavi DB, ecc.) è demandata ai layer esterni
    (Application, Infrastructure).

* **SOLID (SRP)**

  * Ha una responsabilità unica: rappresentare in modo sicuro l’identificatore di una Joke.
  * Non contiene logica di persistenza, mapping DTO, logging o altro.

---

## 4.23 Linee guida per estensioni future

Nel caso in cui i requisiti evolvano (es. passaggio da `int` a `long`, o ad un GUID, o ad
un identificatore più complesso):

* il punto da modificare sarà principalmente `JokeId` (tipo di `Value`, factory `Create`,
  validazione, conversioni);
* il resto del dominio continuerà a lavorare con `JokeId` come concetto, senza essere
  impattato direttamente dal cambio di tipo fisico.

Questo è uno dei vantaggi principali di aver introdotto un Value Object tipizzato:

> l’**identità concettuale** di una Joke è separata dalla **rappresentazione tecnica**
> dell’identificatore, rendendo l’evoluzione molto più controllata e meno invasiva.

---


