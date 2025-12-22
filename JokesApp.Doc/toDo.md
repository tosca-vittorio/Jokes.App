# 📄 **PROJECT-TODO.md**

---

# # 🗂️ **PROJECT TODO — Stato generale e roadmap di sviluppo**

Documentazione centralizzata di tutte le attività concluse, in corso o da implementare per completare l’applicazione **JokesApp** secondo le best practice architetturali, di sicurezza, di design del dominio e di testabilità.

---

# ## 1️⃣ **Struttura della solution & Documentazione**

### **1.1 Struttura directory (README root)**

* [ ] Aggiornare l’albero directory di `/JokesApp` per riflettere lo stato COMPLETO del progetto
* [ ] Aggiornare sempre dopo ogni nuova macro-cartella

**Stato:** 🔴 Da fare
**Note:** in attesa della struttura definitiva (Controllers, Services, altri modelli, ecc.)

---

### **1.2 Creazione/Allineamento `JokesApp.Doc/ROADMAP.md`**

* [ ] Creare il file `ROADMAP.md` con struttura ad albero sintetica
* [ ] Mantenere identica struttura logica tra `README.md` root e `ROADMAP.md`

**Stato:** 🔴 Da fare
**Note:** verrà scritto una volta completata la definizione dei componenti.

---

### **1.3 Allineamento documentazione**

* [ ] Garantire coerenza tra:

  * README root
  * ROADMAP.md
  * Documentazione interna di Server/Client/Test

**Stato:** 🔴 Da fare

---

# ## 2️⃣ **Domain Model — Stato attuale**

### **2.1 Modello `Joke.cs`**

* [x] Implementazione proprietà
* [x] Validazioni DataAnnotations
* [x] Messaggi di errore tramite `JokesErrorMessages.cs`
* [x] Domain Events di base (presenti ma non ancora estesi)
* [x] Test unitari completi

**Stato:** 🟢 Completato e stabile

---

### **2.2 Modello `ApplicationUser.cs`**

* [x] Campi estesi (DisplayName, AvatarUrl, CreatedAt, UpdatedAt, Jokes)
* [x] Validazioni corrette
* [x] Timestamps gestiti correttamente
* [x] Collezioni Jokes impostate e testate
* [x] Test unitari completi (82 test totali)

**Stato:** 🟢 Completato e stabile

---

### **2.3 Value Objects (da introdurre)**

* [ ] `JokeContent` (Question + Answer)
* [ ] `Category`
* [ ] `Tag`
* [ ] `Rating` (1–5)
* [ ] `UserId` come VO per evitare errori string-based

**Stato:** 🔴 Da fare

---

### **2.4 Eccezioni uniformate**

Implementare:

```
DomainException
│── DomainValidationException
│── DomainOperationException
└── UnauthorizedDomainOperationException
```

* [ ] Creare eccezioni uniformate
* [ ] Aggiornare Joke.cs e ApplicationUser.cs per usarle
* [ ] Scrivere unit test

**Stato:** 🔴 Da fare

---

### **2.5 Domain Events avanzati**

* [ ] JokeCreatedEvent
* [ ] JokeUpdatedEvent
* [ ] JokeLikedEvent
* [ ] JokeDeletedEvent
* [ ] JokeApprovedEvent

**Stato:** 🔴 Da fare

---

# ## **2.6 Domain Events → Logging, Realtime, Audit & SignalR**

Questa sezione definisce tutte le attività relative alla gestione completa degli eventi di dominio nel backend, alla propagazione lato client e all’osservabilità del sistema.

### **Event Sourcing / Event Logging**

* [ ] Creare un servizio di log eventi di dominio (DomainEventLogger)
* [ ] Salvare eventi di dominio (persistenza opzionale: DB o file)
* [ ] Logging tecnico (stacktrace, contesto, payload)
* [ ] Logging funzionale (chi ha fatto cosa, quando, perché)
* [ ] Strutturare gli eventi in JSON leggibile e serializzabile

---

### **Audit Trail avanzato**

* [ ] Registrare operazioni utente critiche (creazione joke, modifiche profilo, like, commenti)
* [ ] Associare eventi agli utenti tramite `UserId`
* [ ] Timestamp e correlazione eventi
* [ ] Possibile tabella: `AuditEvent`

