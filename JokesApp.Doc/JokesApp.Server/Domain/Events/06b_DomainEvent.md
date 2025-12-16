# 📘 06b_DomainEvent.md

### *DomainEvent — Base class per gli eventi di dominio*

`DomainEvent` è una classe astratta che implementa `IDomainEvent` e fornisce una
implementazione comune del timestamp di occorrenza (`OccurredOn`).

---

## 1) Scopo

Fornire una base riusabile per tutti gli eventi di dominio concreti:
- standardizza la gestione del timestamp;
- riduce duplicazioni nei singoli eventi (`JokeWasCreated`, `JokeWasUpdated`, ecc.);
- mantiene il Domain Layer puro e indipendente da infrastruttura.

---

## 2) Proprietà

- `DateTime OccurredOn`  
  Timestamp dell’evento, valorizzato in **UTC** al momento della creazione.

---

## 3) Creazione dell’evento

Il costruttore protetto assegna automaticamente:

- `OccurredOn = DateTime.UtcNow`

Questo garantisce coerenza temporale e ordinabilità degli eventi.

---

## 4) Linee guida

- Gli eventi concreti devono contenere solo dati di dominio (Value Object, id tipizzati, ecc.).
- Nessuna dipendenza da framework (HTTP, EF, Identity, logging).

---

## 5) Uso tipico

Ogni evento concreto:
- eredita da `DomainEvent`;
- aggiunge il payload necessario (es. `JokeId`, `UserId`, `QuestionText`, `AnswerText`, ecc.);
- viene creato e aggiunto dall’Aggregate in seguito a un’operazione valida di dominio.

> Gli snippet sono illustrativi e non vincolanti.
