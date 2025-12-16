# 📘 **01_DomainValidationException.md** 

### *Errori di validazione nel dominio*

---

## 1.23 Ruolo nel contesto del Domain Layer

Nel Domain Layer, una parte essenziale del modello è la **validazione dei dati** rispetto alle
regole di business e agli **invarianti di dominio**. Non si tratta solo di controlli sintattici
(es. “stringa non vuota”), ma di vincoli che descrivono cosa è *ammissibile* nel mondo del dominio.

`DomainValidationException` è l’eccezione dedicata a rappresentare tutti i casi in cui:

> “I dati forniti non rispettano le regole di validazione definite dal dominio.”

È largamente usata da:

- **Value Object** (ad es. `QuestionText`, `AnswerText`, `JokeId`, `UserId`, …),
- metodi di **creazione/factory** nel dominio,
- eventuali servizi di dominio che applicano invarianti complessi.

Lo scopo è separare in modo chiaro:

- gli errori di validazione (dati non accettabili per il dominio),
- dagli errori di operazione (operazioni non consentite nello stato corrente, gestiti da `DomainOperationException`),
- dagli errori di autorizzazione (operazioni non consentite al soggetto, gestiti da `UnauthorizedDomainOperationException`).

---

## 1.24 Definizione della classe

```csharp
using System;

namespace JokesApp.Server.Domain.Exceptions
{
    /// <summary>
    /// Eccezione generata quando i dati di input non rispettano
    /// le regole di validazione definite nel dominio.
    /// </summary>
    public class DomainValidationException : DomainException
    {
        /// <summary>
        /// Nome logico del membro (proprietà, Value Object o campo) che ha
        /// causato la violazione di validazione. Può essere nullo.
        /// </summary>
        public string? MemberName { get; }

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> con un messaggio descrittivo.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        public DomainValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> indicando anche
        /// il membro che ha causato la violazione di validazione.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="memberName">Nome del membro (es. "QuestionText", "AnswerText").</param>
        public DomainValidationException(string message, string? memberName)
            : base(message)
        {
            MemberName = memberName;
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> con un messaggio descrittivo
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public DomainValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Crea una nuova istanza di <see cref="DomainValidationException"/> indicando il membro coinvolto
        /// e una eccezione interna che ha causato l'errore corrente.
        /// </summary>
        /// <param name="message">Messaggio descrittivo dell'errore di validazione.</param>
        /// <param name="memberName">Nome del membro (es. "QuestionText", "AnswerText").</param>
        /// <param name="innerException">Eccezione che ha causato l'errore corrente.</param>
        public DomainValidationException(string message, string? memberName, Exception innerException)
            : base(message, innerException)
        {
            MemberName = memberName;
        }

        #endregion
    }
}
```

---

## 1.25 Obiettivi progettuali di `DomainValidationException`

La progettazione di `DomainValidationException` risponde a una serie di obiettivi precisi:

1. **Modellare esplicitamente gli errori di validazione di dominio**

   Non tutti gli errori sono uguali: qui parliamo in modo esplicito di:

   * valori nulli o vuoti dove il dominio li vieta,
   * lunghezze massime/minime violate,
   * formati non validi (email, identificativi, ecc.),
   * combinazioni di dati che non rispettano regole di coerenza interna.

   Usare una eccezione dedicata permette di distinguere questi casi da altri
   errori di business (operazioni non valide) e da errori infrastrutturali.

2. **Associare, quando serve, l’errore al “membro” che ha fallito**

   La proprietà opzionale `MemberName` permette di indicare **quale** elemento
   del modello ha causato la violazione:

   * una proprietà (`"QuestionText"`),
   * un Value Object (`"AnswerText"`),
   * un campo specifico (`"UserId"`, `"Email"`, ecc.).

   Questo rende più semplice:

   * loggare errori in modo leggibile,
   * costruire risposte strutturate verso il client (es. JSON con `field` e `message`),
   * aggregare più errori di validazione, se in futuro adotterai una strategia
     di “validation summary”.

