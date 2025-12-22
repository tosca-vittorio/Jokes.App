# 📘 **04_CustomEmailAttribute.md**

### *Validazione personalizzata dell’indirizzo e-mail nel dominio utente*

---

# 1️⃣ Introduzione

`CustomEmailAttribute` è un attributo di validazione personalizzato progettato per estendere il sistema di validazione di ASP.NET Core, introducendo regole più rigorose e controllate per il formato degli indirizzi e-mail utilizzati all’interno dell’applicazione **JokesApp**.

Questa componente svolge un ruolo fondamentale nel **dominio dell’utente**, poiché:

* garantisce che gli indirizzi e-mail rispettino uno standard coerente e prevedibile,
* migliora la sicurezza impedendo input anomali, malevoli o formati inconsueti,
* sostituisce la validazione di base offerta da `EmailAddressAttribute`, che è più permissiva,
* viene utilizzata direttamente nel modello `ApplicationUser`, diventando parte integrante della sua integrità.

Per questo motivo la classe viene documentata in continuità con:

* `04_ApplicationUser.md`
* `04_ApplicationUserErrorMessages.md`

---

# 2️⃣ Codice sorgente finale

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace JokesApp.Server.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CustomEmailAttribute : ValidationAttribute
    {
        // Regex: consente lettere, numeri, punti, trattini, underscore nella local part
        // dominio: lettere, numeri, trattini e punti, TLD minimo 2 caratteri
        private static readonly Regex EmailRegex = new Regex(
            @"^[A-Za-z0-9._%+-]+@([A-Za-z0-9]+(-[A-Za-z0-9]+)*\.)+[A-Za-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Required è già gestito da [Required]
                return ValidationResult.Success;
            }

            string email = value.ToString()!.Trim();

            if (!EmailRegex.IsMatch(email))
            {
                return new ValidationResult(
                    ErrorMessage ?? "L'indirizzo e-mail non è valido."
                );
            }

            return ValidationResult.Success;
        }
    }
}
```

---

# 3️⃣ Scopo dell’attributo

L’obiettivo di `CustomEmailAttribute` è:

### ✔ Validare l’indirizzo e-mail con regole più severe

La regex utilizzata impone:

* caratteri ammessi nella *local part*:
  `A-Z a-z 0-9 . _ % + -`
* dominio composto da etichette alfanumeriche con eventuali trattini
* TLD con almeno 2 caratteri (es. `.it`, `.com`, `.net`)

### ✔ Garantire coerenza e prevedibilità nel dominio utente

Poiché l’e-mail rappresenta:

* l’identità digitale dell’utente,
* una chiave primaria logica,
* un riferimento persistente nei sistemi di login,

è fondamentale che rispetti standard rigidi.

### ✔ Evitare input problematici

La validazione impedisce:

* caratteri Unicode non standard,
* domini con simboli non ammessi,
* formati parziali o incompleti (`user@domain`, `user@.com`, ecc.),
* indirizzi con spazi o caratteri invisibili.

---

# 4️⃣ Analisi approfondita della regex

La regex è:

```
^[A-Za-z0-9._%+-]+@([A-Za-z0-9]+(-[A-Za-z0-9]+)*\.)+[A-Za-z]{2,}$
```

Suddividiamola:

### **1) Local part**

```
^[A-Za-z0-9._%+-]+
```

Ammessi:

* lettere (A–Z, a–z)
* numeri
* `. _ % + -`

❌ NO a spazi
❌ NO caratteri Unicode
❌ NO emoji
❌ NO simboli non standard

---

### **2) Dominio**

```
([A-Za-z0-9]+(-[A-Za-z0-9]+)*\.)+
```

Ogni segmento del dominio (es. `example`, `mail`, `my-server`) deve rispettare:

* almeno un carattere alfanumerico
* trattino solo interno, non iniziale/finale
* punto obbligatorio tra i segmenti

Esempi validi:

* `example.com`
* `my-server.company.net`

Esempi non validi:

* `-example.com`
* `example..com`
* `example-.com`

---

### **3) TLD**

```
[A-Za-z]{2,}$
```

* solo lettere
* minimo 2 caratteri

Validi: `.it`, `.com`, `.academy`
Non validi: `.c`, `.1t`, `.co-m`

---

# 5️⃣ Comportamento della validazione

### ✔ Ignora input vuoti

Se la proprietà non contiene valori:

```csharp
return ValidationResult.Success;
```

Perché?

* La responsabilità dello "required" è del DataAnnotation `[Required]`.
* L’attributo deve validare **solo il formato**, non la presenza.

Questo segue le best practice di ASP.NET Core.

---

### ✔ Trim automatico

Prima della validazione:

```csharp
email = value.ToString()!.Trim();
```

Rimuove:

* spazi iniziali e finali
* caratteri invisibili introdotti accidentalmente

---

### ✔ Restituisce un messaggio d’errore personalizzato

Se non viene passato un `ErrorMessage`, utilizza:

```
"L'indirizzo e-mail non è valido."
```

Nel tuo progetto la usi così:

```
[CustomEmail(ErrorMessage = ApplicationUserErrorMessages.EmailInvalid)]
```

quindi si integra perfettamente con i messaggi centralizzati.

---

# 6️⃣ Integrazione con ApplicationUser

Nel modello utente:

```csharp
[Required(ErrorMessage = ApplicationUserErrorMessages.EmailRequired)]
[CustomEmail(ErrorMessage = ApplicationUserErrorMessages.EmailInvalid)]
[MaxLength(256)]
public override string? Email { ... }
```

👉 Ordine delle validazioni:

1. **Required** → obbliga a fornire un indirizzo
2. **CustomEmail** → controlla il formato
3. **MaxLength(256)** → protegge la lunghezza massima lato DB/Identity

Questo rende la tua pipeline email **robusta, sicura, coerente**.

---

# 7️⃣ Quando viene eseguito l’attributo?

### ✔ Durante il binding dei DTO

Se un DTO assegna un valore a `ApplicationUser.Email`, la validazione scatta immediatamente.

### ✔ Durante la creazione utente con Identity

```csharp
_userManager.CreateAsync(user, password)
```

Identity applica tutte le DataAnnotations.

### ✔ Durante la modifica del profilo

Aggiornamenti a `Email` vengono validati allo stesso modo.

### ✔ Durante i test unitari

Ogni test che usa:

```csharp
Validator.TryValidateObject(...)
```

invoca automaticamente CustomEmailAttribute.

---

# 8️⃣ Architettura e Design

### ✔ Single Responsibility Principle

L’attributo ha una responsabilità singola:
**validare un indirizzo e-mail secondo precise regole di dominio.**

### ✔ Open/Closed Principle

È estendibile (nuova regex? supporto IDN?),
ma chiuso alla modifica del comportamento interno.

### ✔ Domain-Driven Design

Rappresenta un *Domain Rule*, non un’infrastruttura.
Per questo risiede in:

```
JokesApp.Server.Domain.Attributes
```

scelta corretta e pulita.

---

# 9️⃣ Diagramma ASCII della sua posizione nel sistema

```
             ┌───────────────────────────┐
             │     ApplicationUser        │
             │  (Email, DisplayName...)   │
             └─────────────┬─────────────┘
                           │ usa
                           ▼
             ┌───────────────────────────┐
             │   CustomEmailAttribute     │
             │  (validazione formato)     │
             └─────────────┬─────────────┘
                           │ partecipa a
                           ▼
             ┌───────────────────────────┐
             │  ASP.NET Core Validation   │
             │  Identity UserManager      │
             │  DTO Model Binding         │
             └───────────────────────────┘
```

---

# 🔟 Conclusioni

`CustomEmailAttribute` è una componente essenziale del dominio utente:

* controlla il formato dell’email in modo rigoroso e sicuro,
* integra perfettamente ApplicationUser,
* sostiene il comportamento di Identity,
* migliora l’affidabilità dell’intera piattaforma,
* è facilmente estensibile e perfettamente documentata.

La sua implementazione è solida, pulita e conforme alle best practice.

---
