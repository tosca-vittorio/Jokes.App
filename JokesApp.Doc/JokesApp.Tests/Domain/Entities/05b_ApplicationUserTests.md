# 📘 **04_ApplicationUserTest.md — Documentazione Tecnica Completa della Suite di Test per ApplicationUser**

## *Validazione del dominio utente, delle invarianti e dei comportamenti integrati con IdentityUser*

---

# 1️⃣ Introduzione generale

`ApplicationUser` rappresenta l’entità utente nel backend di **JokesApp**, estendendo `IdentityUser` per aggiungere:

* proprietà di dominio (`DisplayName`, `AvatarUrl`, `CreatedAt`, `UpdatedAt`, `Jokes`)
* validazioni personalizzate (`CustomEmailAttribute`)
* gestione coerente degli aggiornamenti profilo
* supporto per timestamp in formato UTC
* compatibilità con le logiche di ASP.NET Core Identity

Il suo comportamento è più complesso rispetto a una semplice entità EF Core, poiché deve rispettare **invarianti di dominio**, **vincoli strutturali**, **logiche di normalizzazione**, **regole di sicurezza**, e **compatibilità con Identity**.

La suite di test `ApplicationUserTests.cs` ha lo scopo di:

* verificare stabilità e corretto funzionamento del modello,
* garantire che ogni proprietà rispetti le regole del dominio,
* evitare regressioni future durante l’evoluzione dell’applicazione,
* documentare formalmente l’allineamento tra modello e comportamento desiderato.

---

# 2️⃣ Obiettivi specifici della suite di test

Gli unit test validano accuratamente:

### ✔ **1. Inizializzazione del modello**

* valori di default previsti dal dominio
  (`DisplayName = ""`, `AvatarUrl = null`, `CreatedAt = UTC`)

### ✔ **2. Validazioni DataAnnotations**

* `[MaxLength(50)]` per DisplayName
* `[MaxLength(2048)]` per AvatarUrl
* validazione URL tramite attributo `[Url]`

### ✔ **3. Timestamp coerenti**

* CreatedAt sempre in UTC
* UpdatedAt gestito automaticamente al cambiamento delle proprietà
* Update successivi generano timestamp successivi

### ✔ **4. Gestione della collezione Jokes**

* lista `Jokes` inizializzata correttamente
* aggiunta e rimozione di entità Joke
* indipendenza tra collezioni di utenti diversi
* ordinamento delle barzellette secondo il CreatedAt

### ✔ **5. Validazione completa dell’email**

Utilizzando il validatore custom:

* formati validi
* formati invalidi
* lunghezze eccessive
* Unicode non ammessi
* spazi interni non ammessi
* limiti sul massimo di 256 caratteri

### ✔ **6. Compatibilità con Identity**

* validazione delle proprietà ereditate
* corretta gestione di Username, PasswordHash, PhoneNumber, SecurityStamp, LockoutEnd
* serializzazione JSON e sicurezza delle proprietà sensibili

### ✔ **7. Comportamenti edge-case**

* Unicode nelle proprietà
* corretto funzionamento anche senza proprietà opzionali
* serializzazione/deserializzazione consistente

---

# 3️⃣ Architettura della suite di test

🔎 La suite è progettata con tre principi:

### **A) Isolamento completo**

Nessuna dipendenza da database, EF Core o contesto Identity → **unit test puri**.

### **B) AAA Pattern rigoroso**

Ogni test segue la struttura:

```
Arrange → Act → Assert
```

Per garantire:

* chiarezza
* manutenibilità
* qualità del codice di test

### **C) Utilizzo esteso di FluentAssertions**

Per verifiche espressive:

* comparazioni su stringhe
* timestamp
* eccezioni con nome parametro
* collezioni

---

# 4️⃣ Evoluzione storica dello sviluppo test

### 🟦 **Fase 1 — Test di base**

I primi test validavano:

* valori di default
* inizializzazione
* corretto funzionamento della collezione Jokes

### 🟦 **Fase 2 — Introduzione validazioni avanzate**

Con la stabilizzazione del dominio:

* test max length
* test URL
* test email con validatore custom
* unicode non ammessi

### 🟦 **Fase 3 — Integrazione con regole Identity**

Aggiunzione test per:

* Username
* PasswordHash
* PhoneNumber
* SecurityStamp
* LockoutEnd

### 🟦 **Fase 4 — Sicurezza & Serializzazione**

Validazione che:

