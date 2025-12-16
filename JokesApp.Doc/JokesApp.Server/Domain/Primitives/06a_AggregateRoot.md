# 📘 06a_AggregateRoot.md

### *AggregateRoot — Primitive di dominio per Aggregate Root*

`AggregateRoot` è una primitive del Domain Layer che fornisce una base comune per gli Aggregate Root DDD.
Il suo compito è gestire **la coda degli eventi di dominio** generati durante operazioni valide, mantenendo il dominio puro e disaccoppiato dall’infrastruttura. 

> Questa classe non si occupa di “pubblicare” eventi: li **accumula**. La pubblicazione avviene nell’Application Layer (o Infrastructure) dopo la persistenza.

---

## 1) Obiettivi di design (DDD/Clean)

- Centralizzare la gestione dei **Domain Events** (DRY) evitando duplicazioni nei singoli aggregate.
- Permettere all’aggregate di produrre eventi come “fatti” del dominio, senza dipendenze da framework.
- Fornire un’API minimale e consistente per:
  - accodare (`AddDomainEvent`)
  - consultare (`DomainEvents`)
  - estrarre+svuotare (`PullDomainEvents`)

---

## 2) Event queue: struttura e responsabilità

### 2.1 Campo interno
L’implementazione mantiene una lista interna `_domainEvents` (mutabile) non esposta direttamente, così da preservare il controllo del dominio sulla coda eventi. 

### 2.2 Vista read-only: `DomainEvents`
`DomainEvents` espone una collezione **read-only** mantenuta in cache, evitando allocazioni ripetute ad ogni accesso.
È pensata per ispezione (debug/test/logica applicativa), non per modifiche esterne. 
---

## 3) API dell’AggregateRoot

### 3.1 `AddDomainEvent(IDomainEvent domainEvent)` (protected)
Metodo protetto usato dagli aggregate per accodare un evento:
- rifiuta `null` (`ArgumentNullException`);
- aggiunge l’evento alla lista interna. 

**Quando usarlo:** quando un’operazione di dominio va a buon fine e vuoi registrare un fatto significativo (es. `JokeWasCreated`, `JokeWasLiked`, …).

### 3.2 `PullDomainEvents()` (public)
Estrae gli eventi correnti e **svuota** la coda:
- se non ci sono eventi, ritorna `Array.Empty<IDomainEvent>()` (evita allocazioni);
- altrimenti crea uno snapshot read-only e poi pulisce la coda tramite `ClearDomainEvents()`. 

**Quando usarlo:** tipicamente nell’Application Layer dopo la persistenza (o dopo un’operazione applicativa riuscita), per:
1) ottenere l’elenco degli eventi prodotti dall’aggregate;
2) pubblicarli tramite mediator/outbox/message bus;
3) garantire che la coda non venga ripubblicata accidentalmente.

### 3.3 `ClearDomainEvents()` (protected)
Pulisce la coda interna. È usato internamente (e può essere utile in test avanzati o scenari particolari), ma l’uso standard è tramite `PullDomainEvents()`. 

---

## 4) Regole d’uso (importanti)

- Il Domain Layer **non deve** inviare eventi su bus, non deve loggare su infrastrutture esterne, non deve invocare servizi di trasporto.
- L’Application Layer è responsabile di:
  - orchestrare la persistenza;
  - estrarre eventi con `PullDomainEvents()`;
  - pubblicarli con il meccanismo scelto (in-process mediator, outbox, message bus). 

---

## 5) Esempio illustrativo (non vincolante)

```csharp
public sealed class Joke : AggregateRoot
{
    public void Like()
    {
        // mutate state...
        AddDomainEvent(new JokeWasLiked(jokeId, likesAfterChange));
    }
}

// Application Layer (pseudo):
var events = joke.PullDomainEvents();
// persist aggregate...
// publish events...
```