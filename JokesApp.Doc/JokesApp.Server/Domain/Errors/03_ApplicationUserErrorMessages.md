# 📘 **03_ApplicationUserErrorMessages.md**

### *Messaggi di errore del dominio ApplicationUser*

---

## 3.8 Ruolo nel contesto del Domain Layer

Il dominio **ApplicationUser** modella l’utente dell’applicazione, con alcune proprietà
che sono particolarmente rilevanti per l’esperienza d’uso e per le regole di business:

- **UserId** → identificatore tipizzato e univoco dell’utente,
- **DisplayName** → nome visualizzato pubblicamente,
- **AvatarUrl** → URL dell’immagine profilo,
- **Email** → identificativo logico principale per autenticazione/comunicazioni.

Ognuna di queste proprietà è soggetta a **regole di validazione di dominio**, ad esempio:

- obbligatorietà di alcuni campi,
- limiti di lunghezza,
- formato corretto (es. email, URL, identificatore utente).

Per evitare che i relativi messaggi di errore siano sparsi e duplicati in entità, Value Object
e servizi, è stata introdotta la classe statica `ApplicationUserErrorMessages`, che:

- centralizza i messaggi di errore legati al dominio `ApplicationUser`,
- garantisce coerenza terminologica in tutto il codice,
- funge da punto unico di modifica in caso di cambiamenti futuri delle regole o dei testi.

Questi messaggi sono tipicamente utilizzati da:

- **Value Object** (`UserId`, `DisplayName`, `EmailAddress`, `AvatarUrl`),
- logica di validazione all’interno dell’entità di dominio `ApplicationUser`,
- lanci di `DomainValidationException` durante la creazione o modifica dei dati utente,
- eventuali servizi di dominio che si occupano di aggiornare il profilo utente.

---

## 3.9 Definizione della classe

```csharp
namespace JokesApp.Server.Domain.Errors
{
    /// <summary>
    /// Contiene tutti i messaggi di errore relativi al dominio ApplicationUser.
    /// Questi messaggi vengono utilizzati da entità e Value Object
    /// per mantenere consistenza nelle regole di validazione.
    /// </summary>
    public static class ApplicationUserErrorMessages
    {
        #region DisplayName errors

        /// <summary>
        /// Messaggio per indicare che il DisplayName supera la lunghezza massima consentita.
        /// </summary>
        public const string DisplayNameMaxLength =
            "DisplayName exceeds maximum length of 50.";

        /// <summary>
        /// Messaggio per indicare che il DisplayName è obbligatorio ma non è stato fornito.
        /// </summary>
        public const string DisplayNameRequired =
            "DisplayName is required.";

        #endregion

        #region AvatarUrl errors

        /// <summary>
        /// Messaggio per indicare che l'AvatarUrl supera la lunghezza massima consentita.
        /// </summary>
        public const string AvatarUrlMaxLength =
            "AvatarUrl exceeds maximum length of 2048.";

        /// <summary>
        /// Messaggio per indicare che l'AvatarUrl non è un URL valido.
        /// </summary>
        public const string AvatarUrlInvalid =
            "AvatarUrl is not a valid URL.";

        #endregion

        #region Email errors

        /// <summary>
        /// Messaggio per indicare che l'email non è in un formato valido.
        /// </summary>
        public const string EmailInvalid =
            "Email is not a valid email address.";

        /// <summary>
        /// Messaggio per indicare che l'email è obbligatoria ma non è stata fornita.
        /// </summary>
        public const string EmailRequired =
            "Email is required.";

        /// <summary>
        /// Messaggio per indicare che l'email supera la lunghezza massima consentita.
        /// </summary>
        public const string EmailTooLong =
            "Email exceeds maximum length of 256.";

        #endregion

        #region UserId errors

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente è nullo o vuoto.
        /// </summary>
        public const string UserIdNullOrEmpty =
            "UserId cannot be null or empty.";

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente contiene caratteri non validi.
        /// (Riservato per eventuali regole future sul formato dell'Id utente.)
        /// </summary>
        public const string UserIdInvalid =
            "UserId contains invalid characters.";

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente supera la lunghezza massima consentita.
        /// </summary>
        public const string UserIdTooLong =
            "UserId exceeds maximum allowed length.";

        #endregion
    }
}
```