3. **Fornire i costruttori più comuni per scenari reali**

   Grazie ai diversi costruttori disponibili, `DomainValidationException` è
   utilizzabile in più scenari:

   * solo messaggio,
   * messaggio + nome del membro,
   * messaggio + inner exception,
   * messaggio + nome del membro + inner exception.

   In questo modo puoi:

   * segnalare semplici errori di dominio,
   * wrappare eccezioni interne (se derivano da logiche di validazione complesse)
     senza perdere la stack trace originale.

---

## 1.26 Differenza rispetto a `DomainOperationException` e `UnauthorizedDomainOperationException`

Per evitare ambiguità d’uso nel codice, è importante tenere distinta la semantica:

* **`DomainValidationException`**

  * indica che *il dato in sé* non è valido rispetto alle regole del dominio;
  * tipicamente lanciata da Value Objects e factory quando un valore non supera
    i controlli di validazione (es. testo troppo lungo, formato errato, null/empty).

* **`DomainOperationException`**

  * indica che l’operazione richiesta **non è compatibile con lo stato corrente**
    dell’aggregato o del modello (es. like duplicato, transizione di stato non prevista).

* **`UnauthorizedDomainOperationException`**

  * indica che l’operazione sarebbe astrattamente valida, ma **il soggetto che la
    richiede non ha i permessi** necessari (regole di autorizzazione nel dominio).

In breve:

* *“Questo valore non è ammissibile nel dominio”* → `DomainValidationException`.
* *“Questa operazione non è ammessa in questo stato”* → `DomainOperationException`.
* *“Tu non hai i permessi per fare questa operazione”* → `UnauthorizedDomainOperationException`.

---

## 1.27 Esempi pratici d’uso nei Value Object

Un caso tipico è la creazione di un Value Object come `AnswerText`:

```csharp
public sealed class AnswerText
{
    public string Value { get; }

    private AnswerText(string value)
    {
        Value = value;
    }

    public static AnswerText Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Domain validation: AnswerText cannot be null or empty.
            throw new DomainValidationException(
                "Answer text cannot be null or empty.",
                nameof(AnswerText)
            );
        }

        if (value.Length > 500)
        {
            // Domain validation: AnswerText too long.
            throw new DomainValidationException(
                "Answer text is too long.",
                nameof(AnswerText)
            );
        }

        return new AnswerText(value);
    }
}
```

In una fase successiva, al posto delle stringhe “hard-coded”, potrai utilizzare i
messaggi centralizzati (es. `JokeErrorMessages.AnswerTextCannotBeNullOrEmpty`) mantenendo
la stessa semantica e la stessa struttura.

---

## 1.28 Coerenza con DDD, Clean Architecture e SOLID

`DomainValidationException` è coerente con i principi architetturali e di design
utilizzati nel progetto:

* **DDD**

  * La validazione non è trattata come un dettaglio tecnico, ma come parte integrante
    del modello concettuale.
  * Gli errori di validazione fanno parte del linguaggio del dominio e sono espressi
    con un tipo dedicato.

* **Clean Architecture**

  * La classe vive nel Domain Layer e non dipende da framework, protocolli o tecnologie.
  * La traduzione in risposte HTTP (es. 400 Bad Request con lista di errori di campo)
    avviene a livello di Application/API.

* **SOLID (SRP)**

  * Ha una responsabilità unica: rappresentare un errore di validazione di dominio.
  * Non effettua la validazione, non logga, non si occupa di mapping verso client:
    rappresenta solamente *il fatto* che una regola è stata violata.

---

## 1.29 Linee guida per l’uso e l’estensione

Per mantenere il codice coerente:

* utilizza `DomainValidationException` ogni volta che **un valore/parametro di dominio (anche se proveniente da un DTO/Command)** non rispetta le regole di validazione del dominio; in pratica, durante il mapping verso i Value Object (`QuestionText.Create(...)`, ecc.);
* valorizza `MemberName` quando l’errore è chiaramente legato a una singola
  proprietà/Value Object (es. `nameof(AnswerText)`), così da poter sfruttare
  meglio logging e mapping verso il frontend;
* valuta la creazione di sottoclassi specifiche solo se portano un reale beneficio
  in termini di espressività e manutenibilità (altrimenti, la classe base è più che
  sufficiente).

In questo modo, `DomainValidationException` diventa il punto di riferimento unico
per tutti gli errori di validazione nel Domain Layer, e i Value Objects possono
appoggiarsi a essa in maniera consistente e prevedibile.

---