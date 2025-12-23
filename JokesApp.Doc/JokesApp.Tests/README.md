# ✅ **Il progetto corretto per creare i tuoi test xUnit è:

➡️ “App console C# (.NET)”**

Non devi scegliere:

* **progetto vuoto (.NET Framework)** → è vecchio, non usa .NET 8, non compatibile con EF Core moderno
* **ASP.NET Core vuoto** → è un progetto Web, non un progetto di test
* **ASP.NET Core empty template** → NON serve per i test

### 🔥 DEVI usare **solo e soltanto**:

⭐ **App Console C# (.NET)**

(progetto console moderno che usa .NET 8)

Aggiungi un nuovo progetto di test alla solution:

```bash
dotnet new xunit -n JokesApp.Tests
```

---

## 🎯 Perché è il progetto giusto?

Perché un progetto di test:

* è una normale *class library* o console app senza entrypoint
* NON deve essere ASP.NET
* NON deve essere Web
* NON deve essere Framework vecchio
* deve solo poter contenere classi C# e dipendenze xUnit

Il progetto Console .NET è perfetto perché:

* ti crea automaticamente un `.csproj` compatibile con .NET 8
* non include nulla che interferisce con xUnit
* può essere trasformato facilmente in progetto di test
* il Test Explorer lo riconoscerà automaticamente

---

## Struttura dei test

Si consiglia di organizzare i test così:

```
JokesApp.Tests/
 ├─ Domain/
 │   ├─ ValueObjects/
 │   ├─ Aggregates/
 │   └─ Primitives/
 ├─ Models/
 └─ Data/
```

---

## 📂 Documentazione di riferimento

* 📄 **Matrice dominio & piano file**: `JokesApp.Doc/JokesApp.Tests/Domain/01_DomainTestMatrix.md`
* 📄 **Test modello Joke**: `JokesApp.Doc/JokesApp.Tests/Model/03_JokeTest.md`
* 📄 **Test modello ApplicationUser**: `JokesApp.Doc/JokesApp.Tests/Model/04_ApplicationUserTest.md`
* 📄 **Validator email**: `JokesApp.Doc/JokesApp.Tests/Domain/Attributes/04_CustomEmailAttributeTest.md`

Questi documenti indicano **cosa** testare (matrice) e **come** mantenere le convenzioni attuali (messaggi di dominio, AAA, FluentAssertions).

---

## 🔧 Procedura esatta che devi fare: nel **Package Manager**

### 1️⃣ In Visual Studio:

**Nuovo Progetto → App console C# (.NET)**
Nome: `JokesApp.Tests`

### 2️⃣ Elimina `Program.cs`

### 3️⃣ Aggiungi pacchetti NuGet:

#### **Microsoft.NET.Test.Sdk**

```
Install-Package Microsoft.NET.Test.Sdk
```

#### **xUnit**

```
Install-Package xunit
```

#### **xUnit runner (per farli vedere al Test Explorer)**

```
Install-Package xunit.runner.visualstudio
```

#### **Pacchetti Consigliati**:

```bash
dotnet add JokesApp.Tests package FluentAssertions
dotnet add JokesApp.Tests package Microsoft.EntityFrameworkCore.InMemory
```

### 4️⃣ Aggiungi un riferimento al tuo server:

Right click su progetto →
**Add → Project Reference → JokesApp.Server**

```bash
dotnet add JokesApp.Tests reference JokesApp.Server
```

### 5️⃣ Aggiungi il primo test:

`ExampleTests.cs`

```csharp
using Xunit;

public class ExampleTests
{
    [Fact]
    public void Test1()
    {
        Assert.True(1 == 1);
    }
}
```

---

## 📌 Perché NON devi usare “ASP.NET Core vuoto”?

Perché:

* creerebbe un secondo backend, inutile e fuorviante
* aggiungerebbe dipendenze che non servono ai test
* è un progetto Web, non un progetto di test
* i test NON devono essere Web app

---

## 📌 Perché NON devi usare “Progetto vuoto (.NET Framework)”?

Perché:

* usa .NET Framework (vecchio)
* EF Core moderno non lo supporta
* il tuo progetto è .NET 8 → incompatibile
* xUnit moderno non funziona con Framework legacy

---

## 🎯 Conclusione

👉 **Il progetto giusto è solo: App Console C# (.NET)**
E poi lo trasformi in progetto xUnit installando le dipendenze.

---
