# 📘 **JokesApp — Git Workflow Creation & Initial DevOps Setup (Monorepo)**

Questo documento descrive in modo chiaro e professionale tutti i passaggi necessari per creare la monorepo **JokesApp**, comprendente:

* **JokesApp.Client** (React + Vite)
* **JokesApp.Server** (ASP.NET Core)
* **JokesApp.Tests** (Test automatici)

È il punto di partenza del tuo percorso DevOps: Git, GitHub, CI/CD, monorepo management.

---

## 🟩 1. Scelta dell’architettura: **Monorepo**

La monorepo è la scelta migliore per un progetto moderno che integra frontend e backend.

### ✅ Vantaggi principali

* **CI/CD unica** per frontend e backend
* **Atomicità delle modifiche** (un’unica PR aggiorna FE+BE)
* **Versionamento coerente**
* **Condivisione semplificata di DTO / contratti API**
* **Zero problemi di sincronizzazione tra repository**
* **Onboarding semplice**: un clone = progetto completo

### 📁 Struttura finale della monorepo

```
/JokesApp
   ├── JokesApp.Client/     → React + Vite
   ├── JokesApp.Server/     → ASP.NET Core Web API
   ├── JokesApp.Tests/      → Test automatici
   ├── .gitignore
   ├── JokesApp.slnx
   └── docs/
```

---

## 🟥 2. Rimozione del repository Git e .gitignore interni a `JokesApp.Client`

*(Step critico e necessario)*

Quando Vite crea un nuovo progetto, spesso inizializza automaticamente un repository Git locale:

```
JokesApp.Client/.git/
```

Questo è **incompatibile con la monorepo**, perché crea:

* submodule indesiderati
* conflitti nelle pipeline
* problemi con la storia Git
* errori nei workflow CI/CD

### 🔎 Verifica la presenza del repository interno

```bash
ls -la JokesApp.Client
```

Se vedi `.git/`, prosegui con la rimozione.

### 🛠 Rimozione definitiva del repository interno

Dalla root della monorepo:

```bash
rm -rf JokesApp.Client/.git
```

Questo **non elimina nessun file del progetto**, rimuove solo il repository interno.

---

## 🟧 3. Pulizia dello staging Git

Se avevi già eseguito un `git add .`, Git potrebbe aver tracciato:

* `node_modules/`
* `.vs/`
* file temporanei
* file che dovrebbero essere ignorati
* tracce del vecchio `.git` del client

Per pulire tutto:

```bash
git rm -r --cached .
```

Questo comando:

* rimuove i file dallo staging
* NON elimina i file dal disco
* permette di ripartire con uno staging pulito

---

## 🟨 4. Configurazione del `.gitignore` (monorepo-ready)

Dopo aver ripulito lo staging, aggiorna o conferma il tuo `.gitignore`.

Esempio completo:

```gitignore
################################################
### 🔹 SEZIONE 1 — ASP.NET Core / .NET

# Build outputs
bin/
obj/

# Visual Studio
.vs/
*.user
*.suo

# Logs
*.log

# Configurazioni locali (segreti)
appsettings.Development.json
appsettings.Local.json
launchSettings.json

# Database locali
*.db
*.db-shm
*.db-wal

################################################
### 🔹 SEZIONE 2 — Node / React / Vite

# Node modules
node_modules/
**/node_modules/

# Build frontend
dist/
**/dist/
**/.vite/

# Env frontend
**/.env
**/.env.*

################################################
### 🔹 SEZIONE 3 — Env & Secrets backend

**/*.env

################################################
### 🔹 SEZIONE 4 — Editor & IDE (VS Code, JetBrains)

# VS Code
.vscode/
!.vscode/extensions.json
*.rsuser


# JetBrains / Rider
.idea/
*.iml

################################################
### 🔹 SEZIONE 5 — Sistema operativo / varie

# Windows / macOS
Thumbs.db
Desktop.ini
.DS_Store

################################################
### 🔹 SEZIONE 6 — Varie / Backup

# Evita di versionare backup SQL pesanti
*.backup
**/BackupSQL/

*.swp
struttura.txt
```

---

## 🔄 5. Riesecuzione dello staging (con `.gitignore` attivo)

Ora che il `.gitignore` è corretto:

```bash
git add .
```

Git includerà SOLO:

* file sorgente frontend
* file sorgente backend
* file test
* documentazione
* solution e progetti

Ed escluderà automaticamente:

* `node_modules/`
* `bin/`, `obj/`
* `.env`
* `dist/`
* backup
* `.git` interni
* file locali

---

## 🟪 6. Verifica finale prima della commit

Controlla:

```bash
git status
```

Dovresti vedere solo file sorgente veri e nessuno dei seguenti:

* `JokesApp.Client/.git`
* `node_modules`
* `.vs`
* `bin`
* `obj`
* file `.env`
* `dist/`

Se tutto è pulito → passa allo step finale.

---

## ⬛ 7. Prima commit professionale

