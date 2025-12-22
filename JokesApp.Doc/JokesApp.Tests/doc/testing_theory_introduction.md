## 📚🧪 Testing Theory Introduction

### 1. Tipi di testing

Nel contesto di un progetto già **buildato**, i tre principali tipi di test sono:

1. **Unit Testing**

   * Scopo: testare singole unità di codice in isolamento (metodi, funzioni, classi).
   * Caratteristiche: rapido, isolato, indipendente dal database o da servizi esterni (di solito si usano *mock* per dipendenze).
   * Esempio: testare che `JokesDbContext` crei correttamente una nuova `Joke` in memoria, oppure che la logica di validazione di `RegisterUserDto` funzioni correttamente.

2. **Integration Testing**

   * Scopo: testare come più componenti interagiscono tra loro (es. il tuo DbContext con il database reale o servizi esterni).
   * Caratteristiche: più lento degli unit test, dipende dalle infrastrutture esterne (DB, API).
   * Esempio: verificare che quando crei un nuovo `ApplicationUser` tramite il repository, venga realmente scritto nel database PostgreSQL e che eventuali constraint vengano rispettati.

3. **End-to-End (E2E) Testing**

   * Scopo: testare l’intera applicazione dal punto di vista dell’utente finale.
   * Caratteristiche: lento, richiede un ambiente completo (frontend + backend + DB), spesso fatto con strumenti tipo Cypress, Playwright o Selenium per il frontend React.
   * Esempio: testare il flusso completo: registrazione utente → login → creazione di una joke → visualizzazione nella lista.

**Conclusione pratica:**

* Se vuoi **fermarti un momento e testare quello che hai già sviluppato**, partire dagli **unit test** è perfetto.
* Dopo puoi aggiungere gli **integration test** per testare il flusso DB-API.
* Gli **E2E test** li farai quando il frontend sarà pronto e integrato.

---

### 2. Unit testing in .NET + React backend

Per il tuo progetto ASP.NET:

* Strumenti principali:

  * **xUnit** o **NUnit** (framework di test .NET)
  * **Moq** (per fare *mocking* di dipendenze come repository o DbContext)
* Dove iniziare:

  * Creare un progetto di test separato (tipo `JokesApp.Tests`) nello stesso solution.
  * Scrivere test per:

    * Validazioni dei DTO (`RegisterUserDto`, `JokeDto` ecc.)
    * Logica dei servizi (repository o business logic)
    * Eventuali metodi statici o helper

---

### 3. Relazione con TDD

* **TDD (Test-Driven Development)** si applica **prima di scrivere il codice**:

  1. Scrivi un test che fallisce (testo cosa vuoi ottenere)
  2. Scrivi il codice minimo per farlo passare
  3. Refactoring

* Quindi:

  * **Unit test, integration test, E2E test**: si usano su codice già sviluppato o in produzione per verificare il comportamento.
  * **TDD**: ti guida nello sviluppo del codice **prima che esista**, aiutandoti a scrivere software più affidabile e modulare.

---

## ✅ 1. **Unit Test**

Un **unit test** verifica il funzionamento di una singola unità di codice, di solito un metodo o una classe, isolandola dal resto del sistema.

### a) Framework più comuni

* **xUnit** (il più moderno e consigliato)
* **NUnit**
* **MSTest** (integrato in Visual Studio)

### b) Creazione di un progetto di test

1. In Visual Studio, clicca su **File → New Project → Unit Test Project**.
2. Scegli il framework (ad esempio `xUnit Test Project`).
3. Aggiungi riferimento al progetto da testare.

### c) Esempio con xUnit

Supponiamo di avere una classe:

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

Il test unitario sarà:

```csharp
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calc = new Calculator();
        int a = 2, b = 3;

        // Act
        int result = calc.Add(a, b);

        // Assert
        Assert.Equal(5, result);
    }
}
```

* `[Fact]` indica un test singolo.
* `Assert.Equal(expected, actual)` verifica il risultato.

---

## 🔗 2. **Integration Test**

Gli **integration test** verificano come più componenti interagiscono tra loro (es. controller e database).

### a) Esempio semplice con ASP.NET Core

Supponiamo tu abbia un **controller**:

```csharp
[ApiController]
[Route("[controller]")]
public class JokesController : ControllerBase
{
    private readonly IJokesService _service;

    public JokesController(IJokesService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public IActionResult GetJoke(int id)
    {
        var joke = _service.GetJoke(id);
        if (joke == null) return NotFound();
        return Ok(joke);
    }
}
```

Un integration test potrebbe usare **WebApplicationFactory**:

```csharp
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

public class JokesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public JokesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetJoke_ReturnsOk()
    {
        var response = await _client.GetAsync("/Jokes/1");
        response.EnsureSuccessStatusCode();
    }
}
```

Questo approccio fa partire il tuo server ASP.NET Core in memoria e ti permette di testare le API senza deploy reale.

---

## 📝⚙️ 3. **Test Driven Development (TDD)**

In .NET è possibile applicare il **TDD**:

1. Scrivi prima un test che fallisce.
2. Implementa il codice necessario per farlo passare.
3. Refactoring e ripetizione.

---

## 🔎🧪 4. **Altri tipi di test**

* **Functional/UI test**: con strumenti come **Selenium** o **Playwright**.
* **Performance test**: con **BenchmarkDotNet** per misurare performance di metodi.

---