Caratteristiche principali:

* **namespace**: `JokesApp.Server.Domain.Errors` → appartiene a pieno titolo al Domain Layer;
* **classe `static`** → puro contenitore di costanti, senza stato né istanziamento;
* **costanti `const string`** → messaggi immutabili, riutilizzabili in tutto il dominio;
* **XML doc in italiano** + messaggi in inglese → commenti pensati per gli sviluppatori, ma output dei messaggi adatto a log e client internazionali.

---

## 3.10 Obiettivi progettuali di `ApplicationUserErrorMessages`

La presenza di `ApplicationUserErrorMessages` nel Domain Layer ha tre obiettivi principali:

1. **Centralizzazione dei messaggi di validazione dell’utente**

   Tutti i casi di input non valido relativi ad `ApplicationUser` vengono descritti da qui:

   * DisplayName mancante o troppo lungo,
   * AvatarUrl troppo lungo o con formato non valido,
   * Email mancante, troppo lunga o non conforme,
   * UserId nullo/vuoto, troppo lungo o (eventualmente) con caratteri non ammessi.

   Invece di avere stringhe ripetute e potenzialmente divergenti, l’intero dominio
   usa sempre gli stessi messaggi, aumentando coerenza e manutenibilità.

2. **Supporto diretto a `DomainValidationException`**

   Quando una regola di validazione fallisce, i Value Object o l’entità `ApplicationUser`
   possono lanciare una `DomainValidationException` usando i messaggi definiti qui.
   Ad esempio:

   ```csharp
   throw new DomainValidationException(
       ApplicationUserErrorMessages.EmailInvalid,
       nameof(Email));
   ```

   oppure nel VO `UserId`:

   ```csharp
   throw new DomainValidationException(
       ApplicationUserErrorMessages.UserIdNullOrEmpty,
       nameof(UserId));
   ```

   Questo consente ai layer esterni (Application/API) di avere errori strutturati
   e consistenti, sia per logging sia per le risposte verso il client.

3. **Separazione dei ruoli tra dominio e infrastruttura**

   `ApplicationUserErrorMessages` vive nel dominio e descrive le regole di validazione
   dei dati utente, senza occuparsi di:

   * protocolli (HTTP),
   * storage (database),
   * errori tecnici (connessioni, I/O, ecc.).

   In questo modo segue lo stesso principio già fatto valere per `JokeErrorMessages`
   e per le `DomainExceptions`: il dominio parla il suo linguaggio, l’infrastruttura
   si occupa dei dettagli tecnici.

---

## 3.11 Organizzazione per sezioni (`#region`)

La classe è organizzata per gruppi logici di proprietà, tramite `#region`:

* **DisplayName errors**

  * `DisplayNameMaxLength` → lunghezza massima (50 caratteri),
  * `DisplayNameRequired` → campo obbligatorio.

* **AvatarUrl errors**

  * `AvatarUrlMaxLength` → limite di lunghezza dell’URL (2048 caratteri),
  * `AvatarUrlInvalid` → formato URL non valido.

* **Email errors**

  * `EmailInvalid` → formato email non valido,
  * `EmailRequired` → campo obbligatorio,
  * `EmailTooLong` → lunghezza massima (256 caratteri) superata.

* **UserId errors**

  * `UserIdNullOrEmpty` → identificativo utente nullo o vuoto,
  * `UserIdTooLong` → identificativo utente troppo lungo,
  * `UserIdInvalid` → riservato per segnalare caratteri non validi (se/quando verrà introdotta la regola).

Questa suddivisione per region:

* migliora la leggibilità del file,
* rende immediato trovare il messaggio corretto per la proprietà che si sta validando,
* facilita eventuali estensioni future (nuove proprietà o nuove regole per DisplayName, AvatarUrl, Email, UserId).