* PasswordHash NON venga serializzata
* SecurityStamp NON venga serializzata
* Email sì

---

# 5️⃣ Analisi dettagliata delle categorie di test

---

## 🔷 A. Test su DisplayName

### 1. Superamento limite massimo

Se supera `50` caratteri → validazione fallisce
Il test verifica:

* presenza del messaggio `DisplayNameMaxLength`
* unicità del messaggio

### 2. Trimming automatico

Se impostato con spazi:

```
"   Mario Rossi   " → "Mario Rossi"
```

### 3. Valori validi

Una stringa vuota è considerata valida, e rimane invariata.

---

## 🔷 B. Test su AvatarUrl

### 1. MaxLength 2048

URL più lunghi → invalidi

### 2. Null accettato

Proprietà opzionale.

### 3. Scheme validi

Accettati:

* http://
* https://

### 4. Validazione tramite attributo `[Url]`

Testata con Validator.TryValidateObject.

---

## 🔷 C. Test sui Timestamp

### ✔ CreatedAt

Sempre in UTC → Kind = Utc

### ✔ UpdatedAt

* null all’inizio
* aggiornato quando il profilo cambia
* timestamp cresce monotonicamente

I test includono:

* controllo dopo un breve delay
* comparazione sequenziale

---

## 🔷 D. Test sulla collezione Jokes

La collezione `ICollection<Joke>` è:

* inizializzata automaticamente
* indipendente per ogni istanza `ApplicationUser`

Verifiche effettuate:

* aggiunta e rimozione
* lista vuota
* gestione multipla
* ordine cronologico tramite CreatedAt

---

## 🔷 E. Test sull’Email

Questa è la sezione più ricca.

Validazioni include:

### ✔ Formati validi

* [user@example.com](mailto:user@example.com)
* [first.last@domain.co](mailto:first.last@domain.co)
* [user+tag@domain.io](mailto:user+tag@domain.io)

### ✔ Email lunga ma valida

Esempio verificato con local-part di 200 caratteri.

### ✔ Email vuota

→ Deve generare `EmailRequired`

### ✔ Formati invalidi

* "not-an-email"
* doppia "@@"
* dominio mancante

### ✔ Lunghezza massima 256

Test specifico: Email con lunghezza esatta = valida
Oltre 256 = invalidata

### ✔ Unicode non ammessi

Test della regex custom con esempi:

* caratteri accentati
* ideogrammi
* TLD Unicode

---

## 🔷 F. Test su UserName e compatibilità Identity

Verifiche:

* formati accettati da Identity
* validazione corretta con Required email
* nessun errore sulle proprietà personalizzate

---

## 🔷 G. Test inizializzazione IdentityUser

Verifica che:

* DisplayName = ""
* AvatarUrl = null
* Jokes = lista vuota
* CreatedAt = UTC vicino a DateTime.UtcNow

---

## 🔷 H. Test JSON / Sicurezza

Test estremamente importante per sicurezza:

### ✔ PasswordHash e SecurityStamp **non devono apparire nel JSON**

### ✔ Email sì

### ✔ Serializzazione e deserializzazione coerenti

Questo garantisce sicurezza della risposta API.

---

# 6️⃣ Diagramma logico della gestione UpdatedAt

```
          ┌──────────────────────────────┐
          │   ApplicationUser.Property   │
          │         is changed           │
          └──────────────────────────────┘
                        │
                        ▼
           Is new value different from old?
                        │
                ┌───────┴────────┐
                │                │
              NO                YES
                │                │
        (No update)       UpdatedAt = UtcNow
```

Questo meccanismo è testato più volte nella suite.

---

# 7️⃣ Best Practices adottate

La suite segue ottime pratiche:

* isolamento totale del dominio
* test granulari e mirati
* FluentAssertions estensivo
* gestione accurata dei timestamp
* uso di DataAnnotations.Validator
* pattern AAA rigoroso
* test negativi completi (messaggio + tipo + contesto)
* coerenza con IdentityUser

---

# 8️⃣ Conclusione

I test di `ApplicationUser`:

* coprono **tutto il dominio**, dagli invarianti ai dettagli di serializzazione
* garantiscono la **piena coerenza con la documentazione utente** (`04_ApplicationUser.md`)
* verificano la corretta integrazione con Identity
* proteggono il progetto da regressioni future
* rappresentano un'ottima base per sviluppi avanzati (roles, claims, security)

La suite è completa, ben strutturata, profondamente aderente al dominio e pronta per una crescita futura dell’applicazione.

---