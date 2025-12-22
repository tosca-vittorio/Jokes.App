# 🚀📘 **JokesApp — 1. Git Workflow Creation & Bootstrap (Monorepo)**

Questa guida descrive i passaggi **one-time** e **recurring** per creare correttamente la monorepo JokesApp (React + ASP.NET Core + Tests) e pubblicarla su GitHub senza errori tipici (repo annidate, staging sporco, file segreti versionati). È il punto di partenza del percorso DevOps: Git, GitHub, CI/CD, monorepo management.

Questo documento contiene sia:

### PARTE 1:
- **BOOTSTRAP**: creazione/inizializzazione monorepo   [ONE-TIME]
- **WORKFLOW**: lavoro quotidiano (branch/PR/commit)   [RECURRING] 

### PARTE 2:
- **CI/CD**: pipeline e basi DevOps            
> **Status**: TO-BE (non ancora implementata in questo documento).
---

## ⬜ 1. Scelta Architetturale: Monorepo 

La monorepo è la scelta migliore per un progetto moderno che integra frontend e backend.

### ✅ 1.1 Vantaggi principali
- **Zero problemi di sincronizzazione tra repository**
- **Condivisione semplificata di DTO / contratti API**
- **CI/CD unica** per frontend e backend (build/test FE+BE nella stessa PR)
- **Atomicità delle modifiche** (una PR può toccare contratti + implementazioni)
- **Onboarding semplice** (un clone = progetto completo)
- **Storico coerente** (una timeline Git)
- **Versionamento coerente**

### 📁 1.2 Struttura finale della monorepo

```text
/JokesApp
├── JokesApp.Client/      → React + Vite
├── JokesApp.Server/      → ASP.NET Core Web API
├── JokesApp.Tests/       → Test automatici
├── JokesApp.Doc/
├── .github/workflows/
├── .gitignore
└── JokesApp.slnx
```

---

## 🟥 2. Rimozione del repository Git e .gitignore interni a `JokesApp.Client`

*(Step critico e necessario)*

Quando strumenti frontend inizializzano Git automaticamente, può comparire:
`JokesApp.Client/.git/`

Questo è **incompatibile con la monorepo**, perché crea:

* submodule indesiderati
* conflitti nelle pipeline
* problemi con la storia Git
* errori nei workflow CI/CD

### 🔎 2.1 Verifica
Dalla root della monorepo verifica la presenza del repository interno. 
Se vedi `.git/`, prosegui con la rimozione.

#### 2.1.1 **Linux/macOS (bash):**
```bash
ls -la JokesApp.Client
```

#### 2.1.2 **Windows (PowerShell):**

```powershell
Get-ChildItem -Force .\JokesApp.Client
```

### 2.2 🛠 Rimozione

Questo **non elimina nessun file del progetto**, rimuove solo il repository interno.
Dalla root della monorepo:

#### 2.2.1 **Linux/macOS:**

```bash
rm -rf JokesApp.Client/.git
```

#### 2.2.2 **Windows (PowerShell):**

```powershell
Remove-Item -Recurse -Force .\JokesApp.Client\.git
```

---

## 🟩 3. Inizializzare Git nella Root Project

Dalla root del progetto:

```bash
git init -b main
```

---

## 🟧 4. Pulizia dello staging Git 

*RECOVERY:* Se erroneamente si è fatto un `git add .`, e Git ha tracciato file non desiderati, è possibile pulire lo staging se è stata tracciata roba da ignorare:

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

Poi ricostruisci lo staging pulito dopo aver sistemato `.gitignore`.

---

## ⬛ 5. Configurazione del `.gitignore` monorepo-ready

Dopo aver ripulito lo staging, aggiorna o conferma il tuo `.gitignore`.

**Obiettivo**: ignorare artefatti e segreti senza rompere l’esperienza dev.

```gitignore
################################################
### .NET / ASP.NET Core

# Build outputs
**/bin/
**/obj/

# Visual Studio
**/.vs/
*.user
*.suo
*.csproj.user

# Test / Coverage
**/TestResults/
**/coverage/
**/coverageReports/

# Logs
*.log

# Local configs / secrets
**/appsettings.Development.json
**/appsettings.Local.json
**/Properties/launchSettings.json

# Local databases
*.db
*.db-shm
*.db-wal

# NuGet artifacts (optional but common)
*.nupkg
*.snupkg

################################################
### Node / React / Vite

# Dependencies
**/node_modules/

# Build / cache
**/dist/
**/.vite/
**/.eslintcache

# Node logs
npm-debug.log*
yarn-debug.log*
yarn-error.log*
pnpm-debug.log*

################################################
### Env & Secrets (global)

**/.env
**/.env.*

################################################
### IDE / Editor

# VS Code
.vscode/
!.vscode/extensions.json

# Visual Studio extra
*.userosscache
*.sln.docstates


# JetBrains / Rider
.idea/
*.iml
*.DotSettings.user

################################################
### OS

Thumbs.db
Desktop.ini
.DS_Store

################################################
### Varie / Backup

*.backup
**/BackupSQL/
*.swp

### Publish / artifacts
**/publish/
**/artifacts/

################################################
# File di lavoro locali
struttura.txt
```

