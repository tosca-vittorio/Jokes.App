namespace JokesApp.Server.Domain.Errors
{
    /// <summary>
    /// Contiene tutti i messaggi di errore relativi al dominio ApplicationUser.
    /// Questi messaggi vengono utilizzati da entità e Value Object
    /// per mantenere consistenza nelle regole di validazione.
    /// </summary>
    public static class ApplicationUserErrorMessages
    {
        #region DisplayName errors

        /// <summary>
        /// Messaggio per indicare che il DisplayName supera la lunghezza massima consentita.
        /// </summary>
        public const string DisplayNameMaxLength =
            "DisplayName exceeds maximum length of 50.";

        /// <summary>
        /// Messaggio per indicare che il DisplayName è obbligatorio ma non è stato fornito.
        /// </summary>
        public const string DisplayNameRequired =
            "DisplayName is required.";

        #endregion

        #region AvatarUrl errors

        /// <summary>
        /// Messaggio per indicare che l'AvatarUrl supera la lunghezza massima consentita.
        /// </summary>
        public const string AvatarUrlMaxLength =
            "AvatarUrl exceeds maximum length of 2048.";

        /// <summary>
        /// Messaggio per indicare che l'AvatarUrl non è un URL valido.
        /// </summary>
        public const string AvatarUrlInvalid =
            "AvatarUrl is not a valid URL.";

        #endregion

        #region Email errors

        /// <summary>
        /// Messaggio per indicare che l'email non è in un formato valido.
        /// </summary>
        public const string EmailInvalid =
            "Email is not a valid email address.";

        /// <summary>
        /// Messaggio per indicare che l'email è obbligatoria ma non è stata fornita.
        /// </summary>
        public const string EmailRequired =
            "Email is required.";

        /// <summary>
        /// Messaggio per indicare che l'email supera la lunghezza massima consentita.
        /// </summary>
        public const string EmailTooLong =
            "Email exceeds maximum length of 256.";

        #endregion

        #region UserId errors

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente è nullo o vuoto.
        /// </summary>
        public const string UserIdNullOrEmpty =
            "UserId cannot be null or empty.";

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente contiene caratteri non validi.
        /// </summary>
        public const string UserIdInvalid =
            "UserId contains invalid characters.";

        /// <summary>
        /// Messaggio per indicare che l'identificativo utente supera la lunghezza massima consentita.
        /// </summary>
        public const string UserIdTooLong =
            "UserId exceeds maximum allowed length.";
        #endregion
    }
}
