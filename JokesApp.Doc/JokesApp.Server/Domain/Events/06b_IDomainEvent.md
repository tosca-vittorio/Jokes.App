# 📘 06b_IDomainEvent.md

### *IDomainEvent — Contratto base per gli eventi di dominio*

`IDomainEvent` definisce il contratto minimo che ogni **Domain Event** deve rispettare nel Domain Layer.
È volutamente semplice per mantenere il dominio puro (DDD/Clean) e senza dipendenze infrastrutturali.

---

## 1) Scopo

Un **Domain Event** rappresenta un fatto significativo già avvenuto nel dominio (es. una barzelletta creata, aggiornata, ecc.).
Gli eventi permettono di:
- disaccoppiare le reazioni a un cambiamento dallo stato dell’Aggregate;
- abilitare processi asincroni o cross-cutting (notifiche, audit, integrazioni) fuori dal dominio;
- mantenere un tracciamento temporale coerente dei fatti.

---

## 2) Contratto

### Proprietà obbligatoria
- `DateTime OccurredOn`  
  Timestamp di occorrenza dell’evento.

> Regola: `OccurredOn` è **atteso in UTC** (es. `DateTime.UtcNow`) per evitare ambiguità tra timezone e per garantire ordinamenti consistenti.

---

## 3) Linee guida DDD/Clean

- Un Domain Event **non** deve contenere dipendenze da:
  - HTTP / controller / DTO,
  - EF Core / ORM,
  - logging framework,
  - servizi esterni.
- Il payload dell’evento deve contenere solo dati di dominio utili:
  - identificatori tipizzati (Value Object),
  - Value Object rilevanti,
  - eventuali timestamp.

---

## 4) Uso tipico

Gli eventi vengono:
1. creati **dentro l’Aggregate** nel momento in cui un’invariante/operazione di dominio va a buon fine;
2. aggiunti alla collezione di Domain Events dell’Aggregate (es. `AddDomainEvent(...)`);
3. pubblicati/dispatchati dall’Application Layer dopo la persistenza (unit-of-work).

> Gli snippet sono esempi illustrativi, non vincolanti.

---

## 5) Implementazioni

Ogni evento concreto (es. `JokeWasCreated`, `JokeWasUpdated`, ecc.) implementa `IDomainEvent` e valorizza:
- `OccurredOn = DateTime.UtcNow`.

---

## 6) Motivazione del design

Mantenere `IDomainEvent` minimale:
- riduce l’accoppiamento tra dominio e infrastruttura,
- rende gli eventi facili da serializzare o proiettare in outbox in layer superiori,
- permette evoluzioni (nuovi eventi) senza modificare il contratto.