---

## 🔄 6.  Primo staging pulito e “Initial Clean Commit” (ONE-TIME)

Ora che il `.gitignore` è corretto:

```bash
git add .
git status
git commit -m "Initial clean commit"
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

> `git add .` qui è accettabile perché è il bootstrap. Dopo, si passa a commit mirati.

### 6.1 Regola “anti-caos”

> Regola pratica:
> - `git add .` → solo nel BOOTSTRAP (primo commit pulito) o in casi eccezionali controllati.
> - altrimenti → commit, piccoli, verificabili e mirati (`git add <file>`) soprattutto sulla documentazione.

Risultato: storico pulito, debugging semplice, PR review facile.

### 6.2 Commit message 

Per restare coerente con lo stile:

* `(docs): ...`
* `(feat): ...`
* `(fix): ...`
* `(refactor): ...`
* `(test): ...`
* `(chore): ...`

Esempi:

* `(docs): update doc hub`
* `(docs): finalize architecture`
* `(fix): handle invalid joke payload`

---

## 🟪 7. Creazione remote GitHub e push iniziale

1. Crea repo remoto **vuoto** per evitare conflitti (senza README/.gitignore/LICENSE).
2. Collega e push:

```bash
git remote add origin https://github.com/tosca-vittorio/JokesApp.git
git push -u origin main
```

Adesso il branch viene caricato su GitHub.

---

## 🟨 8. Branch Strategy consigliata: creare branch `development`

I branch principali sono:

* **`main`** → (release): produzione, codice stabile, rilasci 
* **`development`** → sviluppo continuo / integrazione continua

- opzionale:
  - `feature/<topic>` → nuove funzionalità
  - `fix/<topic>` → bugfix / `hotfix/<topic>` → correzioni

Regola: **mai commit diretti su main**. Solo PR.

Per il livello attuale:

👉 **main** →  `main` stabile
👉 **development** → `development` per lavoro quotidiano

sono più che sufficienti.

Dalla root del progetto:
```bash
git checkout -b development
```

Questo crea e ti sposta sul branch `development`.
Ora puoi sviluppare SOLO su `development`; ogni file che si:

* aggiunge
* modifica
* aggiorna
* crea

verrà tracciato SOLO nel branch `development`, non in `main` branch.

---

## 🟫 9. Impostazioni GitHub consigliate (ONE-TIME, ma fondamentali)

### 9.1 Branch protection (main)

* blocca push diretto su `main`
* richiedi PR per merge
* richiedi “status checks” (CI) prima del merge

### 9.2 Protezione ambienti (quando si farà CD)

* `staging` e `production` come environments
* approvazione manuale su `production`

---

## 🔳 10. Sicurezza minima (ONE-TIME)

* non versionare segreti (`.env`, connection string reali, token)
* usa GitHub Secrets/Environments per CI/CD
* opzionale: aggiungi un “secret scanning” (anche in futuro)

---

## 🔲 11. Pull Request → Merge

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
Dopo il merge → torni su development per continuare

Il ciclo è:

```bash
git checkout development
git pull
git checkout -b feature/something (opzionale)
git add <file>
git commit
git push
PR → main
```

**Da ora in poi MAIN non si tocca mai direttamente**

RULE:

👉 **Non fare MAI commit diretti su main.**
Solo PR da development → main.

### Ricapitolazione: **workflow Git professionale.**

| Step | Azione                                                    |
| ---- | -------------------------------------------------------   |
| 1    | Crei branch `development` → `git checkout -b development` |
| 2    | Modifichi file, crei README, aggiorni codice              |
| 3    | Fai commit → `git commit -m "..."`                        |
| 4    | Push su GitHub → `git push -u origin development`         |
| 5    | Apri una PR da development → main                         |
| 6    | GitHub → fa il merge                                      |
| 7    | Continui a lavorare su `development`                      |

> Approccio corretto per DevOps, monorepo, CI/CD e collaborazioni future.

### Quando si apre una Pull Request?

Apri PR quando:

* la feature è completa **o** c’è uno “slice” verificabile,
* CI passa,
* la doc “owner” è aggiornata (se necessario).

Checklist minima PR:

* build/test ok
* cambiamenti descritti
* eventuale doc aggiornata (owner)

---
