using System;
using JokesApp.Server.Domain.ValueObjects;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Exceptions;

namespace JokesApp.Server.Domain.Events
{
    /// <summary>
    /// Evento di dominio che rappresenta l'aggiunta di un like a una barzelletta.
    /// Registra l'ID della barzelletta e il nuovo totale dei like dopo l'operazione.
    /// Il timestamp dell'evento è esposto tramite <see cref="DomainEvent.OccurredOn"/>.
    /// </summary>
    public sealed class JokeWasLiked : DomainEvent
    {
        #region Properties

        /// <summary>
        /// Identificativo tipizzato della barzelletta a cui è stato aggiunto un like.
        /// </summary>
        public JokeId JokeId { get; }

        /// <summary>
        /// Numero totale dei like dopo l'operazione.
        /// </summary>
        public int LikesAfterChange { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Crea un evento di like valido secondo le regole di dominio.
        /// </summary>
        /// <param name="jokeId">Identificatore della barzelletta.</param>
        /// <param name="likesAfterChange">Numero totale di like dopo l'operazione.</param>
        /// <exception cref="DomainValidationException">
        /// Generata quando l'identificatore della barzelletta è vuoto
        /// o il numero di like risulta negativo.
        /// </exception>
        public JokeWasLiked(JokeId jokeId, int likesAfterChange)
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
            => $"[JokeWasLiked] JokeId={JokeId}, Likes={LikesAfterChange}, OccurredOn={OccurredOn:O}";

        #endregion
    }
}
