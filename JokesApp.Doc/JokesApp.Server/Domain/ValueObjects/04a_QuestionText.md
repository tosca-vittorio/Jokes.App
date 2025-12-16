# 📘 **04a_QuestionText.md**

### *Value Object per il testo della domanda*

---

## 4.1 Ruolo nel dominio

Nel sottodominio **Joke**, il testo della domanda (question) è un elemento centrale del modello:
non è una semplice `string`, ma un concetto dotato di regole proprie:

- non può essere nullo o vuoto,
- non può superare una lunghezza massima,
- può essere normalizzato (es. trimming degli spazi).

Per evitare di avere queste regole sparse in più punti del codice e applicate in modo incoerente,
il testo della domanda viene modellato come **Value Object** dedicato: `QuestionText`.

`QuestionText`:

- incapsula il valore testuale e tutte le **regole di validazione locali**,
- garantisce che ogni istanza valida rispetti gli **invarianti di dominio**,
- è immutabile e dotato di semantica di **value-based equality** (grazie al `record`),
- viene usato all’interno dell’entità/aggregate `Joke` al posto di una semplice `string`.

In questo modo il codice che lavora con le joke non manipola più “stringhe generiche”,
ma oggetti che esprimono esplicitamente il concetto di “testo di domanda valido nel dominio”.

---

## 4.2 Definizione della classe

```csharp
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta il testo della domanda di una barzelletta.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record QuestionText
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita per la domanda.
        /// </summary>
        public const int MaxLength = 200;

        #endregion

        #region Properties

        /// <summary>
        /// Valore testuale della domanda.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il testo rappresenta un valore vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Restituisce la lunghezza del testo interno.
        /// </summary>
        public int Length => Value.Length;

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato: l'unica via per creare il VO è tramite <see cref="Create(string?)"/>.
        /// </summary>
        /// <param name="value">Testo della domanda già validato.</param>
        private QuestionText(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un nuovo <see cref="QuestionText"/>
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">Testo della domanda da validare.</param>
        /// <returns>Un'istanza valida di <see cref="QuestionText"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static QuestionText Create(string? value)
        {
            if (value is null)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(QuestionText));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length == 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(QuestionText));
            }

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionTooLong,
                    nameof(QuestionText));
            }

            // At this point the value is valid according to the domain rules.
            return new QuestionText(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Rappresenta un valore vuoto, utile come placeholder per scenari tecnici di mapping/persistenza, test o binding.
        /// </summary>
        public static QuestionText Empty { get; } = new QuestionText(string.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce il valore testuale interno.
        /// </summary>
        public override string ToString() => Value;

        #endregion
    }
}
```

Elementi chiave:

* **namespace**: `JokesApp.Server.Domain.ValueObjects` → appartiene esplicitamente al Domain Layer;
* `sealed record` → Value Object immutabile, con equality basata sui valori;
* proprietà pubblica `Value` readonly → espone il testo normalizzato e validato;
* costante `MaxLength` → definisce l’invariante di lunghezza massima (200 caratteri);
* metodo statico `Create` → unica porta di ingresso per creare istanze valide;
* uso di `DomainValidationException` + `JokeErrorMessages` → integrazione completa con il sistema di errori di dominio;
* membro statico `Empty` → sentinel/placeholder controllato, utile in scenari particolari (mapping/persistenza, binding, test).

---

## 4.3 Invarianti e regole di validazione

Il metodo `Create` è il punto in cui vengono applicate le regole di dominio sul testo
della domanda. Le regole attuali sono:

1. **Non null**

   ```csharp
   if (value is null)
   {
       throw new DomainValidationException(
           JokeErrorMessages.QuestionNullOrEmpty,
           nameof(QuestionText));
   }
   ```

   Il dominio non accetta una question “non definita”: il chiamante è obbligato a fornire
   un valore.

2. **Trimming e non vuoto**

   ```csharp
   var trimmed = value.Trim();

   if (trimmed.Length == 0)
   {
       throw new DomainValidationException(
           JokeErrorMessages.QuestionNullOrEmpty,
           nameof(QuestionText));
   }
   ```

   Prima si normalizza l’input rimuovendo gli spazi iniziali/finali; una stringa composta
   solo da spazi viene considerata vuota e non valida.

3. **Lunghezza massima**

   ```csharp
   if (trimmed.Length > MaxLength)
   {
       throw new DomainValidationException(
           JokeErrorMessages.QuestionTooLong,
           nameof(QuestionText));
   }
   ```

   Viene imposto un limite di lunghezza (`MaxLength = 200`) per garantire:

   * coerenza con requisiti di UI/UX,
   * compatibilità con vincoli di persistenza,
   * leggibilità e chiarezza delle joke.

