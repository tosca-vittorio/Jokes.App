# 📘 **04a_JokeId.md**

### *Value Object per l’identificatore della barzelletta*

---

## 4.17 Ruolo nel dominio

Nel sottodominio **Joke**, l’identificatore della barzelletta non è gestito come un semplice `Guid`,
ma come un **Value Object tipizzato**: `JokeId`.

L’obiettivo è duplice:

- distinguere chiaramente, a livello di tipo, un “identificatore di Joke” da un qualsiasi `Guid`
  utilizzizzato per altri scopi;
- garantire che ogni identificatore che circola nel **Domain Layer** rispetti gli **invarianti**
  stabiliti dal modello (in questo caso: deve essere un Guid tipizzato, non deve essere Guid.Empty e generato nel dominio).

In questo modo:

- si riducono gli errori dovuti a scambi di parametri (`Guid` usati in modo invertito o errato),
- si rende più espressivo il codice (una firma con `JokeId` comunica immediatamente l’intento),
- si separano in modo netto le scelte di persistenza (PK nel database) dalla rappresentazione
  concettuale nel dominio.

---

## 4.18 Definizione della struttura

```csharp
using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Identificatore tipizzato e immutabile per la barzelletta.
    /// Viene generato nel dominio per essere disponibile immediatamente (es. Domain Events).
    /// </summary>
    /// <remarks>
    /// Essendo uno <c>struct</c>, in C# esiste sempre un costruttore di default che produce
    /// uno stato equivalente a <see cref="Empty"/> (cioè <see cref="Guid.Empty"/>).
    /// Nel dominio non dovresti mai emettere eventi o accettare stati "vuoti" come fatto di business:
    /// per ottenere un Id valido usa <see cref="New()"/>; per reidratazione usa <see cref="Create(Guid)"/>.
    /// </remarks>
    public readonly record struct JokeId
    {
        #region Properties

        /// <summary>
        /// Valore dell'identificatore.
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// Indica se l'identificatore rappresenta uno stato non inizializzato (<see cref="Guid.Empty"/>).
        /// </summary>
        public bool IsEmpty => Value == Guid.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato.
        /// La creazione nel codice applicativo deve passare da <see cref="Create(Guid)"/> o <see cref="New()"/>.
        /// </summary>
        /// <param name="value">Valore dell'identificatore.</param>
        private JokeId(Guid value)
        {
            Value = value;
        }

        #endregion

        #region Factories

        /// <summary>
        /// Crea un identificatore valido a partire da un valore già noto (es. reidratazione da persistenza).
        /// </summary>
        /// <param name="value">Guid già noto (non deve essere <see cref="Guid.Empty"/>).</param>
        /// <returns>Un <see cref="JokeId"/> valido.</returns>
        /// <exception cref="DomainValidationException">
        /// Lanciata se <paramref name="value"/> è <see cref="Guid.Empty"/>.
        /// </exception>
        public static JokeId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(JokeId));
            }

            return new JokeId(value);
        }


        /// <summary>
        /// Genera un nuovo identificatore valido per una barzelletta.
        /// </summary>
        /// <returns>Un <see cref="JokeId"/> valido.</returns>
        public static JokeId New()
            => new JokeId(Guid.NewGuid());

        #endregion

        #region Static members

        /// <summary>
        /// Identificatore "vuoto" (stato non inizializzato / placeholder tecnico).
        /// </summary>
        public static JokeId Empty { get; } = new JokeId(Guid.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce una rappresentazione testuale dell'identificatore.
        /// </summary>
        /// <returns>Il valore <see cref="Guid"/> in formato stringa.</returns>
        public override string ToString() => Value.ToString();

        #endregion
    }
}
```

> 🔎 Nota sul costruttore di default (struct)
> JokeId è implementato come readonly record struct. Questo implica che, oltre al costruttore privato usato internamente, esiste anche il costruttore di default di C# (default(JokeId) / new JokeId()), che inizializza Value a Guid.Empty.
> Nel modello di dominio, Guid.Empty viene considerato uno stato “vuoto/non inizializzato” (IsEmpty == true), equivalente alla costante JokeId.Empty. Per questo motivo, anche gli eventuali default(JokeId) sono trattati correttamente come identificatori non inizializzati.



Elementi chiave di design

* `readonly record struct` → Value Object leggero, immutabile, confrontabile per valore;
* proprietà `Value` readonly → incapsula il `Guid` usato come identificatore;
* costruttore privato → impedisce la creazione arbitraria, imponendo il passaggio da `Create` o da `New`;
* metodo statico `Create(Guid)` → unica via “controllata” per ottenere un `JokeId` valido a partire da un valore già noto (non `Guid.Empty`);
* metodo statico `New()` → genera un nuovo identificatore valido nel dominio;
* membro statico `Empty` → placeholder controllato per scenari tecnici (mapping/persistenza, binding, inizializzazioni).


---

