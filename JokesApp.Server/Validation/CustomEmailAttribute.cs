using System;
using System.ComponentModel.DataAnnotations;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;

namespace JokesApp.Server.Validation
{
    /// <summary>
    /// Attributo di validazione personalizzato per indirizzi e-mail.
    /// Delegando la logica di validazione al Value Object <see cref="EmailAddress"/>,
    /// garantisce coerenza con le regole di dominio senza duplicare la logica.
    ///
    /// Nota:
    /// - La presenza del valore (Required) viene gestita separatamente tramite [Required].
    /// - Questo attributo valida solo il formato/coerenza rispetto al dominio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CustomEmailAttribute : ValidationAttribute
    {
        /// <summary>
        /// Esegue la validazione sul valore associato alla proprietà o al campo.
        /// Se il valore è nullo o vuoto, la responsabilità viene delegata ad altri attributi
        /// (es. <see cref="RequiredAttribute"/>), restituendo <see cref="ValidationResult.Success"/>.
        /// </summary>
        /// <param name="value">Valore da validare.</param>
        /// <param name="validationContext">Contesto di validazione.</param>
        /// <returns>
        /// <see cref="ValidationResult.Success"/> se la validazione ha esito positivo;
        /// in caso contrario, un <see cref="ValidationResult"/> con il messaggio di errore.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Se il valore è nullo o vuoto, lasciamo che sia [Required] (se presente)
            // a gestire l'obbligatorietà. Qui validiamo solo il formato/logica di dominio.
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string email = value.ToString()!.Trim();

            try
            {
                // Delego la validazione al Value Object di dominio.
                // Se non rispetta le regole di dominio, verrà sollevata una DomainValidationException.
                EmailAddress.Create(email);
                return ValidationResult.Success;
            }
            catch (DomainValidationException ex)
            {
                // Se è stato impostato un messaggio custom sull'attributo, lo usiamo;
                // altrimenti usiamo il messaggio di dominio per mantenere coerenza.
                return new ValidationResult(
                    ErrorMessage ?? ex.Message
                );
            }
        }

        /// <summary>
        /// Metodo di utilità per validare un indirizzo e-mail in modo statico,
        /// riutilizzando le stesse regole del Value Object <see cref="EmailAddress"/>.
        /// </summary>
        /// <param name="email">Stringa contenente l'indirizzo e-mail da validare.</param>
        /// <returns>
        /// <c>true</c> se l'e-mail è considerata valida dal dominio;
        /// <c>false</c> in caso contrario.
        /// </returns>
        public static bool IsValidStatic(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                EmailAddress.Create(email.Trim());
                return true;
            }
            catch (DomainValidationException)
            {
                return false;
            }
        }
    }
}
