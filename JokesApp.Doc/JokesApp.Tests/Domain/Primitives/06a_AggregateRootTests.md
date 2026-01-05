# 📘 JokesApp.Doc/JokesApp.Tests/Domain/Primitives/06a_AggregateRootTests.md — Manuale didattico per `AggregateRoot`

> **File di test:** `JokesApp.Tests/Domain/Primitives/AggregateRootTests.cs`
> **Classe di test:** `JokesApp.Tests.Domain.Primitives.AggregateRootTests`
> **Soggetto:** Classe `AggregateRoot` (Layer di dominio)

---

## 1️⃣ Scopo

Questa suite di test si concentra sulla verifica del comportamento della classe `AggregateRoot`, che gestisce la coda di eventi di dominio (`Domain Events`). In particolare, il test verifica la protezione contro gli input null, l’ordine di accodamento degli eventi, e il comportamento delle operazioni di estrazione e pulizia degli eventi. Il suo scopo è assicurarsi che la gestione della coda di eventi sia sicura, consistente e conforme ai principi di `Domain-Driven Design` (DDD).

**Obiettivi principali della suite di test:**

* **Protezione contro input null**: L’evento di dominio deve essere robusto e rifiutare eventuali tentativi di aggiungere un evento nullo, evitando errori nel flusso di lavoro.
* **Ordine FIFO degli eventi**: La coda deve garantire che gli eventi vengano estratti nell'ordine in cui sono stati aggiunti (FIFO - First In, First Out).
* **Pulizia e svuotamento**: Deve essere possibile estrarre tutti gli eventi tramite `PullDomainEvents()` e svuotare la coda, e ci deve essere una funzione esplicita per pulire gli eventi con `ClearDomainEvents()`.

Questi comportamenti sono essenziali per mantenere la consistenza degli eventi nel dominio e prevenire comportamenti indesiderati.

---

## 2️⃣ Strumenti e pattern ricorrenti

### ✅ xUnit

* **`[Fact]`**: Test singolo, non parametrico. Ogni metodo di test è indipendente e isolato per garantire la precisione dei risultati.

### ✅ FluentAssertions

* **`.Should().Be(...)`**: Asserzione di uguaglianza diretta tra le proprietà dell’evento e i valori passati al costruttore.
* **`.Throw<DomainValidationException>()`**: Verifica che venga sollevata un'eccezione di tipo specifico, nel caso di eventi invalidi (come eventi nulli).
* **`.WithMessage(...)`**: Assicura che l’eccezione lanciata contenga il messaggio di errore previsto, mantenendo coerenza e uniformità nel dominio.
* **`.Which.MemberName`**: Verifica il nome del parametro associato all’errore, permettendo un controllo preciso sui singoli parametri dell'evento.

### ✅ Lambda “act” (per testare eccezioni)

Per testare situazioni di errore, l’azione che deve sollevare l’eccezione viene incapsulata in una lambda. Questo approccio consente a FluentAssertions di catturare e verificare in modo preciso l’eccezione lanciata durante l’esecuzione.

```csharp
var act = () => /* operazione che deve lanciare l’eccezione */;
```

---

## 3️⃣ Test documentati (1:1 con `AggregateRootTests.cs`)

### 🧪 Test: `AddDomainEvent_ShouldThrow_WhenEventIsNull`

**Scopo:** Verifica che il metodo `AddDomainEvent` lanci un'eccezione quando l'evento passato è nullo, proteggendo la coda da eventi invalidi. La validazione deve intercettare un evento nullo e lanciare un’eccezione di tipo `ArgumentNullException`, garantendo che il dominio non possa avere eventi non definiti.

**Sintassi (estratto):**

```csharp
var aggregate = new FakeAggregateRoot();
var act = () => aggregate.AddEvent(null!);
act.Should()
   .Throw<ArgumentNullException>()
   .WithParameterName("domainEvent");
```

**Spiegazione:**

* **`null!`**: Il `null!` è usato per bypassare i warning di nullability e per forzare un errore quando si tenta di passare un evento nullo.
* **`WithParameterName("domainEvent")`**: Verifica che l'eccezione sia sollevata a causa di un parametro nullo e assicura che il nome del parametro violato sia quello corretto (`domainEvent`).

---

### 🧪 Test: `AddDomainEvent_ShouldPreserveOrder_WhenMultipleEventsAreAdded`

**Scopo:** Verifica che la coda di eventi rispetti l'ordine di inserimento degli eventi (FIFO). Quando più eventi sono aggiunti alla coda, devono essere estratti nell’ordine in cui sono stati inseriti.

**Sintassi (estratto):**

