# 📘 **03_JokeErrorMessages.md**

### *Messaggi di errore del dominio Joke*

---

## 3.1 Ruolo nel contesto del Domain Layer

Il dominio di **Joke** è caratterizzato da diverse regole di validazione e di coerenza interna:

- vincoli sui testi di domanda e risposta (Question/Answer),
- vincoli sull’identità dell’autore associato alla joke,
- vincoli sull’identificativo della joke (JokeId),
- regole di business su like/unlike, modifiche, coerenza tra campi, ecc.,
- vincoli sui **timestamp** associati agli eventi di dominio (creazione, aggiornamento).

Per evitare di spargere stringhe di errore “magiche”
(`"Question cannot be null or empty"`, `"CreatedAt timestamp is invalid."`, …)
all’interno di Value Object, entità ed eventi, è stato introdotto un **contenitore centralizzato**
di messaggi di errore specifici del sottodominio Joke: `JokeErrorMessages`.

Si tratta di una classe statica nel **Domain Layer**, pensata per:

- mantenere **coerenza** nei messaggi di errore esposti verso l’esterno (log, API, UI),
- garantire la **centralizzazione** delle stringhe, così da poterle modificare in un solo punto,
- rendere esplicito, in modo leggibile, il catalogo delle regole di validazione e delle violazioni
  possibili nel sottodominio Joke.

Questi messaggi vengono utilizzati da:

- **Value Object** (es. `QuestionText`, `AnswerText`, `JokeId`),
- **entità / aggregate** (es. `Joke`),
- **domain events** (es. `JokeWasCreated`, `JokeWasUpdated`),
- eventuali **domain services** che applicano regole di business sul mondo delle joke.

---

## 3.2 Definizione della classe

```csharp
using System;

namespace JokesApp.Server.Domain.Errors
{
    /// <summary>
    /// Contiene tutti i messaggi di errore relativi al dominio Joke.
    /// Questi messaggi vengono utilizzati da entità, Value Object ed eventi
    /// per mantenere consistenza e centralità delle regole di validazione.
    /// </summary>
    public static class JokeErrorMessages
    {
        #region Question errors

        /// <summary>
        /// Messaggio per indicare che la domanda è nulla o vuota.
        /// </summary>
        public const string QuestionNullOrEmpty =
            "Question cannot be null or empty.";

        /// <summary>
        /// Messaggio per indicare che la domanda supera la lunghezza massima consentita.
        /// </summary>
        public const string QuestionTooLong =
            "Question exceeds maximum allowed length.";

        #endregion

        #region Answer errors

        /// <summary>
        /// Messaggio per indicare che la risposta è nulla o vuota.
        /// </summary>
        public const string AnswerNullOrEmpty =
            "Answer cannot be null or empty.";

        /// <summary>
        /// Messaggio per indicare che la risposta supera la lunghezza massima consentita.
        /// </summary>
        public const string AnswerTooLong =
            "Answer exceeds maximum allowed length.";

        #endregion

        #region Author errors

        /// <summary>
        /// Messaggio per indicare che l'autore non può essere nullo.
        /// </summary>
        public const string AuthorNull =
            "The author cannot be null.";

        /// <summary>
        /// Messaggio per indicare che l'autore è già stato assegnato.
        /// </summary>
        public const string AuthorAlreadySet =
            "The author has already been assigned.";

        /// <summary>
        /// Messaggio per indicare che l'AuthorId non corrisponde allo UserId della joke.
        /// </summary>
        public const string AuthorIdMismatch =
            "AuthorId does not match the Joke's UserId.";

        #endregion

        #region JokeId errors (Value Object)

        /// <summary>
        /// Messaggio per indicare che il JokeId è invalido.
        /// Nota: nel Domain puro, con JokeId basato su Guid, l'unico caso "non valido"
        /// è tipicamente il valore vuoto (<see cref="Guid.Empty"/>) associato al messaggio <c>JokeIdEmpty</c>.
        /// Questo messaggio è utile soprattutto in fase di conversione/parsing di input esterni (fuori Domain).
        /// </summary>
        public const string JokeIdInvalid =
            "JokeId is invalid.";

        /// <summary>
        /// Messaggio per indicare che il JokeId è vuoto o non valorizzato (Guid.Empty).
        /// </summary>
        public const string JokeIdEmpty =
            "JokeId cannot be empty.";

        #endregion

        #region Domain rule violations

        /// <summary>
        /// Messaggio per indicare che domanda e risposta non possono essere identiche.
        /// </summary>
        public const string QuestionAndAnswerCannotMatch =
            "Question and Answer cannot be identical.";

        /// <summary>
        /// Messaggio per indicare che è stato raggiunto il numero massimo di like.
        /// </summary>
        public const string MaximumLikeOfJokeReached =
            "Maximum number of likes has been reached.";

        /// <summary>
        /// Messaggio per indicare che il numero di like non può scendere sotto zero.
        /// </summary>
        public const string MinimumLikeOfJokeReached =
            "Like count cannot go below zero.";

        /// <summary>
        /// Messaggio per indicare che l'utente non è autorizzato ad aggiornare la joke.
        /// </summary>
        public const string UpdateNotAllowed =
            "The user is not authorized to update this joke.";

        #endregion

        #region Generic

        /// <summary>
        /// Messaggio generico per indicare che un valore obbligatorio è mancante.
        /// </summary>
        public const string ValueRequired =
            "A required value was missing.";

        #endregion

        #region Domain Event Errors

        /// <summary>
        /// Messaggio per indicare che il timestamp di creazione della barzelletta
        /// non è valido o non impostato correttamente.
        /// </summary>
        public const string JokeCreatedAtInvalid =
            "CreatedAt timestamp is invalid.";

        /// <summary>
        /// Messaggio per indicare che il timestamp di aggiornamento della barzelletta
        /// non è valido o non impostato correttamente.
        /// </summary>
        public const string JokeUpdatedAtInvalid =
            "UpdatedAt timestamp is invalid.";

        #endregion
    }
}
```

