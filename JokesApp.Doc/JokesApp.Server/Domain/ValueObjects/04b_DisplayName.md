# 📘 **04b_DisplayName.md**

### *Value Object per il nome visuale dell’utente*

---

## 4.1 Ruolo nel dominio

`DisplayName` rappresenta il **nome visuale** dell’utente all’interno del dominio
(es. nome che appare nelle liste, vicino alle joke, nelle classifiche, ecc.).

Dal punto di vista del modello:

- non è una semplice `string`, ma un concetto dotato di regole:
  - è **obbligatorio**,
  - ha una **lunghezza massima** definita dal dominio (50 caratteri),
  - non può essere solo spazi vuoti;
- viene utilizzato all’interno dell’entità di dominio `ApplicationUser` come
  proprietà fortemente tipizzata, al posto di una `string`.

Modellarlo come **Value Object** permette di:

- centralizzare in un unico punto le **regole di validazione** del nome visuale,
- garantire che ogni `ApplicationUser` porti con sé un display name **sempre valido**,
- migliorare espressività e sicurezza del codice rispetto a un semplice `string`.

---

## 4.2 Definizione della classe

```csharp
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Value Object che rappresenta il nome visuale (display name) di un utente.
    /// Immutabile, auto-validante e conforme alle regole del dominio.
    /// </summary>
    public sealed record DisplayName
    {
        /// <summary>
        /// Lunghezza massima consentita per il display name.
        /// </summary>
        public const int MaxLength = 50;

        /// <summary>
        /// Valore testuale interno del display name.
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
        private DisplayName(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Factory method che valida e crea un nuovo Value Object
        /// conforme alle regole del dominio.
        /// </summary>
        /// <param name="value">Stringa contenente il display name.</param>
        /// <exception cref="DomainValidationException">
        /// Generata se il valore è nullo, vuoto o eccede la lunghezza massima consentita.
        /// </exception>
        public static DisplayName Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // Nome visuale obbligatorio
                throw new DomainValidationException(ApplicationUserErrorMessages.DisplayNameRequired);
            }

            // Normalize input by trimming leading/trailing whitespace.
            string v = value.Trim();

            if (v.Length > MaxLength)
            {
                // Lunghezza massima superata
                throw new DomainValidationException(ApplicationUserErrorMessages.DisplayNameMaxLength);
            }

            return new DisplayName(v);
        }

        /// <summary>
        /// Istanza vuota, utile per scenari di default, EF Core o binding iniziale.
        /// </summary>
        public static DisplayName Empty { get; } = new DisplayName(string.Empty);

        /// <summary>
        /// Restituisce il valore testuale del display name.
        /// </summary>
        public override string ToString() => Value;
    }
}
```

Caratteristiche principali:

* `sealed record` → Value Object immutabile, confrontabile per valore;
* `MaxLength = 50` → limite esplicito per il nome visuale, coerente con le regole di dominio;
* proprietà `Value` readonly → incapsula il testo normalizzato del display name;
* proprietà di utilità:

  * `IsEmpty` → indica uno stato vuoto/non inizializzato,
  * `Length` → lunghezza del testo;
* factory `Create(string?)` come **unica porta di ingresso** per ottenere istanze valide;
* uso di `ApplicationUserErrorMessages.DisplayNameRequired` e `DisplayNameMaxLength`
  combinati con `DomainValidationException`;
* `Empty` come istanza speciale per casi tecnici (EF, binding, default).

---

## 4.3 Invarianti e regole di validazione

`DisplayName` incapsula le seguenti regole:

1. **Non nullo e non vuoto**

   ```csharp
   if (string.IsNullOrWhiteSpace(value))
   {
       throw new DomainValidationException(
           ApplicationUserErrorMessages.DisplayNameRequired);
   }
   ```

   Il display name:

   * è obbligatorio,
   * non può essere `null`,
   * non può essere vuoto o composto solo da spazi.

2. **Normalizzazione (trim)**

   ```csharp
   string v = value.Trim();
   ```

   Il valore viene normalizzato eliminando spazi iniziali e finali, così:

   * la validazione di lunghezza è effettuata sul valore realmente significativo,
   * il dominio non distingue in modo artificioso tra `"Mario "` e `"Mario"`.