```csharp
var aggregate = new FakeAggregateRoot();
var firstEvent = new FakeDomainEvent("first");
var secondEvent = new FakeDomainEvent("second");

aggregate.AddEvent(firstEvent);
aggregate.AddEvent(secondEvent);

aggregate.DomainEvents.Should().ContainInOrder(firstEvent, secondEvent);
```

**Spiegazione:**

* **FIFO (First In, First Out)**: Il test verifica che l'ordine di aggiunta degli eventi venga rispettato. La funzione `ContainInOrder` è utilizzata per assicurarsi che gli eventi siano restituiti nello stesso ordine in cui sono stati aggiunti.
* Questo comportamento è essenziale per garantire la corretta sequenza degli eventi nel dominio.

---

### 🧪 Test: `PullDomainEvents_ShouldReturnEmpty_WhenNoEventsAreQueued`

**Scopo:** Verifica che quando non ci sono eventi nella coda, la funzione `PullDomainEvents` ritorni una sequenza vuota, senza alterare lo stato della coda. L’obiettivo è assicurarsi che la coda non contenga eventi non processati quando non è stata popolata.

**Sintassi (estratto):**

```csharp
var events = aggregate.PullDomainEvents();
events.Should().BeEmpty();
aggregate.DomainEvents.Should().BeEmpty();
```

---

### 🧪 Test: `PullDomainEvents_ShouldReturnSnapshotAndClearQueue_WhenEventsExist`

**Scopo:** Verifica che quando la coda contiene eventi, il metodo `PullDomainEvents` ritorni una "snapshot" (una copia) degli eventi e successivamente svuoti la coda, garantendo che una seconda chiamata a `PullDomainEvents` ritorni una sequenza vuota.

**Sintassi (estratto):**

```csharp
aggregate.AddEvent(firstEvent);
aggregate.AddEvent(secondEvent);

var events = aggregate.PullDomainEvents();

events.Should().HaveCount(2);
events.Should().ContainInOrder(firstEvent, secondEvent);
aggregate.DomainEvents.Should().BeEmpty();

aggregate.PullDomainEvents().Should().BeEmpty();
```

**Spiegazione:**

* Il metodo `PullDomainEvents` restituisce una copia degli eventi e svuota la coda interna, assicurando che non vi siano duplicati e che ogni evento venga processato una sola volta.

---

### 🧪 Test: `ClearDomainEvents_ShouldRemoveAllEvents_WhenCalled`

**Scopo:** Verifica che il metodo `ClearDomainEvents` rimuova correttamente tutti gli eventi dalla coda, garantendo che la coda sia completamente vuota dopo la sua esecuzione.

**Sintassi (estratto):**

```csharp
aggregate.AddEvent(new FakeDomainEvent("first"));
aggregate.AddEvent(new FakeDomainEvent("second"));

aggregate.ClearEvents();

aggregate.DomainEvents.Should().BeEmpty();
aggregate.PullDomainEvents().Should().BeEmpty();
```

**Spiegazione:**

* **`ClearDomainEvents`**: Metodo che svuota esplicitamente la coda degli eventi, utile per garantire che la coda non contenga eventi precedenti che non siano stati processati o siano obsoleti.

---

## 4️⃣ Test doubles: spiegazione sintattica

Per testare il comportamento della coda degli eventi, vengono utilizzati **test doubles** per simulare i comportamenti di `AggregateRoot` e `DomainEvent`. La classe `FakeAggregateRoot` espone metodi pubblici per l’aggiunta e la pulizia degli eventi, mentre la classe `FakeDomainEvent` è un evento minimale, utilizzato per testare il comportamento della coda.

**Sintassi (estratto):**

```csharp
private sealed class FakeAggregateRoot : AggregateRoot
{
    public void AddEvent(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    public void ClearEvents() => ClearDomainEvents();
}
```

**Perché serve:** I metodi di `AggregateRoot` sono protetti, quindi è necessario un wrapper pubblico per testare il comportamento senza compromettere l’incapsulamento della classe.

---

## 5️⃣ Conclusione

`AggregateRoot` è una classe fondamentale nel modello DDD, poiché gestisce gli eventi che rappresentano le modifiche dello stato del dominio. La suite di test ha verificato che:

* La coda degli eventi rispetta l’ordine FIFO degli eventi.
* Gli eventi nulli vengono rifiutati, garantendo la protezione da input non validi.
* Le operazioni di estrazione e pulizia degli eventi sono correttamente gestite, con `PullDomainEvents()` che svuota la coda dopo l'estrazione e `ClearDomainEvents()` che rimuove tutti gli eventi.

L’implementazione di questi test assicura che gli eventi vengano gestiti in modo sicuro e deterministico, proteggendo l'integrità del dominio.

---
