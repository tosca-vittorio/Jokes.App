# 📘 **04b_EmailAddress.md**

### *Value Object per l’indirizzo email dell’utente*

---

## 4.9 Ruolo nel dominio

`EmailAddress` rappresenta l’**indirizzo email** di un utente all’interno del dominio
(ApplicationUser).

Anche se tecnicamente l’email è una `string`, nel modello di dominio non viene trattata come
un tipo primitivo generico, ma come un **Value Object specializzato** che:

- incapsula tutte le regole di validazione (obbligatorietà, lunghezza, formato),
- garantisce che un indirizzo email sia **sempre valido** quando entra nel Domain Layer,
- rende le API di dominio più espressive (`EmailAddress` al posto di `string`),
- mantiene allineata la logica del dominio a quella utilizzata a livello di DTO
  (es. `CustomEmailAttribute`).

`EmailAddress` è usato dall’entità `ApplicationUser` per rappresentare l’email dell’utente
in modo consistente e tipizzato.

---

## 4.10 Definizione della classe

```csharp
using System.Text.RegularExpressions;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta l'indirizzo email di un utente.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record EmailAddress
    {
        /// <summary>
        /// Lunghezza massima consentita per l'indirizzo email.
        /// </summary>
        public const int MaxLength = 256;

        /// <summary>
        /// Espressione regolare per la validazione dell'indirizzo email.
        /// Allinea la logica del dominio a quella dell'attributo CustomEmailAttribute.
        /// </summary>
        private static readonly Regex EmailRegex = new(
            // Consente lettere, numeri, punti, trattini, underscore nella local part,
            // dominio con label separate da punto e TLD di almeno 2 caratteri.
            @"^[A-Za-z0-9._%+-]+@([A-Za-z0-9]+(-[A-Za-z0-9]+)*\.)+[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Valore testuale interno dell'indirizzo email.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Indica se il valore rappresenta uno stato vuoto o non inizializzato.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        /// <summary>
        /// Restituisce la lunghezza del testo interno.
        /// </summary>
        public int Length => Value.Length;

        /// <summary>
        /// Costruttore privato per garantire l'immutabilità
        /// e la validazione centralizzata tramite <see cref="Create"/>.
        /// </summary>
        /// <param name="value">Valore testuale già validato.</param>
        private EmailAddress(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Factory method che valida e crea un nuovo Value Object
        /// conforme alle regole del dominio ApplicationUser.
        /// </summary>
        /// <param name="value">Stringa contenente l'indirizzo email.</param>
        /// <returns>Un'istanza valida di <see cref="EmailAddress"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata se il valore è nullo, vuoto, troppo lungo o non rispetta il formato email.
        /// </exception>
        public static EmailAddress Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Email is required at domain level.
                throw new DomainValidationException(ApplicationUserErrorMessages.EmailRequired);
            }

            // Normalize input by trimming leading/trailing whitespace.
            string v = value.Trim();

            if (v.Length > MaxLength)
            {
                // Email exceeds maximum allowed length.
                throw new DomainValidationException(ApplicationUserErrorMessages.EmailTooLong);
            }

            if (!EmailRegex.IsMatch(v))
            {
                // Email format is invalid.
                throw new DomainValidationException(ApplicationUserErrorMessages.EmailInvalid);
            }

            return new EmailAddress(v);
        }

        /// <summary>
        /// Istanza vuota, utile per scenari di default, EF Core o binding iniziale.
        /// </summary>
        public static EmailAddress Empty { get; } = new EmailAddress(string.Empty);

        /// <summary>
        /// Restituisce il valore testuale dell'indirizzo email.
        /// </summary>
        public override string ToString() => Value;
    }
}
```

Punti chiave:

* `sealed record` → Value Object immutabile con equality per valore;
* `MaxLength = 256` → limite esplicito per la lunghezza dell’email;
* `EmailRegex` → regex compilata, case-insensitive, allineata alla logica di `CustomEmailAttribute`;
* `Create(string?)` → unica entry point per creare istanze valide;
* uso di `ApplicationUserErrorMessages.EmailRequired`, `EmailTooLong`, `EmailInvalid`
  combinati con `DomainValidationException`;
* `Empty` come istanza speciale per scenari tecnici.

---

## 4.11 Invarianti e regole di validazione

`EmailAddress` impone una serie di invarianti che devono essere sempre veri
per qualsiasi istanza creata tramite `Create`:

1. **Email obbligatoria a livello di dominio**

   ```csharp
   if (string.IsNullOrWhiteSpace(value))
   {
       throw new DomainValidationException(ApplicationUserErrorMessages.EmailRequired);
   }
   ```

   Nel Domain Layer l’email dell’utente è considerata **obbligatoria**:

   * `null` → non ammesso,
   * stringa vuota o solo spazi → non ammesso.

   Questo è coerente con l’idea che un `ApplicationUser` “valido” abbia sempre un’email definita.

2. **Normalizzazione (trim)**

   ```csharp
   string v = value.Trim();
   ```

   L’input viene normalizzato rimuovendo spazi iniziali e finali.
   In questo modo:

   * l’utente non viene penalizzato per spazi accidentali,
   * la validazione lavora sul valore effettivo e non su artefatti.

3. **Lunghezza massima**

   ```csharp
   if (v.Length > MaxLength)
   {
       throw new DomainValidationException(ApplicationUserErrorMessages.EmailTooLong);
   }
   ```

   Il dominio impone un limite di `MaxLength = 256` caratteri per l’email:

   * coerente con vincoli classici di database/Identity,
   * evita stringhe eccessivamente lunghe e sospette.