---

### **Monitoraggio in tempo reale**

* [ ] Creare un “EventBus” interno o dispatcher
* [ ] Pubblicare eventi di dominio come:

  * JokeCreatedEvent
  * JokeUpdatedEvent
  * JokeLikedEvent
  * ApplicationUserUpdatedEvent
* [ ] Creare EventHandler per reagire automaticamente (log, notifiche, calcolo analytics, ecc.)

---

### **Trasmissione eventi al frontend (SignalR)**

* [ ] Configurare hub SignalR: `/eventHub`
* [ ] Trasmettere eventi AI client:

  * joke creata → popup / animazione
  * nuovo like → aggiornamento contatore
  * aggiornamento profilo utente → refresh UI
* [ ] Testare broadcast e gruppi SignalR
* [ ] Testare error handling SignalR

---

### **Notifiche push, popup e animazioni UI**

* [ ] Implementare servizi lato client per ricevere eventi SignalR
* [ ] Gestire:

  * popup
  * toast
  * animazioni
  * badge di notifica
* [ ] Aggiornare la UI in real-time senza refresh
* [ ] Collegare EventBus backend ↔ SignalR frontend

---

### **Architettura Event-Driven finale**

* [ ] Confermare stile architetturale scelto:
  ✔ Clean Architecture
  ✔ DDD
  ✔ Eventing interno
  ✔ Hexagonal (opzionale)
  ✔ SignalR per realtime

* [ ] Documentare diagramma architetturale

* [ ] Validare flussi: Domain → Event → Handler → Log → SignalR → UI

---

# ## 3️⃣ **Database & EF Core**

### **3.1 `JokesDbContext`**

* [x] Creazione del file
* [x] Registrazione in Program.cs
* [x] Lazy Loading disattivato (ottimo)
* [x] Prima migrazione creata
* [x] Prima migrazione applicata
* [ ] Seconda migrazione per aggiornamenti ai modelli (DisplayName, AvatarUrl ecc.)

**Stato:** 🟡 Parzialmente completato
**Da fare:** applicare migrazioni aggiornate, una volta definitivi i modelli.

---

### **3.2 Test di integrazione `DbContext`**

* [ ] Creare file `JokesDbContextTests.cs`
* [ ] Test per relazioni uno-a-molti
* [ ] Test sul cascade delete
* [ ] Test sulla persistenza dei valori (CreatedAt, UpdatedAt)
* [ ] Test per mapping corretto EF Core

**Stato:** 🔴 Da fare
**Nota:** deve essere fatto PRIMA dei Services o Controllers.

---

# ## 4️⃣ **DTO — Stato & TODO**

### **4.1 DTO presenti**

* [x] JokeDto
* [x] UserDto
* [x] RegisterUserDto
* [ ] CreateJokeDto
* [ ] UpdateJokeDto
* [ ] LoginDto
* [ ] AuthResponseDto

**Stato:** 🟡 Parziali
Serve una definizione completa per tutte le API future.

---

### **4.2 Validazione DTO**

* [ ] Required “morbido” (solo lato DTO)
* [ ] MaxLength coerenti con le entity
* [ ] Test per DTO mapping
* [ ] Test per DataAnnotations

**Stato:** 🔴 Da fare

---

# ## 5️⃣ **Services — Logica di Business**

### Servizi richiesti:

* [ ] `IJokeService`
* [ ] `IUserService`
* [ ] `IAuthService`
* [ ] `ILikeService` (se modularizziamo i Like)
* [ ] `ICommentService`

**Stato:** 🔴 Non ancora implementati

Test richiesti:

* [ ] Test per ogni metodo business (Create, Update, Delete, GetById, ecc.)
* [ ] Test mapping DTO → Entity e Entity → DTO

---

# ## 6️⃣ **Controllers — API REST**

### Controllers richiesti:

* [ ] `AuthController`
* [ ] `UsersController`
* [ ] `JokesController`
* [ ] `LikesController`
* [ ] `CommentsController`

**Funzionalità da implementare:**