3. **Lunghezza massima**

   ```csharp
   if (v.Length > MaxLength)
   {
       throw new DomainValidationException(
           ApplicationUserErrorMessages.DisplayNameMaxLength);
   }
   ```

   Il display name non può superare `MaxLength` (50 caratteri).
   Questo limite è pensato per:

   * evitare nomi eccessivamente lunghi e poco usabili in UI/UX,
   * restare coerenti con eventuali vincoli di persistenza,
   * mantenere l’identità visuale dell’utente chiara e leggibile.

Se una di queste regole è violata, `DisplayName.Create` lancia una `DomainValidationException`
con un messaggio centrale definito in `ApplicationUserErrorMessages`, garantendo coerenza
nei testi di errore in tutto il dominio.

---

## 4.4 Immutabilità e semantica di Value Object

`DisplayName` è dichiarato come:

```csharp
public sealed record DisplayName
```

Questo implica:

* **immutabilità**: il valore interno (`Value`) non può essere modificato dopo la creazione;
* **value-based equality**: due `DisplayName` con lo stesso `Value` sono considerati uguali
  a livello di dominio;
* `sealed` → non è prevista una gerarchia di tipi più specifici per il display name,
  semplificando il modello.

La creazione è controllata tramite:

```csharp
private DisplayName(string value) { ... }

public static DisplayName Create(string? value) { ... }
```

Quindi:

* non è possibile istanziare un `DisplayName` arbitrario con `new DisplayName("...")`;
* tutte le istanze “normali” passano attraverso la validazione di `Create`,
  che fa rispettare gli invarianti di dominio.

---

## 4.5 Membro statico `Empty` e proprietà di utilità

`DisplayName` espone:

```csharp
public static DisplayName Empty { get; } = new DisplayName(string.Empty);
public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
public int Length => Value.Length;
```

Questi membri sono pensati per:

* fornire una **istanza vuota controllata** (`Empty`), utile in scenari tecnici:

  * EF Core,
  * binding iniziale di form,
  * test in cui si desidera un placeholder;
* verificare rapidamente se un `DisplayName` rappresenta uno stato vuoto / non inizializzato (`IsEmpty`);
* leggere la lunghezza del testo senza accedere direttamente alla stringa (`Length`).

Dal punto di vista semantico:

* il percorso “valido di dominio” resta la creazione tramite `Create`;
* `Empty` è una scorciatoia tecnica consapevole, da usare con cautela e dove ha senso
  (es. inizializzazioni), non come valore definitivo per un utente reale.

---

## 4.6 Utilizzo tipico nel dominio (`ApplicationUser`)

All’interno dell’entità `ApplicationUser`, `DisplayName` viene utilizzato al posto di una `string`:

```csharp
public class ApplicationUser
{
    public UserId Id { get; private set; }
    public DisplayName DisplayName { get; private set; }
    // ...

    public void SetDisplayName(string? value)
    {
        DisplayName = DisplayName.Create(value);
    }
}
```

Vantaggi:

* l’entità non deve riscrivere le regole (`required`, lunghezza, trim, ecc.): delega tutto al VO;
* qualunque tentativo di impostare un display name non valido viene intercettato dal dominio
  tramite `DomainValidationException`;
* il codice che usa `ApplicationUser` può dare per scontato che `DisplayName` sia sempre coerente
  con le regole del modello.

---

## 4.7 Coerenza con DDD, Clean Architecture e SOLID

`DisplayName` rispetta pienamente i principi alla base dell’architettura:

* **DDD**

  * rappresenta esplicitamente un concetto del linguaggio ubiquo: il nome visuale dell’utente;
  * incapsula le regole che lo definiscono valido a livello di dominio.

* **Clean Architecture**

  * vive nel Domain Layer;
  * non conosce Identity Core, database, JSON, HTTP: usa solo
    `DomainValidationException` e `ApplicationUserErrorMessages`.

* **SOLID (SRP)**

  * ha una responsabilità unica: rappresentare e validare il display name;
  * non gestisce persistenza, logging, mapping verso DTO, o logica applicativa.

---

## 4.8 Estensioni future

Se in futuro le regole sul display name cambiano (es. divieto di certe parole,
vincoli di pattern, normalizzazione Unicode, ecc.), il punto naturale in cui intervenire è:

```csharp
public static DisplayName Create(string? value)
{
    // nuove regole aggiunte qui
}
```

Tutto il resto del dominio continuerà a usare `DisplayName` senza modifiche:

> chi possiede un `DisplayName` può assumere che rispetti **sempre** le regole
> più aggiornate del dominio per il nome visuale di un utente.

---
