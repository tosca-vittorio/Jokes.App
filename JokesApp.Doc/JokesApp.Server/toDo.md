Alcune direzioni naturali di estensione del modello JOKE:

* Aggiungere una **categoria** o tag per le barzellette (`Category`, `Tags`).
* Introdurre un campo **Rating** o **LikeCount** con metodi di dominio dedicati per l’upvote.
* Gestire uno stato di **visibilità** (es. `IsPublic`, `IsDeleted` soft delete).
* Spostare le classi di errori in una cartella/logica `Domain/Errors` per aderire in modo più esplicito a un approccio DDD-light.
* Integrare una logica di **versioning** delle joke, se necessario.

---