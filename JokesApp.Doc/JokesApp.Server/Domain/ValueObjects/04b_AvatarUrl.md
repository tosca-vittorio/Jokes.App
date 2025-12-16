# 📘 **04b_AvatarUrl.md**

### _Value Object per l’URL dell’avatar utente_

---

## 4.18 Ruolo nel dominio

`AvatarUrl` rappresenta l’**URL dell’avatar** di un utente all’interno del dominio.

Non è un semplice `string`, ma un **Value Object** con regole chiare:

- l’avatar è **opzionale**:
  - l’assenza di avatar è un caso previsto e modellato esplicitamente;
- se presente, l’URL deve:
  - rispettare una **lunghezza massima** (2048 caratteri),
  - essere un **URL assoluto**,
  - usare il protocollo **HTTP** o **HTTPS**,
  - essere considerato valido dal punto di vista sintattico (`Uri.TryCreate`).

`AvatarUrl` viene utilizzato all’interno di `ApplicationUser` per rappresentare
eventuale immagine profilo, in modo tipizzato e coerente con le regole del dominio.

---

## 4.19 Definizione della classe

```csharp
using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta l'URL dell'avatar di un utente.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record AvatarUrl
    {
        /// <summary>
        /// Lunghezza massima consentita per l'URL dell'avatar.
        /// </summary>
        public const int MaxLength = 2048;

        /// <summary>
        /// Valore testuale interno dell'URL dell'avatar.
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
        /// e la validazione centralizzata tramite Create().
        /// </summary>
        /// <param name="value">Valore testuale già validato.</param>
        private AvatarUrl(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Factory method che valida e crea un nuovo Value Object
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">
        /// Stringa contenente l'URL dell'avatar. Può essere nulla o vuota
        /// per indicare l'assenza di avatar.
        /// </param>
        /// <returns>Un'istanza valida di <see cref="AvatarUrl"/>.</returns>
        /// <exception cref="DomainValidationException">
        /// Generata se l'URL non è valido o eccede la lunghezza massima consentita.
        /// </exception>
        public static AvatarUrl Create(string? value)
        {
            // If no avatar is provided, return the Empty instance.
            if (string.IsNullOrWhiteSpace(value))
            {
                return Empty;
            }

            // Normalize input by trimming leading/trailing whitespace.
            string v = value.Trim();

            if (v.Length > MaxLength)
            {
                // Avatar URL exceeds maximum allowed length.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.AvatarUrlMaxLength,
                    nameof(AvatarUrl));
            }

            // Validate URL format (absolute HTTP/HTTPS URL).
            if (!Uri.TryCreate(v, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                // Avatar URL is not a valid HTTP/HTTPS URL.
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.AvatarUrlInvalid,
                    nameof(AvatarUrl));
            }

            return new AvatarUrl(v);
        }

        /// <summary>
        /// Istanza vuota, utile per scenari di default, EF Core o binding iniziale,
        /// e per rappresentare l'assenza di un avatar impostato.
        /// </summary>
        public static AvatarUrl Empty { get; } = new AvatarUrl(string.Empty);

        /// <summary>
        /// Restituisce il valore testuale dell'URL dell'avatar.
        /// </summary>
        public override string ToString() => Value;
    }
}
```

Elementi chiave:

- `sealed record` → Value Object immutabile, equality per valore;
- `MaxLength = 2048` → limite esplicito per stringhe URL, in linea con limiti comuni di browser/DB;
- `Create(string?)` → unico punto di creazione, con gestione esplicita del caso “nessun avatar”;
- uso di `ApplicationUserErrorMessages.AvatarUrlMaxLength` e `AvatarUrlInvalid`
  con `DomainValidationException`;
- `Empty` e `IsEmpty` per modellare direttamente “assenza di avatar” nel dominio.

---

## 4.20 Invarianti e regole di validazione

`AvatarUrl` fa rispettare le seguenti regole:

1. **Avatar opzionale**

   ```csharp
   if (string.IsNullOrWhiteSpace(value))
   {
       return Empty;
   }
   ```

   Se il valore è `null`, vuoto o solo spazi:

   - non viene lanciata alcuna eccezione;
   - il dominio interpreta la situazione come “nessun avatar impostato”;
   - ritorna l’istanza `AvatarUrl.Empty`.

   Il fatto che non esista un messaggio `AvatarUrlRequired` in `ApplicationUserErrorMessages`
   è coerente con questa scelta: l’avatar non è un campo obbligatorio.

2. **Normalizzazione (trim)**

   ```csharp
   string v = value.Trim();
   ```

   L’URL viene normalizzato rimuovendo spazi iniziali e finali, per evitare errori
   banali dovuti a input sporchi.