```bash
git commit -m "Initial Clean Commit"
```

---

## ⬜ 8. Creazione repository remoto GitHub

Repository senza:

* README
* .gitignore
* LICENSE

Per evitare conflitti.

Poi collega la repo:

```bash
git remote add origin https://github.com/<utente>/JokesApp.git
git branch -M main
git push -u origin main
```

---

## 🟢 9. **Branch Strategy consigliata (GitFlow semplificato)**

I branch principali sono:

* **`main`** → produzione, codice stabile, rilasci
* **`development`** → sviluppo continuo
* **feature/*** → nuove funzionalità
* **fix/*** → bugfix

Per il tuo livello attuale:

👉 **main**
👉 **development**

sono più che sufficienti.

---

## 🔴 10. **Creazione del branch di sviluppo**

Dalla root del progetto, fai:

```bash
git checkout -b development
```

Questo crea e ti sposta sul branch `development`.

---

## 🟠 11. Ora puoi sviluppare SOLO su `development`

Qualunque file tu:

* aggiungi
* modifichi
* aggiorni
* crei

verrà tracciato SOLO nel branch `development`.

Esempio:

### Aggiungi README.md

```bash
git add README.md
git commit -m "Add initial README documentation"
```

Questi commit sono presenti **solo in development**, non in main.

---

## 🟡 12. Quando il codice è stabile → *commit finale su development*

Sempre:

```bash
git add .
git commit -m "Stabilized documentation + structure"
git push -u origin development
```

Adesso il branch viene caricato su GitHub.

---

## 🔵 13. **Apri una Pull Request → merge su main**

Vai su GitHub:

**Pull Requests → New Pull Request**

* **base:** `main`
* **compare:** `development`

Questa PR rappresenta:

* Revisione del codice
* Validazione CI
* Controllo qualità
* Merge sicuro nella versione ufficiale

Quando approvi la PR → GitHub fa il merge.

---

## 🟣 14. Dopo il merge → torni su development per continuare

Il ciclo è:

```
git checkout development
git pull
git checkout -b feature/something   (opzionale)
git add .
git commit
git push
PR → main
```

---

## 🟤 15. **Da ora in poi MAIN non si tocca mai direttamente**

RULE:

👉 **Non fare MAI commit diretti su main.**
Solo PR da development → main.

Questo è ciò che fanno:

* aziende
* team professionali
* DevOps engineer
* sviluppatori senior

---

## ⚫ 16. Ricapitolazione professionale

| Step | Azione                                                  |
| ---- | ------------------------------------------------------- |
| 1    | Crei branch development → `git checkout -b development` |
| 2    | Modifichi file, crei README, aggiorni codice            |
| 3    | Fai commit → `git commit -m "..."`                      |
| 4    | Push su GitHub → `git push -u origin development`       |
| 5    | Apri una PR da development → main                       |
| 6    | GitHub fa il merge (o lo fai tu)                        |
| 7    | Continui a lavorare su development                      |

### **Risultato: stai usando un workflow Git professionale.**

Questo è l’approccio corretto per DevOps, monorepo, CI/CD e collaborazioni future.

---

## ⚪ 17. CI/CD — Continuous Integration (prima pipeline)

*(La pipeline completa sarà trattata nel documento CI/CD.)*

Esempio base:

```yaml
# .github/workflows/ci.yml

name: CI

on:
  push:
    branches: [ "main", "dev", "feature/*" ]
  pull_request:

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - uses: actions/setup-dotnet@v2
      with:
        dotnet-version: 8.0.x

    - name: Backend - Restore & Build
      run: dotnet build JokesApp.slnx --configuration Release

    - name: Run Tests
      run: dotnet test JokesApp.slnx --no-build

    - uses: actions/setup-node@v3
      with:
        node-version: 18

    - name: Frontend - Install & Build
      run: |
        cd JokesApp.Client
        npm install
        npm run build
```

---

## 🧠 18. Roadmap DevOps del progetto

| Step | Materia                      | Perché è fondamentale      |
| ---- | ---------------------------- | -------------------------- |
| 1    | Git + GitHub Workflow        | Base del versionamento     |
| 2    | CI (build & test automatici) | Inizia il DevOps reale     |
| 3    | CD (deploy automatico)       | Distribuzione continua     |
| 4    | Docker                       | Standard moderno           |
| 5    | Kubernetes                   | Scalabilità                |
| 6    | Monitoring & Logging         | Osservabilità              |
| 7    | IaC (Terraform)              | Automazione infrastruttura |
| 8    | DevSecOps                    | Sicurezza completa         |

---

## ⭐ 19. **Documento completato e pronto per essere aggiunto alla repository.**

Si può creare anche:

* **02_MONOREPO_STRUCTURE.md**
* **03_GIT_WORKFLOW.md**
* **04_CI_PIPELINE.md**
* **05_CD_PIPELINE.md**
* **ROADMAP_DEVOPS.md**
* **README.md professionale**