---

## 3.12 Esempi di utilizzo nel dominio

Nella pratica, questi messaggi vengono utilizzati soprattutto all’interno dei **Value Object**
e, in seconda battuta, nell’entità `ApplicationUser`.

**1. Validazione del DisplayName (VO `DisplayName`)**

```csharp
public static DisplayName Create(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.DisplayNameRequired);
    }

    string v = value.Trim();

    if (v.Length > DisplayName.MaxLength)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.DisplayNameMaxLength);
    }

    return new DisplayName(v);
}
```

**2. Validazione di AvatarUrl (VO `AvatarUrl`)**

```csharp
public static AvatarUrl Create(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return Empty;
    }

    string v = value.Trim();

    if (v.Length > MaxLength)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.AvatarUrlMaxLength);
    }

    if (!Uri.TryCreate(v, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.AvatarUrlInvalid);
    }

    return new AvatarUrl(v);
}
```

**3. Validazione dell’Email (VO `EmailAddress`)**

```csharp
public static EmailAddress Create(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.EmailRequired);
    }

    string v = value.Trim();

    if (v.Length > MaxLength)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.EmailTooLong);
    }

    if (!EmailRegex.IsMatch(v))
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.EmailInvalid);
    }

    return new EmailAddress(v);
}
```

**4. Validazione di UserId (VO `UserId`)**

```csharp
public static UserId Create(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.UserIdNullOrEmpty);
    }

    string trimmed = value.Trim();

    if (trimmed.Length > MaxLength)
    {
        throw new DomainValidationException(
            ApplicationUserErrorMessages.UserIdTooLong);
    }

    // Eventuale uso futuro di UserIdInvalid per pattern/charset specifici.

    return new UserId(trimmed);
}
```

In tutti questi esempi, i messaggi non sono hard-coded, ma vengono sempre recuperati da
`ApplicationUserErrorMessages`, garantendo uniformità e facilitando eventuali cambi futuri.

---

## 3.13 Coerenza con DDD, Clean Architecture e SOLID

`ApplicationUserErrorMessages` è perfettamente allineata ai principi generali del progetto:

* **DDD**

  * I messaggi descrivono regole del dominio utente (UserId, nome visuale, avatar, email) in forma testuale,
    coerente con il linguaggio ubiquo.
  * Rendono esplicito quali vincoli l’utente deve rispettare per essere considerato valido nel sistema.

* **Clean Architecture**

  * La classe vive nel Domain Layer e non dipende da componenti tecnici.
  * I messaggi vengono usati per lanciare eccezioni di dominio (`DomainValidationException`), che a loro volta
    saranno tradotte in risposte tecniche nei layer esterni (Application/API).

* **SOLID (SRP)**

  * `ApplicationUserErrorMessages` ha una responsabilità unica e ben definita:
    centralizzare i messaggi di errore relativi ad `ApplicationUser`.
  * Non contiene logica di validazione, non lancia eccezioni da sola, non interagisce con altri sistemi.

---

## 3.14 Linee guida per l’estensione futura

Nel caso in cui il modello `ApplicationUser` si arricchisca di nuove proprietà (es. bio, location,
social links, impostazioni di visibilità, ecc.), sarà naturale:

* aggiungere nuove costanti in `ApplicationUserErrorMessages`,
* organizzare i messaggi tramite nuove `#region` (ad es. `#region Bio errors`, `#region Social errors`),
* riutilizzare questi messaggi in combinazione con `DomainValidationException`.

La regola rimane:

> ogni volta che il dominio rileva un input non valido relativo all’utente, il messaggio
> dovrebbe arrivare da `ApplicationUserErrorMessages`, non da una stringa scritta “a mano”
> all’interno del codice.

In questo modo, `ApplicationUserErrorMessages` rimane il **punto unico di verità** per
tutti gli errori di validazione legati all’utente nel Domain Layer.

---
