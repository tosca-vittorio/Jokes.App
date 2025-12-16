# 📘 06b_JokeWasUpdated.md

### *JokeWasUpdated — Domain Event*

Evento di dominio che rappresenta il fatto: **una barzelletta è stata aggiornata**.
Registra i nuovi valori e il timestamp dell’aggiornamento.

---

## 1) Quando viene emesso

L’Aggregate `Joke` emette `JokeWasUpdated` quando un’operazione di aggiornamento va a buon fine e le invarianti sono rispettate:
- `JokeId` valido e inizializzato
- `Question` valida
- `Answer` valida
- timestamp `UpdatedAt` valido (UTC)

---

## 2) Payload dell’evento

- `JokeId JokeId` — identificatore tipizzato della barzelletta aggiornata (obbligatorio, non vuoto)
- `QuestionText NewQuestion` — nuova domanda (obbligatoria, non vuota)
- `AnswerText NewAnswer` — nuova risposta (obbligatoria, non vuota)
- `DateTime UpdatedAt` — istante di aggiornamento (UTC)

Inoltre, come per ogni `DomainEvent`:
- `OccurredOn` (UTC) — istante di occorrenza dell’evento

---

## 3) Regole e invarianti

L’evento è valido se:
- `JokeId` non è `Empty`
- `NewQuestion` e `NewAnswer` non sono `Empty`
- `UpdatedAt` non è `default` ed è normalizzato a UTC

---

## 4) Note DDD/Clean

- Nessuna dipendenza da framework (HTTP, EF Core, Identity, logging).
- Payload composto solo da Value Object e timestamp.
- Emesso dall’Aggregate e dispatchato dall’Application Layer dopo la persistenza (eventing/outbox).

> Gli snippet sono illustrativi e non vincolanti.
