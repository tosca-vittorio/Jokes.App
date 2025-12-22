# 📘 **04_CustomEmailAttributeTest.md — Documentazione Tecnica della Suite di Test per CustomEmailAttribute**

## *Validazione del formato e-mail tramite attributo custom e verifica formale del comportamento del dominio*

---

# 1️⃣ Introduzione generale

`CustomEmailAttribute` è l’attributo di validazione personalizzato utilizzato nel dominio utente (`ApplicationUser`) per garantire che un indirizzo e-mail rispetti:

* un formato rigoroso,
* una sintassi conforme agli standard minimi RFC,
* requisiti strutturali più severi rispetto al `[EmailAddress]` built-in,
* l’assenza di Unicode, emoji e caratteri speciali non ammessi,
* la corretta gestione dei whitespace.

Rispetto ai validatori standard di ASP.NET, `CustomEmailAttribute` applica una **regex più restrittiva**, studiata per evitare input ambigui o malevoli, fornendo al dominio un comportamento affidabile e prevedibile.

La suite di test `CustomEmailAttributeTests.cs` ha lo scopo di verificare in maniera completa e deterministica:

* la correttezza della validazione,
* la conformità della regex a tutti i casi attesi,
* la gestione dei valori borderline,
* il comportamento con messaggi di errore personalizzati,
* la piena coerenza con il documento tecnico `04_CustomEmailAttribute.md`.

---

# 2️⃣ Obiettivi della suite di test

La suite mira a validare i seguenti aspetti:

### 🎯 **1. Gestione dei valori null o whitespace**

Il validator **non** deve fallire quando:

* il valore è `null`
* il valore è stringa vuota `""`
* il valore contiene solo whitespace `"   "`

Ciò perché la presenza obbligatoria del campo è responsabilità di `[Required]`, non del validator di formato.

### 🎯 **2. Validazione del formato e-mail corretto**

Il test verifica che la regex accetti:

* email standard
* email con sottodomini
* email con TLD lunghi
* email con caratteri ammessi nella local-part (., _, %, +, -)
* email con domini contenenti trattini interni
* email con whitespace esterno (che viene trimmato prima della validazione)

### 🎯 **3. Rilevazione email invalide**

Vengono testati molteplici casi non conformi:

* assenza della struttura `local@domain`
* domini incompleti
* TLD troppo corti (<2 caratteri)
* doppio punto o pattern ripetuti
* domini che iniziano con trattino
* Unicode (accentate, caratteri speciali, emoji)

Questa sezione conferma **la robustezza della regex** e la sua aderenza ai requisiti del dominio.

### 🎯 **4. Uso del messaggio di errore personalizzato**

Il test conferma che, se l’attributo viene istanziato con `ErrorMessage = "..."`,
il messaggio personalizzato **sovrascrive quello di default**.

---

# 3️⃣ Architettura dei test e strumenti utilizzati

### ✔ xUnit

Usato come framework principale per la definizione dei test.

### ✔ `ValidationResult` + `GetValidationResult(...)`

Utilizzato al posto di `IsValid()` per ottenere:

* il messaggio di errore,
* la struttura di ritorno standard dei DataAnnotations,
* coerenza con gli altri validatori nel sistema.

### ✔ Assenza di FluentAssertions

A differenza di altre suite, questi test utilizzano gli assert xUnit tradizionali (`Assert.Equal`, `Assert.NotEqual`) poiché lavorano esclusivamente con istanze di `ValidationResult`, mantenendo i test estremamente semplici e diretti.

---

# 4️⃣ Evoluzione storica della suite di test

### 🟦 **Fase 1 — Verifica casi semplici**

Test su `null`, whitespace e email standard.

### 🟦 **Fase 2 — Verifica formati complessi**

Aggiunta di test per:

* sottodomini
* TLD lunghi
* local-part avanzate (`+`, `_`, `-`)
* whitespace ai bordi

### 🟦 **Fase 3 — Casi patologici e Unicode**