3. **Lunghezza massima**

   ```csharp
   if (v.Length > MaxLength)
   {
    throw new DomainValidationException(
        ApplicationUserErrorMessages.AvatarUrlMaxLength,
        nameof(AvatarUrl));
   }
   ```

   L’URL dell’avatar non può superare `MaxLength = 2048` caratteri, sia per motivi:

   - tecnici (limiti comuni di URL),
   - di robustezza (evitare stringhe patologicamente lunghe).

4. **Formato URL (HTTP/HTTPS assoluto)**

   ```csharp
   if (!Uri.TryCreate(v, UriKind.Absolute, out var uri)
    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
   {
    throw new DomainValidationException(
        ApplicationUserErrorMessages.AvatarUrlInvalid,
        nameof(AvatarUrl));
   }
   ```

   Regole:

   - deve essere un **URI assoluto** (`UriKind.Absolute`),
   - schema ammesso solo `http` o `https`.

   Non vengono accettati:

   - URL relativi,
   - schemi diversi (ftp, file, data, ecc.),
   - stringhe che non sono un URL valido.

Inoltre, quando la validazione fallisce, `AvatarUrl` utilizza `DomainValidationException` specificando `nameof(AvatarUrl)` come `MemberName`, in modo che i layer applicativi possano mappare facilmente l’errore al campo corretto del modello utente.

---

## 4.21 Immutabilità, `Empty` e proprietà di utilità

`AvatarUrl` è dichiarato come:

```csharp
public sealed record AvatarUrl
```

Questo garantisce:

- immutabilità → una volta creato, `Value` non cambia;
- uguaglianza per valore → due `AvatarUrl` con lo stesso `Value` sono considerati uguali.

La gestione del caso “assenza di avatar” è esplicita:

```csharp
public static AvatarUrl Empty { get; } = new AvatarUrl(string.Empty);
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
```

Significa che:

- `AvatarUrl.Empty` rappresenta un utente **senza avatar impostato**;
- `IsEmpty` è il modo idiomatico per verificare questa condizione.

Questo approccio evita l’uso di `null` nel Domain Layer, mantenendo il modello più sicuro
e facile da manutenere.

`Length` e `ToString()` completano le utility:

```csharp
public int Length => Value.Length;
public override string ToString() => Value;
```

---

## 4.22 Utilizzo tipico in `ApplicationUser`

All’interno dell’entità `ApplicationUser`, `AvatarUrl` viene utilizzato per rappresentare
l’eventuale immagine profilo:

```csharp
public class ApplicationUser
{
    public UserId Id { get; private set; }
    public DisplayName DisplayName { get; private set; }
    public EmailAddress Email { get; private set; }
    public AvatarUrl Avatar { get; private set; }

    public void SetAvatar(string? url)
    {
        Avatar = AvatarUrl.Create(url);
    }

    public void RemoveAvatar()
    {
        Avatar = AvatarUrl.Empty;
    }
}
```

Vantaggi:

- la logica di validazione dell’URL non è duplicata nei servizi o nei controller:
  vive solo in `AvatarUrl`;
- il dominio ragiona sempre in termini di `AvatarUrl`:

  - avatar presente → `!Avatar.IsEmpty`,
  - nessun avatar → `Avatar.IsEmpty` / `Avatar == AvatarUrl.Empty`.

---

## 4.23 Coerenza con DDD, Clean Architecture e SOLID

`AvatarUrl` è pienamente allineato all’architettura:

- **DDD**

  - modella un concetto ben preciso del dominio: l’URL dell’avatar utente;
  - incapsula regole e invarianti relativi a quel concetto.

- **Clean Architecture**

  - vive nel Domain Layer;
  - non dipende da framework, da DataAnnotations, da HTTP o da DTO:
    usa solo BCL (`System.Uri`) e componenti di dominio (`ApplicationUserErrorMessages`, `DomainValidationException`).

- **SOLID (SRP)**

  - responsabilità unica → rappresentare e validare l’URL dell’avatar;
  - nessuna logica di persistenza, presentazione o mapping.

---

## 4.24 Estensioni future

In caso di requisiti futuri aggiuntivi (es. limiti di dominio specifici, whitelisting
di host, supporto a CDN dedicate):

- il punto naturale per estendere la logica è `AvatarUrl.Create`,
- l’API pubblica (`Create`, `Empty`, `Value`, `IsEmpty`) può rimanere invariata,
- il resto del dominio continuerà a funzionare senza modifiche.

In sintesi:

> `AvatarUrl` fornisce al dominio un modo tipizzato, sicuro e intenzionale di rappresentare
> l’avatar di un utente, distinguendo chiaramente tra “nessun avatar impostato” e “URL valido”
> secondo le regole del modello.

---
