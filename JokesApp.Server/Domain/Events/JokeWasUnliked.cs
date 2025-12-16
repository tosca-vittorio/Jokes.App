using System;
using JokesApp.Server.Domain.ValueObjects;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Evento di dominio che indica che una barzelletta ha ricevuto un "unlike".
    /// Registra l'identificativo della barzelletta e il nuovo conteggio dei like
    /// dopo l'operazione. Il timestamp dell'evento è esposto tramite
    /// <see cref="DomainEvent.OccurredOn"/>.
    /// </summary>
    public sealed class JokeWasUnliked : DomainEvent
    {
        #region Properties

        /// <summary>
        /// Identificativo tipizzato della barzelletta coinvolta nell'evento.
        /// </summary>
        public JokeId JokeId { get; }

        /// <summary>
        /// Conteggio totale dei like dopo la rimozione.
        /// </summary>
        public int LikesAfterChange { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Crea un evento di "unlike" conforme alle regole del dominio.
        /// </summary>
        /// <param name="jokeId">Identificatore della barzelletta.</param>
        /// <param name="likesAfterChange">Numero totale di like dopo la rimozione.</param>
        /// <exception cref="DomainValidationException">
        /// Generata quando l'identificatore della barzelletta è vuoto
        /// o il numero di like risulta negativo.
        /// </exception>
        public JokeWasUnliked(JokeId jokeId, int likesAfterChange)
        {
            if (jokeId.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(jokeId));
            }

            if (likesAfterChange < 0)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.MinimumLikeOfJokeReached,
                    nameof(likesAfterChange));
            }

            JokeId = jokeId;
            LikesAfterChange = likesAfterChange;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Rappresentazione leggibile dell'evento per log e debugging.
        /// </summary>
        public override string ToString()
            => $"[JokeWasUnliked] JokeId={JokeId}, Likes={LikesAfterChange}, OccurredOn={OccurredOn:O}";

        #endregion
    }
}