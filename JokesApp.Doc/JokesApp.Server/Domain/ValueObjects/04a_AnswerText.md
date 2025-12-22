# 📘 **04a_AnswerText.md**

### *Value Object per il testo della risposta*

---

## 4.9 Ruolo nel dominio

Nel sottodominio **Joke**, oltre al testo della domanda (`QuestionText`), anche il testo della
risposta è un elemento centrale del modello. Come per la question, non si tratta di una semplice
`string`, ma di un concetto dotato di regole proprie:

- non può essere nullo o vuoto,
- non può superare una lunghezza massima definita dal dominio,
- viene normalizzato (trim degli spazi).

Per evitare che queste regole vengano duplicate o applicate in modo incoerente in più punti
(entità, servizi, controller), la risposta viene modellata come **Value Object** dedicato:
`AnswerText`.

`AnswerText`:

- incapsula il valore testuale della risposta e le **regole locali di validazione**,
- garantisce che ogni istanza valida rispetti gli **invarianti definiti dal dominio**,
- è immutabile e confrontabile per valore (`record` sigillato),
- viene utilizzato dall’entità/aggregate `Joke` al posto di una semplice `string`.

In questo modo, tutto il codice che lavora sulle joke interagisce con un tipo che esprime
esplicitamente il concetto di “testo risposta valido nel dominio”.

---

## 4.10 Definizione della classe

```csharp
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta il testo della risposta di una barzelletta.
    /// È immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record AnswerText
    {
        #region Constants

        /// <summary>
        /// Lunghezza massima consentita dal dominio per la risposta.
        /// </summary>
        public const int MaxLength = 500;

        #endregion

        #region Properties

        /// <summary>
        /// Valore testuale interno della risposta.
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
        /// Costruttore privato per garantire l’immutabilità
        /// e la validazione centralizzata tramite <see cref="Create(string?)"/>.
        /// </summary>
        /// <param name="value">Testo della risposta già validato.</param>
        private AnswerText(string value)
        {
            Value = value;
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method che valida e crea un nuovo <see cref="AnswerText"/>
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">Testo della risposta da validare.</param>
        /// <returns>Un'istanza valida di <see cref="AnswerText"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata quando il valore è nullo, vuoto o supera la lunghezza massima consentita.
        /// </exception>
        public static AnswerText Create(string? value)
        {
            if (value is null)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(AnswerText));
            }

            // Normalize input by trimming leading/trailing whitespace.
            var trimmed = value.Trim();

            if (trimmed.Length == 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(AnswerText));
            }

            if (trimmed.Length > MaxLength)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerTooLong,
                    nameof(AnswerText));
            }

            // At this point the value is valid according to the domain rules.
            return new AnswerText(trimmed);
        }

        #endregion

        #region Static members

        /// <summary>
        /// Istanza vuota, utile per EF Core, test o scenari di binding iniziale.
        /// </summary>
        public static AnswerText Empty { get; } = new AnswerText(string.Empty);

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

Punti chiave:

* `sealed record` → Value Object immutabile con equality per valore;
* `MaxLength = 500` → vincolo esplicito sulla lunghezza massima consentita;
* metodo statico `Create` come **unica porta di ingresso** per creare istanze valide;
* uso di `DomainValidationException` e `JokeErrorMessages` (`AnswerNullOrEmpty`, `AnswerTooLong`);
* membro statico `Empty` come sentinel controllata;
* proprietà di utilità `IsEmpty` e `Length` per controlli veloci.

---

## 4.11 Invarianti e regole di validazione

Il metodo `Create` implementa gli invarianti di dominio per il testo della risposta:

1. **Non null**

   ```csharp
   if (value is null)
   {
       throw new DomainValidationException(
           JokeErrorMessages.AnswerNullOrEmpty,
           nameof(AnswerText));
   }
   ```

   La risposta non può essere “non definita”: chi costruisce il Value Object è obbligato
   a fornire un valore.

2. **Trimming e non vuoto**

   ```csharp
   var trimmed = value.Trim();

   if (trimmed.Length == 0)
   {
       throw new DomainValidationException(
           JokeErrorMessages.AnswerNullOrEmpty,
           nameof(AnswerText));
   }
   ```

   L’input viene normalizzato rimuovendo spazi iniziali e finali. Una stringa composta
   solo da spazi viene considerata vuota e quindi non valida.

3. **Lunghezza massima**

   ```csharp
   if (trimmed.Length > MaxLength)
   {
       throw new DomainValidationException(
           JokeErrorMessages.AnswerTooLong,
           nameof(AnswerText));
   }
   ```

   Viene imposto un limite di lunghezza (`MaxLength = 500`), pensato per:

   * evitare risposte eccessivamente verbose,
   * garantire compatibilità con vincoli di persistenza e UI,
   * mantenere le joke concise e fruibili.

In caso di violazione, viene lanciata `DomainValidationException` con:

* un messaggio proveniente da `JokeErrorMessages`,
* il `MemberName` impostato a `nameof(AnswerText)`, così che la parte applicativa
  possa sapere quale Value Object ha fallito la validazione.

---

## 4.12 Immutabilità e semantica di Value Object

La scelta di modellare `AnswerText` come:

```csharp
public sealed record AnswerText
```

implica che:

* l’istanza è **immutabile**: il testo non può essere modificato dopo la creazione;
* l’uguaglianza (`Equals`, comparazioni, uso in collezioni) è basata sul **valore** (`Value`),
  e non sull’identità dell’istanza;
* `sealed` impedisce ulteriori derivazioni, mantenendo il concetto atomico e semplice.

Questa semantica è perfettamente allineata al concetto di Value Object nel DDD:
`AnswerText` rappresenta un valore, non un’entità con identità propria.

---

## 4.13 Membro statico `Empty` e proprietà di utilità

`AnswerText` espone:

```csharp
public static AnswerText Empty { get; } = new AnswerText(string.Empty);
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
public int Length => Value.Length;
```

Questi membri servono a:

* avere una **istanza vuota controllata** (`Empty`), utile in scenari tecnici
  (EF Core, binding, test) dove può essere necessario inizializzare il Value Object
  senza passare dal processo di validazione standard;
* verificare facilmente se l’istanza rappresenta un valore vuoto/non inizializzato
  (`IsEmpty`), senza doversi ricordare la forma interna;
* ottenere rapidamente la lunghezza del testo (`Length`) per eventuali logiche
  o controlli aggiuntivi.

È importante notare che:

* il “percorso normale” di creazione di un `AnswerText` **resta `Create`**,
* `Empty` è un’eccezione tecnica controllata, non un valore di dominio “valido”
  per una risposta effettivamente impostata dall’utente.

---

## 4.14 Utilizzo tipico nel dominio

**1. All’interno dell’entità `Joke`**

```csharp
public class Joke
{
    public AnswerText Answer { get; private set; }

