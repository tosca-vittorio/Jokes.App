using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.ValueObjects
{
    /// <summary>
    /// Identificatore tipizzato e immutabile per la barzelletta.
    /// Viene generato nel dominio per essere disponibile immediatamente (es. Domain Events).
    /// </summary>
    /// <remarks>
    /// Essendo uno <c>struct</c>, in C# esiste sempre un costruttore di default che produce
    /// uno stato equivalente a <see cref="Empty"/> (cioè <see cref="Guid.Empty"/>).
    /// Nel dominio non dovresti mai emettere eventi o accettare stati "vuoti" come fatto di business:
    /// per ottenere un Id valido usa <see cref="New()"/>; per reidratazione usa <see cref="Create(Guid)"/>.
    /// </remarks>
    public readonly record struct JokeId
    {
        #region Properties

        /// <summary>
        /// Valore dell'identificatore.
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// Indica se l'identificatore rappresenta uno stato non inizializzato (<see cref="Guid.Empty"/>).
        /// </summary>
        public bool IsEmpty => Value == Guid.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore privato.
        /// La creazione nel codice applicativo deve passare da <see cref="Create(Guid)"/> o <see cref="New()"/>.
        /// </summary>
        /// <param name="value">Valore dell'identificatore.</param>
        private JokeId(Guid value)
        {
            Value = value;
        }

        #endregion

        #region Factories

        /// <summary>
        /// Crea un identificatore valido a partire da un valore già noto (es. reidratazione da persistenza).
        /// </summary>
        /// <param name="value">Guid già noto (non deve essere <see cref="Guid.Empty"/>).</param>
        /// <returns>Un <see cref="JokeId"/> valido.</returns>
        /// <exception cref="DomainValidationException">
        /// Lanciata se <paramref name="value"/> è <see cref="Guid.Empty"/>.
        /// </exception>
        public static JokeId Create(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(JokeId));
            }

            return new JokeId(value);
        }

        /// <summary>
        /// Genera un nuovo identificatore valido per una barzelletta.
        /// </summary>
        /// <returns>Un <see cref="JokeId"/> valido.</returns>
        public static JokeId New()
            => new JokeId(Guid.NewGuid());

        #endregion

        #region Static members

        /// <summary>
        /// Identificatore "vuoto" (stato non inizializzato / placeholder tecnico).
        /// </summary>
        public static JokeId Empty { get; } = new JokeId(Guid.Empty);

        #endregion

        #region Overrides

        /// <summary>
        /// Restituisce una rappresentazione testuale dell'identificatore.
        /// </summary>
        /// <returns>Il valore <see cref="Guid"/> in formato stringa.</returns>
        public override string ToString() => Value.ToString();

        #endregion
    }
}