4. **Formato email (Regex)**

   ```csharp
   if (!EmailRegex.IsMatch(v))
   {
       throw new DomainValidationException(ApplicationUserErrorMessages.EmailInvalid);
   }
   ```

   La regex gestisce:

   * **local-part**: lettere, numeri, caratteri `._%+-`,
   * **dominio**: una o più label (`[A-Za-z0-9]+(-[A-Za-z0-9]+)*`) separate da `.`,
   * **TLD**: almeno 2 caratteri alfabetici.

   La scelta è un compromesso tra:

   * rigore sufficiente per evitare chiari formati errati,
   * semplicità e leggibilità del pattern.

Ogni violazione viene segnalata con una `DomainValidationException` che utilizza
messaggi centralizzati in `ApplicationUserErrorMessages`.

---

## 4.12 Immutabilità e semantica di Value Object

La dichiarazione:

```csharp
public sealed record EmailAddress
```

comporta che:

* il VO è **immutabile**: una volta creato, `Value` non può essere modificato;
* l’uguaglianza è **basata sul valore** (`Value`), non sull’identità dell’istanza;
* `sealed` indica che non sono previste ulteriori specializzazioni: un’e-mail è un concetto
  atomico nel dominio.

Il costruttore è privato:

```csharp
private EmailAddress(string value) { ... }
```

e ogni istanza passa da:

```csharp
public static EmailAddress Create(string? value) { ... }
```

Questo garantisce che:

* nessun codice del dominio possa “bypassare” la validazione,
* ogni `EmailAddress` creato tramite `Create` rispetti per definizione gli invarianti
  descritti sopra.

---

## 4.13 Membro statico `Empty` e proprietà di utilità

`EmailAddress` mette a disposizione:

```csharp
public static EmailAddress Empty { get; } = new EmailAddress(string.Empty);
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
public int Length => Value.Length;
```

Questi membri servono a:

* avere una **istanza di placeholder** (`Empty`) per scenari tecnici:

  * EF Core,
  * binding iniziale,
  * oggetti in costruzione;

* verificare se un’istanza rappresenta uno stato vuoto/non inizializzato (`IsEmpty`);

* leggere la lunghezza dell’indirizzo email (`Length`) a scopo informativo.

Dal punto di vista semantico:

* il percorso “normale” per valori effettivamente usati nel dominio resta `Create(...)`;
* `Empty` è un’eccezione tecnica controllata, utile quando serve un valore di default
  ma non si vuole usare `null` nel Domain Layer.

---

## 4.14 Relazione con `CustomEmailAttribute` e DTO

Nel progetto esiste anche un attributo di validazione:

* `CustomEmailAttribute`, basato su `System.ComponentModel.DataAnnotations`,
* usato per validare email in DTO / input model / view model.

L’idea architetturale è:

* **EmailAddress** è la **fonte di verità** per la validazione nel **Domain Layer**;
* `CustomEmailAttribute` è un **aiuto per i layer di presentazione/application**,
  e dovrebbe riflettere la stessa logica (stessi pattern, stessi confini).

Differenza di semantica:

* nel **dominio**, l’email è obbligatoria:

  * `EmailAddress.Create(null)` → `EmailRequired`;
  * `EmailAddress.Create("   ")` → `EmailRequired`.

* nei **DTO**, si possono combinare gli attributi:

  ```csharp
  [Required]
  [CustomEmail]
  public string Email { get; set; }
  ```

  dove:

  * `[Required]` gestisce null/vuoto,
  * `[CustomEmail]` si concentra sul **formato**.

In una fase successiva, l’implementazione ideale è far sì che `CustomEmailAttribute`
riusi `EmailAddress.Create` (gestendo l’eccezione) per evitare duplicazioni di logica.

---

## 4.15 Utilizzo tipico in `ApplicationUser`

All’interno dell’entità `ApplicationUser`, `EmailAddress` viene usato al posto di una `string`:

```csharp
public class ApplicationUser
{
    public UserId Id { get; private set; }
    public DisplayName DisplayName { get; private set; }
    public EmailAddress Email { get; private set; }
    // ...

    public void SetEmail(string? email)
    {
        Email = EmailAddress.Create(email);
    }
}
```

In questo scenario:

* l’entità delega completamente al VO la validazione dell’email;
* se l’input non è valido, viene lanciata `DomainValidationException` con messaggi
  presi da `ApplicationUserErrorMessages`;
* il resto del dominio può assumere che `Email` sia **sempre** coerente
  con le regole stabilite.

---

## 4.16 Coerenza con DDD, Clean Architecture e SOLID

`EmailAddress` è in linea con i principi architetturali del progetto:

* **DDD**

  * l’e-mail è modellata come concetto del dominio, non come primitiva;
  * gli invarianti sono incapsulati nel VO, non sparsi tra controller/servizi.

* **Clean Architecture**

  * vive nel **Domain Layer** e dipende solo da BCL + Domain (Errors, Exceptions);
  * non conosce DataAnnotations, HTTP, Identity o DTO.

* **SOLID (SRP)**

  * responsabilità unica: rappresentare e validare l’indirizzo email;
  * non si occupa di persistenza, logging, mapping, interazione con l’utente.

---

## 4.17 Estensioni future

Eventuali evoluzioni sulla gestione delle email (es. supporto per formati internazionali
più complessi, logiche di normalizzazione più avanzate, validazione MX/SMTP lato applicativo)
potranno essere introdotte:

* ampliando la logica interna di `EmailAddress.Create`,
* mantenendo intatta la firma pubblica e il contratto del VO.

Il resto del dominio continuerà a lavorare con `EmailAddress` senza modifiche:

> se `EmailAddress.Create` accetta un valore, quell’indirizzo email è valido per il dominio,
> secondo le regole più aggiornate definite nel modello.

---