Elementi salienti:

* **namespace**: `JokesApp.Server.Domain.Errors` → appartenenza esplicita al Domain Layer;
* **classe `static`** → puro contenitore di costanti, senza stato né istanze;
* **costanti `const string`** → messaggi immutabili e riusabili ovunque nel dominio;
* **XML doc in italiano** + messaggi in inglese → commenti pensati per sviluppatori italiani,
  ma errori esposti in una lingua standard per client, log e API.

---

## 3.3 Obiettivi progettuali di `JokeErrorMessages`

L’introduzione di `JokeErrorMessages` risponde a diversi obiettivi:

1. **Centralizzare i messaggi di errore del sottodominio Joke**

   Invece di avere frammenti di stringa duplicati in Value Object, entità e servizi,
   tutti i messaggi vengono definiti una sola volta. Questo garantisce:

   * coerenza terminologica (*sempre lo stesso testo per lo stesso errore*),
   * manutenibilità (modifica in un solo punto),
   * facilità di eventuale internazionalizzazione in futuro.

2. **Rendere esplicito il “catalogo” delle regole di validazione**

   Leggendo la classe è immediatamente evidente:

   * quali vincoli esistono su Question/Answer,
   * quali vincoli esistono sull’autore (Author) della joke,
   * come è inteso JokeId nel dominio,
   * quali regole di business governano like, min/max, update, coerenza tra campi,
   * quali vincoli esistono sui timestamp veicolati dai Domain Events (CreatedAt/UpdatedAt).


   Questo file diventa quasi una **mappa testuale** delle regole di dominio per la joke.

3. **Separare messaggi di dominio da messaggi tecnici**

   A differenza dei messaggi di startup (`StartupErrorMessages` nel Data Layer),
   questi messaggi:

   * appartengono al dominio,
   * descrivono regole di business e invarianti,
   * sono pensati per essere usati da `DomainValidationException`,
     `DomainOperationException` e `UnauthorizedDomainOperationException`, non da errori tecnici.

---

## 3.4 Organizzazione per sezioni (`#region`)

La classe è organizzata in blocchi logici:

* **Question errors**

  * `QuestionNullOrEmpty`
  * `QuestionTooLong`

* **Answer errors**

  * `AnswerNullOrEmpty`
  * `AnswerTooLong`

* **Author errors**

  * `AuthorNull`
  * `AuthorAlreadySet`
  * `AuthorIdMismatch`

* **JokeId errors (Value Object)**

  * `JokeIdInvalid`
  * `JokeIdEmpty`

* **Domain rule violations**

  * `QuestionAndAnswerCannotMatch`
  * `MaximumLikeOfJokeReached`
  * `MinimumLikeOfJokeReached`
  * `UpdateNotAllowed`

* **Generic**
  * `ValueRequired`

* **Domain Event Errors**
  * `JokeCreatedAtInvalid`
  * `JokeUpdatedAtInvalid`

Questa suddivisione per region:

* migliora la leggibilità del file,
* rende semplice l’aggiunta futura di nuovi messaggi nella sezione corretta,
* aiuta a ragionare per “cluster” di regole (testo, autore, identificativo, regole di business).

---