## 4.19 Invarianti e regole di validazione

L’invariante principale di `JokeId` è molto chiaro:

> “Un identificatore di joke valido deve essere un Guid tipizzato, non deve essere Guid.Empty e generato nel dominio.”

Questa regola viene applicata nella factory `Create`:

```csharp
public static JokeId Create(Guid value)
{
    if (value == Guid.Empty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.JokeIdEmpty,
            nameof(JokeId));
    }

    return new JokeId(value);
}
```

Qualunque tentativo di istanziare un `JokeId` con:

* `value == Guid.Empty` → considerato identificatore vuoto/non inizializzato → `JokeErrorMessages.JokeIdEmpty`
* qualunque altro `Guid` → identificatore valido nel dominio

viene gestito tramite `DomainValidationException`, specificando `nameof(JokeId)` come `MemberName`.

Nota: il messaggio `JokeErrorMessages.JokeIdInvalid` è tipicamente utile quando un identificatore arriva come stringa/valore non parsabile (scenario gestito fuori dal Domain, es. Application/API), mentre questo Value Object lavora deliberatamente su `Guid` già materializzati.


In questo modo:

* il Domain Layer non può mai trovarsi con un `JokeId` “valido” che violi l’invariante,
* gli strati superiori (Application/API) possono identificare chiaramente l’origine
  dell’errore (il membro `JokeId`).

La proprietà:

```csharp
public bool IsEmpty => Value == Guid.Empty;
```

fornisce inoltre un controllo rapido per distinguere tra:

* identificatori validi (`Value != Guid.Empty`),
* stati “vuoti” o non inizializzati (`Value == Guid.Empty`), tipicamente collegati a `Empty`
  o a valori tecnici di placeholder.

---

## 4.20 Placeholder `Empty` e semantica di `IsEmpty`

`JokeId` espone un membro statico:

```csharp
public static JokeId Empty { get; } = new JokeId(Guid.Empty);
```

`Empty` non rappresenta un identificatore valido nel senso del dominio, ma un **segnaposto tecnico**:

* può essere utilizzato prima che un ORM (mapping/persistenza) assegni l’identificatore reale alla joke,
* può fungere da valore di default in binding o test,
* evita l’uso di "magic values come `Guid.Empty`" direttamente nel codice applicativo.

La proprietà:

```csharp
public bool IsEmpty => Value == Guid.Empty;
```

* per creare identificatori **validi di dominio** → usare `JokeId.New()`; per reidratazione usare `JokeId.Create(Guid)`;
* per placeholder tecnici → usare `JokeId.Empty` e verificare con `IsEmpty`.

Regola pratica:

* per creare identificatori **validi di dominio** → usare `JokeId.New()` per **nuovi Id**, `JokeId.Create(Guid)` per **reidratazione**
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

Il fatto di usare `JokeId` al posto di `Guid` rende immediatamente più leggibile e sicura
l’API dell’entità/aggregate.

**2. Mapping da/perso DTO o layer applicativo**

Quando l’Application Layer riceve un identificatore come `Guid` (ad esempio da una route HTTP),
può convertirlo in `JokeId` tramite la factory:

```csharp
public async Task<JokeDto> GetJokeAsync(Guid id)
{
    var jokeId = JokeId.Create(id);

    var joke = await _jokeRepository.GetByIdAsync(jokeId);
    // ...
}
```

In questo punto, eventuali valori non validi (Guid.Empty) vengono immediatamente respinti
come `DomainValidationException`, semplificando la logica di gestione errori a valle.

**3. Integrazione con la persistenza/ORM mapping (esempio EF Core)**

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
  * L’invariante (Guid tipizzato, non deve essere Guid.Empty e generato nel dominio) è codificato direttamente nel Value Object.

* **Clean Architecture**

  * Vive nel Domain Layer e non dipende da framework o dettagli infrastrutturali.
  * La traduzione da/verso tipi primitivi (string/Guid/chiavi DB, ecc.) è demandata ai layer esterni
    (Application, Infrastructure).

* **SOLID (SRP)**

  * Ha una responsabilità unica: rappresentare in modo sicuro l’identificatore di una Joke.
  * Non contiene logica di persistenza, mapping DTO, logging o altro.

---

## 4.23 Linee guida per estensioni future

Nel caso in cui i requisiti evolvano (es. passaggio da Guid a Ulid/string/identificatore composto)

* il punto da modificare sarà principalmente `JokeId` (tipo di `Value`, factory `Create`,
  validazione, conversioni);
* il resto del dominio continuerà a lavorare con `JokeId` come concetto, senza essere
  impattato direttamente dal cambio di tipo fisico.

Questo è uno dei vantaggi principali di aver introdotto un Value Object tipizzato:

> l’**identità concettuale** di una Joke è separata dalla **rappresentazione tecnica**
> dell’identificatore, rendendo l’evoluzione molto più controllata e meno invasiva.

---


