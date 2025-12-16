# 📘 06b_JokeWasLiked.md

### *JokeWasLiked — Domain Event*

Evento di dominio che rappresenta il fatto: **è stato aggiunto un like a una barzelletta**.
Registra l’identificatore della barzelletta e il nuovo totale dei like dopo l’operazione.

---

## 1) Quando viene emesso

L’Aggregate `Joke` emette `JokeWasLiked` quando l’operazione di aggiunta like va a buon fine e lo stato risultante è coerente.

---

## 2) Payload dell’evento

- `JokeId JokeId` — identificatore tipizzato della barzelletta (obbligatorio, non vuoto)
- `int LikesAfterChange` — totale dei like dopo l’operazione (non negativo)

Inoltre, come per ogni `DomainEvent`:
- `OccurredOn` (UTC) — istante di occorrenza dell’evento

---

## 3) Regole e invarianti

L’evento è valido se:
- `JokeId` non è `Empty`
- `LikesAfterChange >= 0`

Se una regola viene violata, viene generata `DomainValidationException` con messaggi centralizzati in `JokeErrorMessages`.

---

## 4) Note DDD/Clean

- Nessuna dipendenza da framework (HTTP, EF Core, Identity, logging).
- Payload composto solo da dati di dominio (Value Object + contatore).
- L’evento viene creato dall’Aggregate e poi dispatchato dall’Application Layer (eventing/outbox) dopo la persistenza.

> Gli snippet sono illustrativi e non vincolanti.
