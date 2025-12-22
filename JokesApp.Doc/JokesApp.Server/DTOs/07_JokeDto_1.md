# Documentazione sui DTO in ASP.NET Core

## 1. Definizione e scopo dei DTO

Un **DTO (Data Transfer Object)** è un oggetto che viene utilizzato **esclusivamente per il trasferimento dei dati tra client e server**, separando il modello di dominio interno dalla rappresentazione esposta dall’API.

### Obiettivi principali:

1. **Separazione delle responsabilità**: il modello di dominio (`Joke.cs`) gestisce la logica e i dati interni, il DTO espone solo ciò che serve al client.
2. **Sicurezza**: previene l’esposizione di campi sensibili del database (es. password, flag interni, proprietà non pubbliche).
3. **Validazione lato API**: permette di validare i dati prima che arrivino al database, generando messaggi di errore chiari.
4. **Flessibilità e manutenibilità**: l’evoluzione del modello interno non obbliga a cambiare il client se i DTO restano invariati.
5. **Ottimizzazione del payload**: consente di inviare solo i dati strettamente necessari, riducendo il traffico di rete.

---

## 2. Differenza tra DTO e modello di dominio

| Aspetto         | Modello di dominio (`Joke.cs`)                                                                                       | DTO (`JokeDto`)                                                                      |
| --------------- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| **Scopo**       | Rappresenta l’entità come viene salvata nel database. Può contenere logica di business e relazioni con altre entità. | Rappresenta i dati trasportati tra client e server. Non contiene logica di business. |
| **Campi**       | Tutti i campi interni necessari al funzionamento del DB (es. flag, timestamp, relazioni, ID interni).                | Solo i campi che il client deve leggere o scrivere. Può omettere campi sensibili.    |
| **Validazione** | Logica interna e vincoli del DB.                                                                                     | Vincoli lato API tramite DataAnnotations (es. `[Required]`, `[MaxLength]`).          |
| **Sicurezza**   | Può contenere dati sensibili.                                                                                        | Espone solo ciò che è sicuro mostrare.                                               |
| **Mutabilità**  | Può essere modificato dal server secondo la logica di business.                                                      | Lato client, alcune proprietà possono essere di sola lettura.                        |

**Esempio:**

* Modello dominio:

```csharp
public class Joke
{
    public int Id { get; set; }
    public string Question { get; set; }
    public string Answer { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string AuthorId { get; set; }   
    public string AuthorName { get; set; } 
    public bool IsDeleted { get; set; }    // campo interno non esposto
}
```

* DTO:

```csharp
public class JokeDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "La domanda è obbligatoria.")]
    [MaxLength(200, ErrorMessage = "La domanda non può superare i 200 caratteri.")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "La risposta è obbligatoria.")]
    [MaxLength(500, ErrorMessage = "La risposta non può superare i 500 caratteri.")]
    public string Answer { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? AuthorName { get; set; }
    public string? AuthorId { get; set; }
}
```

---

## 3. Composizione di un DTO

Un DTO si compone di:

1. **Campi obbligatori inviati dal client**

   * Annotazioni `[Required]` e `[MaxLength]`.
   * Esempio: `Question`, `Answer`.

2. **Campi opzionali**

   * Nullable (`string?`) o con valori predefiniti.
   * Esempio: `AuthorName?`, `AuthorId?`.

3. **Campi di sola lettura**

   * Popolati dal server, non inviati dal client.
   * Esempio: `CreatedAt`, `UpdatedAt`.

4. **Campi di visualizzazione**

   * Aiutano a mostrare informazioni derivate o aggiuntive al client.
   * Esempio: `AuthorName`.

---

## 4. Costruzione di un DTO

**Passaggi consigliati:**

1. Creare la cartella `DTOs` nel progetto lato server.
2. Creare la classe con nome `<NomeEntità>Dto`.
3. Definire le proprietà pubbliche secondo gli obiettivi del DTO.
4. Aggiungere validazioni lato server con **DataAnnotations**.
5. Gestire campi readonly lato client separando input e output se necessario.
6. Mappare DTO ↔ modello di dominio manualmente o tramite strumenti come **AutoMapper**.

---

## 5. Best Practice per i DTO

* Separare **DTO di input** e **DTO di output** per chiarezza:

  * `CreateJokeDto` → usato in POST.
  * `UpdateJokeDto` → usato in PUT.
  * `JokeResponseDto` → usato in GET.
* Non includere logica di business.
* Validare lato server con messaggi chiari.
* Esporre solo ciò che è sicuro.
* Preferire valori non null per campi obbligatori (`string.Empty` invece di `null`).
* Mappare in modo chiaro tra DTO e modello.

---

## 6. Esempio di utilizzo in un controller ASP.NET Core

```csharp
[HttpPost]
public async Task<IActionResult> CreateJoke(CreateJokeDto jokeDto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var joke = new Joke
    {
        Question = jokeDto.Question,
        Answer = jokeDto.Answer,
        CreatedAt = DateTime.UtcNow,
        AuthorId = jokeDto.AuthorId,
        AuthorName = jokeDto.AuthorName
    };

    _context.Jokes.Add(joke);
    await _context.SaveChangesAsync();

    var responseDto = new JokeResponseDto
    {
        Id = joke.Id,
        Question = joke.Question,
        Answer = joke.Answer,
        CreatedAt = joke.CreatedAt
    };

    return CreatedAtAction(nameof(GetJoke), new { id = joke.Id }, responseDto);
}
```

* Il DTO di input (`CreateJokeDto`) viene validato prima di raggiungere il database.
* Il modello dominio (`Joke`) gestisce la persistenza.
* Il DTO di output (`JokeResponseDto`) restituisce solo i dati consentiti al client.

---