    public void UpdateAnswer(string? newAnswer)
    {
        Answer = AnswerText.Create(newAnswer);
    }
}
```

Qui l’entità delega totalmente al Value Object la validazione del testo.
Se il valore non è valido, viene lanciata una `DomainValidationException`
e il chiamante (Application/API) decide come gestire l’errore.

**2. Mapping da DTO / comando**

```csharp
public async Task CreateJokeAsync(CreateJokeRequest request)
{
    var question = QuestionText.Create(request.Question);
    var answer   = AnswerText.Create(request.Answer);

    var joke = new Joke(question, answer, currentUserId);
    // ...
}
```

Il caso d’uso non si occupa delle regole di lunghezza/null/empty: si affida ai Value Object
per garantire la correttezza dei dati. Se qualcosa va storto, viene lanciata
`DomainValidationException` con i messaggi provenienti da `JokeErrorMessages`.

---

## 4.15 Coerenza con DDD, Clean Architecture e SOLID

`AnswerText` è perfettamente coerente con i principi dell’architettura:

* **DDD**

  * modella un concetto di dominio (testo della risposta) come Value Object dedicato;
  * rende espliciti gli invarianti associati (non null, non vuoto, lunghezza massima).

* **Clean Architecture**

  * vive nel Domain Layer, senza dipendenze tecniche;
  * utilizza solo tipi di dominio (`DomainValidationException`, `JokeErrorMessages`).

* **SOLID (SRP)**

  * ha una sola responsabilità: rappresentare e validare il testo della risposta secondo
    le regole del dominio;
  * non gestisce persistenza, log, mapping verso il client, ecc.

---

## 4.16 Relazione con `QuestionText` e regole più avanzate

`AnswerText` è progettato in parallelo con `QuestionText`:

* stessi pattern (record immutabile, factory `Create`, `Empty` statico),
* stesse modalità di validazione (null/empty, trimming, lunghezza, DomainValidationException),
* uso di `JokeErrorMessages` per i messaggi di errore.

Eventuali regole di dominio più avanzate che coinvolgono **sia** domanda che risposta
(ad esempio: “Question e Answer non possono essere identiche”) sono gestite a livello
di entità/aggregate `Joke`, utilizzando il messaggio:

* `JokeErrorMessages.QuestionAndAnswerCannotMatch`

e, tipicamente, una `DomainValidationException` o `DomainOperationException`.

In questo modo:

* i Value Object restano focalizzati sulle **regole locali** dei rispettivi campi,
* le regole di relazione tra campi diversi sono demandate al livello opportuno
  (entità/aggregate), mantenendo il design pulito e manutenibile.

---