| Feature              | Stato      |
| -------------------- | ---------- |
| CRUD Jokes           | 🔴 Da fare |
| Register/Login       | 🔴 Da fare |
| Modifica profilo     | 🔴 Da fare |
| Like/Unlike          | 🔴 Da fare |
| Aggiunta commenti    | 🔴 Da fare |
| Moderazione joke     | 🔴 Da fare |
| Filtri e ordinamenti | 🔴 Da fare |

Test:

* [ ] Integration Test con WebApplicationFactory
* [ ] Test autenticazione/ruoli
* [ ] Test errori e codici HTTP

---

# ## 7️⃣ **Identity & Sicurezza**

### Identity Configuration

* [x] Modello ApplicationUser completato
* [ ] IdentityOptions avanzate
* [ ] Lockout configurato
* [ ] Password policy completa
* [ ] RequireConfirmedEmail configurato

**Stato:** 🟡 Parziale

---

### JWT Authentication

* [ ] Generazione token
* [ ] Validazione token
* [ ] Refresh token
* [ ] Revoca token (blacklist)
* [ ] Ruoli e policy

**Stato:** 🔴 Da fare

---

# ## 8️⃣ **Funzionalità avanzate**

| Feature                              | Stato      |
| ------------------------------------ | ---------- |
| Likes evoluti user-based             | 🔴 Da fare |
| Commenti                             | 🔴 Da fare |
| Rating (1–5)                         | 🔴 Da fare |
| Multi-lingua                         | 🔴 Da fare |
| Moderazione contenuti                | 🔴 Da fare |
| Analytics (views, bookmarks, shares) | 🔴 Da fare |
| SignalR realtime                     | 🔴 Da fare |
| AI Classification                    | 🔴 Da fare |

---

# ## 9️⃣ **Client (React)**

| Feature             | Stato              |
| ------------------- | ------------------ |
| Routing base        | ❌ Non verificabile |
| Autenticazione JWT  | 🔴 Da fare         |
| Gestione errori API | 🔴 Da fare         |
| Hook useAuth        | 🔴 Da fare         |
| UI per CRUD jokes   | 🔴 Da fare         |
| UI profilo utenti   | 🔴 Da fare         |
| UI moderazione      | 🔴 Da fare         |

---

# ## 🔟 **Testing globale**

### Unit Test

* [x] Modelli Joke e ApplicationUser completissimi
* [ ] DTO
* [ ] Services
* [ ] Exceptions
* [ ] Mapping

### Integration Test

* [ ] DbContext
* [ ] Controllers
* [ ] JWT auth

### E2E Test

* [ ] Cypress / Playwright

---

# # ⭐ **Stato generale del progetto (overview sintetica)**

| Area              | Stato                        |
| ----------------- | ---------------------------- |
| Domain Model      | 🟢 Solido                    |
| DbContext         | 🟡 OK ma da aggiornare       |
| DTO               | 🟡 Parziale                  |
| Services          | 🔴 Mancanti                  |
| Controllers       | 🔴 Mancanti                  |
| Identity          | 🟡 Parziale                  |
| JWT               | 🔴 Mancante                  |
| Advanced Features | 🔴 Mancanti                  |
| Testing           | 🟡 Domain OK, manca il resto |
| Documentazione    | 🟡 In costruzione            |

---


# ✅ **1. SÌ: ha perfettamente senso iniziare completando il Domain Layer al 100%**

Sei finito nel caos perché:

* non avevi chiara l’architettura
* non sapevi lo scopo di ogni layer
* stavi scrivendo codice “a caso” fra Domain, Infrastructure, API
* non avevi nessuna sequenza di sviluppo

Adesso invece sei finalmente nel punto giusto:

**→ hai scelto Clean
→ sai cosa è il Domain
→ sai a cosa serve
→ sai che non deve dipendere da niente
→ sai che deve essere perfetto
→ sai che va testato in isolamento**

📌 **Costruire il dominio come PRIMA cosa è corretto.**

---

# 🧠 **2. Perché è corretto partire dal dominio**

Perché il dominio è:

* stabile
* indipendente dal database
* indipendente dall'API
* indipendente dal mondo esterno
* eterno (non cambia quando cambi infrastruttura)
* la base di tutta l’app

