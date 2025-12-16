using System;
using System.Collections.Generic;
using JokesApp.Server.Domain.ValueObjects;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;


namespace JokesApp.Server.Domain.Entities
{
    /// <summary>
    /// Entità di dominio che rappresenta un utente dell'applicazione.
    /// Non dipende da ASP.NET Identity, da DataAnnotations o dalla serializzazione.
    /// Utilizza Value Object per garantire le regole di validazione.
    /// </summary>
    public class ApplicationUser
    {
        #region Properties

        /// <summary>
        /// Identificativo tipizzato dell'utente.
        /// </summary>
        public UserId Id { get; private set; }

        /// <summary>
        /// Nome visuale mostrato all'interno dell'applicazione.
        /// </summary>
        public DisplayName DisplayName { get; private set; }

        /// <summary>
        /// URL dell'avatar dell'utente (opzionale).
        /// Usa <see cref="AvatarUrl.Empty"/> per rappresentare l'assenza di avatar.
        /// </summary>
        public AvatarUrl AvatarUrl { get; private set; }

        /// <summary>
        /// Indirizzo email dell'utente.
        /// </summary>
        public EmailAddress Email { get; private set; }

        /// <summary>
        /// Data di creazione dell'account (UTC).
        /// </summary>
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Data di ultima modifica del profilo (UTC).
        /// Null se il profilo non è mai stato aggiornato.
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Collezione delle barzellette create dall'utente.
        /// Relazione uno-a-molti con <see cref="Joke"/>.
        /// </summary>
        public ICollection<Joke> Jokes { get; private set; } = new List<Joke>();

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore protetto richiesto dagli strumenti di persistenza (es. EF Core).
        /// Non deve essere utilizzato direttamente nel codice di dominio.
        /// </summary>
        protected ApplicationUser()
        {
            Id = UserId.Empty;
            DisplayName = DisplayName.Empty;
            AvatarUrl = AvatarUrl.Empty;
            Email = EmailAddress.Empty;
            // CreatedAt è inizializzato tramite l'inizializzatore della proprietà.
        }

        /// <summary>
        /// Costruttore principale del dominio per la creazione di un nuovo utente.
        /// I Value Object garantiscono tutte le regole di validazione.
        /// </summary>
        /// <param name="id">Identificatore tipizzato dell'utente.</param>
        /// <param name="displayName">Nome visuale valido.</param>
        /// <param name="email">Indirizzo email valido.</param>
        /// <param name="avatarUrl">
        /// URL dell'avatar; può essere <see cref="AvatarUrl.Empty"/> per indicare nessun avatar.
        /// </param>
        public ApplicationUser(UserId id, DisplayName displayName, EmailAddress email, AvatarUrl avatarUrl)
        {
            if (id.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(id));
            }

            if (displayName.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.DisplayNameRequired,
                    nameof(displayName));
            }

            if (email.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailRequired,
                    nameof(email));
            }

            // AvatarUrl può essere Empty (assenza avatar), quindi è ammesso.
            Id = id;
            DisplayName = displayName;
            Email = email;
            AvatarUrl = avatarUrl;
        }

        #endregion

        #region Domain behavior

        /// <summary>
        /// Verifica che l'entità si trovi in uno stato consistente rispetto
        /// alle principali invarianti di dominio (Id, DisplayName, Email).
        /// Può essere utilizzato in scenari di import, test o debug.
        /// </summary>
        /// <exception cref="DomainValidationException">
        /// Generata quando una delle invarianti di dominio non è rispettata.
        /// </exception>
        public void ValidateIntegrity()
        {
            // L'identificativo dell'utente non deve essere vuoto.
            if (Id.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(Id));
            }

            // Il display name non deve essere vuoto.
            if (DisplayName.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.DisplayNameRequired,
                    nameof(DisplayName));
            }

            // L'email non deve essere vuota.
            if (Email.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailRequired,
                    nameof(Email));
            }

            // AvatarUrl può essere vuoto (AvatarUrl.Empty) per indicare assenza di avatar.
        }

        /// <summary>
        /// Aggiorna il profilo dell'utente (display name, avatar, ed eventualmente email).
        /// Le regole di validazione sono demandate ai Value Object.
        /// </summary>
        /// <param name="displayName">Nuovo display name.</param>
        /// <param name="avatarUrl">Nuovo avatar (o <see cref="AvatarUrl.Empty"/>).</param>
        /// <param name="email">
        /// Nuova email opzionale. Se null, l'email attuale non viene modificata.
        /// </param>
        public void UpdateProfile(
        DisplayName displayName,
        AvatarUrl avatarUrl,
        EmailAddress? email = null)
        {
            // DisplayName è obbligatorio nel dominio: vietato portarlo a Empty.
            if (displayName.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.DisplayNameRequired,
                    nameof(displayName));
            }

            // Email è obbligatoria nel dominio; se fornita, non può essere Empty.
            if (email is not null && email.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailRequired,
                    nameof(email));
            }

            DisplayName = displayName;
            AvatarUrl = avatarUrl;

            if (email is not null)
            {
                Email = email;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cambia l'indirizzo email dell'utente.
        /// </summary>
        /// <param name="newEmail">Nuova email già validata a livello di Value Object.</param>
        public void ChangeEmail(EmailAddress newEmail)
        {
            if (newEmail.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.EmailRequired,
                    nameof(newEmail));
            }

            Email = newEmail;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Imposta o aggiorna l'avatar dell'utente.
        /// </summary>
        /// <param name="avatarUrl">
        /// Nuovo avatar; usa <see cref="AvatarUrl.Empty"/> per rimuoverlo.
        /// </param>
        public void SetAvatar(AvatarUrl avatarUrl)
        {
            AvatarUrl = avatarUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