## 3.5 Esempi di utilizzo nel dominio

**1. Value Object di testo (`QuestionText`, `AnswerText`)**

```csharp
public static QuestionText Create(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new DomainValidationException(
            JokeErrorMessages.QuestionNullOrEmpty);
    }

    var trimmed = value.Trim();

    if (trimmed.Length > QuestionText.MaxLength)
    {
        throw new DomainValidationException(
            JokeErrorMessages.QuestionTooLong);
    }

    return new QuestionText(trimmed);
}
```

Stesso pattern vale per `AnswerText`, utilizzando `AnswerNullOrEmpty` e `AnswerTooLong`.

**2. Regole sull’autore**

```csharp
public void SetAuthor(ApplicationUser author)
{
    if (Author is not null)
    {
        throw new DomainOperationException(JokeErrorMessages.AuthorAlreadySet);
    }

    if (author is null)
    {
        throw new DomainValidationException(JokeErrorMessages.AuthorNull);
    }

    if (author.Id != ApplicationUserId.Value)
    {
        throw new DomainOperationException(JokeErrorMessages.AuthorIdMismatch);
    }

    Author = author;
}
```

**3. Regole su JokeId (Value Object)**

```csharp
public static JokeId Create(string? value)
{
    if (!Guid.TryParse(value, out var guid))
    {
        throw new DomainValidationException(
            JokeErrorMessages.JokeIdInvalid);
    }

    if (guid == Guid.Empty)
    {
        throw new DomainValidationException(
            JokeErrorMessages.JokeIdEmpty);
    }

    return new JokeId(guid);
}

```

**4. Violazioni di regole di business (like/unlike, update)**

```csharp
public void AddLike()
{
    if (Likes == int.MaxValue)
    {
        throw new DomainOperationException(
            JokeErrorMessages.MaximumLikeOfJokeReached);
    }

    Likes++;

    AddDomainEvent(new JokeWasLiked(
        Id,
        Likes));
}

public void RemoveLike()
{
    if (Likes == 0)
    {
        throw new DomainOperationException(
            JokeErrorMessages.MinimumLikeOfJokeReached);
    }

    Likes--;

    AddDomainEvent(new JokeWasUnliked(
        Id,
        Likes));
}

public void Update(UserId userId, QuestionText question, AnswerText answer)
{
    if (!IsAuthoredBy(userId))
    {
        throw new UnauthorizedDomainOperationException(
            JokeErrorMessages.UpdateNotAllowed);
    }

    EnsureQuestionAndAnswerAreDifferent(question, answer);

    Question = question;
    Answer = answer;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new JokeWasUpdated(
        Id,
        Question,
        Answer,
        UpdatedAt.Value));
}
```

---

## 3.6 Coerenza con DDD, Clean Architecture e SOLID

`JokeErrorMessages` è coerente con i principi che guidano l’intero progetto:

* **DDD**

  * I messaggi rispecchiano il linguaggio del dominio (Question, Answer, JokeId, Author,
    like, autorizzazioni, ecc.).
  * Le regole sono esplicitate in modo chiaro, favorendo la comprensione del modello anche
    solo leggendo questo file.

* **Clean Architecture**

  * La classe vive nel Domain Layer e non ha dipendenze tecniche.
  * È utilizzata da eccezioni di dominio e oggetti di dominio, ma non conosce HTTP,
    database o framework specifici.

* **SOLID (SRP)**

  * `JokeErrorMessages` ha una responsabilità singola: centralizzare le stringhe
    di errore del dominio Joke.
  * Non contiene logica, non valida, non lancia eccezioni da sola: fornisce solo
    i messaggi per chi deve farlo.

---

## 3.7 Linee guida per l’estensione futura

Nel momento in cui il dominio di Joke si arricchirà di nuove regole, l’estensione di
`JokeErrorMessages` seguirà poche regole semplici:

* aggiungere nuove costanti nella **region corretta**
  (Question/Answer/Author/JokeId/Domain rules/Generic/Domain Event Errors),

* mantenere la **nomenclatura coerente**:

  * prefissi significativi (`Question*`, `Answer*`, …),
  * messaggi chiari e specifici,
* usare sistematicamente questi messaggi da:

  * `DomainValidationException`,
  * `DomainOperationException`,
  * `UnauthorizedDomainOperationException`,
  * evitando stringhe hard-coded altrove.

In questo modo, `JokeErrorMessages` rimane il **punto di riferimento unico** per
tutte le violazioni di regole nel sottodominio Joke, e il Domain Layer conserva
ordine, coerenza e manutenibilità anche man mano che il modello cresce.

---