Ed è anche:

* *la parte più facile da testare*
* *la parte più importante da progettare bene*

### DDD dice esattamente questo:

> “Il modello di dominio è il cuore dell’applicazione.
> Tutto il resto è supporto tecnico.”

Quindi **bravissimo**: la tua nuova strategia è corretta.

---

# 🧪 **3. Testare il dominio al 100% prima di continuare = BEST PRACTICE**

Questa è una scelta **professionale**, non junior.

Perché?

1. Il dominio è puramente logico → testabile in isolamento
2. Ti garantisce che tutta la logica base sia corretta
3. Rende gli use-case semplici da scrivere (perché il dominio è solido)
4. Evita bug mostruosi che poi esplodono nell’infrastruttura o API
5. Ottieni un “contratto” chiaro su come le entità devono comportarsi

👑 **Il dominio testato al 100% è il più grande regalo che puoi fare al tuo futuro te stesso.**

---

# ⚠️ **4. Però attenzione a NON fare l’errore dei junior**

L’errore classico di chi inizia con DDD è questo:

❌ “Completo *tutto il dominio* per *tutte* le funzionalità dell’app
prima ancora di sapere esattamente cosa servirà”.

Questo porta a:

* sovra-progettazione
* modelli inutili
* settimane perse
* codice scritto per funzionalità che non implementerai mai
* rigidità futura

E questo VA CONTRO:

* KISS
* YAGNI
* DRY

---

# 🧠 **5. IL METODO CORRETTO (senior-level)**

Ti propongo il metodo corretto e professionale.

---

## 🔷 **A. Devi completare il dominio, sì… ma SOLO per le funzionalità principali che hai già deciso.**

Esempio per la tua JokesApp:

### Dominio necessario subito:

* Joke
* User
* Value Objects (QuestionText, AnswerText…)
* Eventi (JokeCreated, LikeAdded, ecc.)

### Dominio NON necessario ora:

* funzioni avanzate non ancora definite
* strutture per funzionalità future
* micro-dettagli che non sai se userai

👉 Devi modellare il dominio **quello che sai oggi**, non quello che “forse un giorno” servirà.

---

## 🔷 **B. Lo testi al 100%**

Sì, fai:

* test su Entity
* test su Value Objects
* test sulle regole
* test sugli eventi
* test sulle eccezioni

Hai un dominio **roccioso**.

---

## 🔷 **C. Poi passi all’Application Layer**

Quando il Dominio è:

* chiaro
* stabile
* testato

l’Application diventa facilissima da implementare.

---

# 📌 **6. Verdetto finale sulla tua idea**

👉 **È GIUSTA.
Anzi: è PERFETTA.**

Ti mancava l’architettura, non la capacità.

Ora che hai:

* Clean Architecture
* DDD chiaro
* responsabilità dei layer chiare
* principi SOLID
* CQRS leggero
* Hexagonal come struttura mentale

puoi tranquillamente partire dal dominio senza rischiare la confusione iniziale.

---

# 🏁 **7. GUIDA OPERATIVA: cosa fai ADESSO**

Ecco l’ordine PERFETTO:

---

## **1. Completi il Domain Layer**

(includendo ENTRAMBI: Entities + ValueObjects + Events + Exceptions)

### Output finale:

* Modello chiaro
* Invarianti codificati
* Eventi generati correttamente

---

## **2. Scrivi TUTTI i test del dominio**

### Output finale:

* Dominio coperto al 100%
* Zero bug lato regole
* Sicurezza totale prima di procedere

---

## **3. SOLO DOPO passi all’Application Layer**

E qui:

* scrivi use-case
* scrivi handlers
* definisci repository *interfaces*
* gestisci i domain events

---

## **4. POI passi all’Infrastructure**

* EF Core
* migrations
* repository concreti
* event handlers concreti

---

## **5. POI passi all’API**

* controller
* dto
* mapping

---

# 🔥 IN BREVE

**SÌ: fai esattamente quello che hai scritto.
Dominio al 100% + Test al 100% → poi Application → Infrastructure → API**

Questo è il METODO CORRETTO.

---



