Se una di queste regole viene violata, viene lanciata una `DomainValidationException` con:

* **messaggio** preso da `JokeErrorMessages` (`QuestionNullOrEmpty`, `QuestionTooLong`),
* **MemberName** impostato a `nameof(QuestionText)`, utile per logging e mapping verso il client.

---

## 4.4 Immutabilità e semantica di Value Object

`QuestionText` è dichiarato come:

```csharp
public sealed record QuestionText
```

Questo implica:

* **immutabilità**: una volta creata, l’istanza non può cambiare il suo valore interno;
* **value-based equality**: due `QuestionText` con lo stesso `Value` sono considerati uguali,
  cosa perfettamente coerente con il concetto di Value Object in DDD;
* `sealed` → impedisce ulteriori derivazioni, evitando gerarchie inutili per un concetto
  così atomico.

Queste caratteristiche garantiscono che:

* il codice che usa `QuestionText` può ragionare in termini di “oggetti valore”,
* non si rischiano modifiche silenziose al testo dopo la convalida,
* confronti e utilizzo in collezioni (Dictionary, HashSet, ecc.) siano affidabili.

---

## 4.5 Membro statico `Empty` e proprietà di utilità

`QuestionText` espone un membro statico:

```csharp
public static QuestionText Empty { get; } = new QuestionText(string.Empty);
```

Questo rappresenta una istanza “vuota” deliberatamente creata **bypassando** le regole
di validazione (costruttore privato), ma controllata:

* è utile come **placeholder** in casi in cui hai bisogno di un valore di default
  (es. mapping/persistenza, binding, test),
* non viene generata tramite `Create`, per non confondere il concetto di “istanza valida
  secondo il dominio” con quello di “istanza tecnica di comodo”.

Le proprietà:

```csharp
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
public int Length => Value.Length;
```

permettono di:

* verificare rapidamente se il VO rappresenta di fatto un testo vuoto/non inizializzato,
* ottenere la lunghezza del valore interno senza dover accedere direttamente alla stringa.

---

## 4.6 Utilizzo tipico nel dominio

**1. All’interno dell’entità/aggregate `Joke`**

```csharp
public class Joke
{
    public QuestionText Question { get; private set; }

    public void UpdateQuestion(string? newQuestion)
    {
        Question = QuestionText.Create(newQuestion);
    }
}
```

In questo modo:

* l’entità non si occupa direttamente della validazione del testo,
* tutta la logica di validazione è concentrata nel Value Object.

**2. Mapping da DTO / comando di input**

```csharp
public async Task CreateJokeAsync(CreateJokeRequest request)
{
    var question = QuestionText.Create(request.Question);
    var answer = AnswerText.Create(request.Answer);

    var joke = new Joke(question, answer, currentUserId);
    // ...
}
```

L’Application Layer delega ai Value Object la responsabilità di validare i dati;
se qualcosa non va, viene lanciata `DomainValidationException` e il caso d’uso
può tradurre l’errore in una risposta HTTP adeguata (tipicamente 400).

---

## 4.7 Coerenza con DDD, Clean Architecture e SOLID

`QuestionText` è pienamente in linea con i principi base della tua architettura:

* **DDD**

  * il testo della domanda è modellato come concetto esplicito (Value Object),
    non come primitiva “anonima” (`string`);
  * le regole di validazione sono parte del modello, non della UI o di un layer generico.

* **Clean Architecture**

  * vive nel Domain Layer, non dipende da infrastruttura o framework;
  * utilizza solo tipi di dominio (`DomainValidationException`, `JokeErrorMessages`),
    senza contaminazioni tecniche.

* **SOLID (SRP)**

  * `QuestionText` ha una singola responsabilità: rappresentare e validare
    il testo della domanda secondo le regole del dominio;
  * non gestisce persistenza, log, mapping, protocolli: fa una cosa sola e la fa bene.

---

## 4.8 Linee guida per evoluzioni future

Se in futuro il dominio introducesse nuove regole sul testo della domanda
(es. divieti di certe parole, pattern particolari, localizzazioni), queste
dovrebbero essere aggiunte all’interno del metodo `Create` o di eventuali
metodi ausiliari privati, mantenendo intatto il contratto:

> “Ogni istanza di `QuestionText` creata tramite `Create` rispetta gli invarianti
> del dominio per quanto riguarda il testo della domanda.”

In questo modo, `QuestionText` rimane il **punto unico di verità** per tutte le
regole che riguardano la question nel Domain Layer.

---