Verifica rifiuto di:

* TLD troppo corti
* domini con emoji
* caratteri accentati
* domini invalidi (`-domain`, `domain..com`)

### 🟦 **Fase 4 — Personalizzazione del messaggio d’errore**

Conferma della possibilità di override del messaggio di default.

---

# 5️⃣ Analisi delle categorie di test

---

## 🔷 A. Test su valori null o whitespace

### Obiettivo

Confermare che il validator **non è responsabile della presenza del campo**, ma solo del formato.

### Comportamento verificato

`ValidationResult.Success` in tutti i seguenti casi:

* `null`
* `""`
* `"   "`

Il test conferma che:

```
[CustomEmail] NON sostituisce [Required]
```

---

## 🔷 B. Test di email valide

I test coprono i casi ammessi dalla regex definita nel documento tecnico:

* local-part con punti, underscore, trattini, simboli `+`
* domini con sottodomini multipli
* domini contenenti trattini interni
* TLD di lunghezza ≥ 2
* email con whitespace ai bordi (trim applicato prima della verifica)

Esempi verificati:

* `test@example.com`
* `user.name+tag@sub.domain.com`
* `user_name-123@example.co.uk`
* `"  test@example.com  "`

Il validator deve restituire:

```
ValidationResult.Success
```

---

## 🔷 C. Test di email invalide

Questa categoria rappresenta il cuore della suite.

I casi verificati includono:

### ❌ Formato strutturalmente errato

* `plainaddress`
* `missing@domain`
* `@example.com`

### ❌ Dominio invalido

* `user@.com`
* `user@domain..com`
* `user@-domain.com`

### ❌ TLD errato

* `user@domain.c` (troppo corto)

### ❌ Unicode (vietati)

* `màrio@example.com`
* `utente@domínio.com`
* `test@esempio.cør`
* `user@emoji😊.com`

In tutti i casi l’output è:

```
ValidationError: "L'indirizzo e-mail non è valido."
```

---

## 🔷 D. Test del messaggio personalizzato

Il validator permette l’override del messaggio:

```csharp
var attr = new CustomEmailAttribute { ErrorMessage = "Email non valida!" };
```

Il test conferma che, in caso di errore:

```
result.ErrorMessage == "Email non valida!"
```

Questo è fondamentale per:

* differenziare messaggi di dominio (ApplicationUser)
* localizzazione futura
* UI customizzata

---

# 6️⃣ Diagramma logico semplificato della validazione

```
                   ┌───────────────────────────┐
                   │  CustomEmailAttribute.IsValid  │
                   └───────────────┬───────────────┘
                                   │
                 Value null/empty? ──► YES → Success
                                   │
                                   ▼
                         Trim(value)
                                   │
                                   ▼
                     Regex.IsMatch(trimmedValue)?
                        │                │
                       YES              NO
                        │                │
                        ▼                ▼
                   ValidationSuccess   ValidationError
```

---

# 7️⃣ Buone pratiche adottate

La suite segue principi di qualità professionale:

* test granulari e indipendenti
* copertura completa della regex
* naming intuitivo e descrittivo
* nessuna dipendenza da EF, Identity o domini esterni
* test puri, deterministici, ripetibili
* check espliciti sul messaggio di errore

---

# 8️⃣ Conclusione

La suite di test per `CustomEmailAttribute` garantisce:

* la validità formale del validatore,
* la robustezza della regex verso casi reali e edge-case,
* la compatibilità con l’intero dominio utenti (`ApplicationUser`),
* la correttezza dei messaggi di errore,
* la possibilità di personalizzazione del comportamento.

Il comportamento del validatore è documentato in modo esaustivo e protetto da regressioni future.

---

## ✔ Documento completato

Vuoi che proceda con:

### 👉 **05_SetupDbContextTest.md**

oppure

### 👉 **06_JokesDbContextTest.md**?

Dimmi pure come preferisci continuare.
