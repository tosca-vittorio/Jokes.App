# 📘 06b_JokeWasCreated.md

### *JokeWasCreated — Domain Event*

Evento di dominio che rappresenta il fatto: **una nuova barzelletta è stata creata**.

Nel dominio attuale l’identificatore della barzelletta (`JokeId`) è generato nel Domain Layer,
quindi l’evento trasporta un Id **reale** (non placeholder).

---

## 1) Quando viene emesso

L’Aggregate `Joke` emette `JokeWasCreated` quando una creazione va a buon fine e le invarianti sono rispettate:
- `Question` valida
- `Answer` valida
- autore valido (`UserId`)
- timestamp di creazione in UTC

---

## 2) Payload dell’evento

- `JokeId JokeId` — Id tipizzato della barzelletta (obbligatorio, non vuoto)
- `UserId AuthorId` — Id tipizzato dell’autore (obbligatorio, non vuoto)
- `QuestionText Question` — testo domanda (obbligatorio, non vuoto)
- `AnswerText Answer` — testo risposta (obbligatorio, non vuoto)
- `DateTime CreatedAt` — istante di creazione (UTC)

Inoltre, come per ogni `DomainEvent`, è presente:
- `OccurredOn` (UTC) — istante di occorrenza dell’evento

---

## 3) Regole e invarianti

L’evento è considerato valido se:
- `JokeId` non è `Empty`
- `AuthorId` non è `Empty`
- `Question` e `Answer` non sono `Empty`
- `CreatedAt` non è `default` ed è normalizzato a UTC

---

## 4) Note DDD/Clean

- Nessuna dipendenza da framework (HTTP, EF Core, Identity, logging).
- Payload composto solo da Value Object e timestamp di dominio.
- Emesso dall’Aggregate e dispatchato dall’Application Layer (eventing/outbox) dopo la persistenza.

---

