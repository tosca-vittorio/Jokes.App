using System;
using JokesApp.Server.Domain.Errors;
using JokesApp.Server.Domain.Events;
using JokesApp.Server.Domain.Exceptions;
using JokesApp.Server.Domain.ValueObjects;
using JokesApp.Server.Domain.Primitives;

namespace JokesApp.Server.Domain.Entities
{
    /// <summary>
    /// Entità del dominio che rappresenta una barzelletta creata da un utente.
    /// Implementa logiche di dominio, validazione, gestione autore e generazione di eventi.
    /// Utilizza Value Objects per garantire integrità e coerenza dei dati.
    /// </summary>
    public sealed class Joke : AggregateRoot
    {

        #region Properties

        /// <summary>
        /// Identificativo tipizzato della barzelletta.
        /// Viene generato nel dominio al momento della creazione, così da essere disponibile immediatamente
        /// (es. per Domain Events). In fase di reidratazione, il valore viene impostato dalla persistenza/ORM.
        /// </summary>
        public JokeId Id { get; private set; }

        /// <summary>
        /// Testo della domanda, rappresentato tramite un Value Object che garantisce lunghezza e validità.
        /// </summary>
        public QuestionText Question { get; private set; }

        /// <summary>
        /// Testo della risposta, rappresentato tramite un Value Object che garantisce lunghezza e validità.
        /// </summary>
        public AnswerText Answer { get; private set; }

        /// <summary>
        /// Identificatore tipizzato dell'autore della barzelletta.
        /// È un Value Object che incapsula le regole di validazione del dominio.
        /// </summary>
        public UserId ApplicationUserId { get; private set; }

        /// <summary>
        /// Riferimento all'entità ApplicationUser lato dominio.
        /// Potrà essere popolata dal livello di persistenza o dall'application layer.
        /// </summary>
        public ApplicationUser? Author { get; private set; }

        /// <summary>
        /// Data e ora di creazione della barzelletta in formato UTC.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Data e ora dell'ultima modifica della barzelletta in formato UTC.
        /// Null se non è mai stata aggiornata.
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Numero totale dei like ricevuti dalla barzelletta.
        /// </summary>
        public int Likes { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Costruttore richiesto da EF Core.
        /// </summary>
        private Joke()
        {
            // EF Core only
        }

        /// <summary>
        /// Costruttore principale del dominio.
        /// Esegue validazioni, assegna i Value Objects e genera un evento di creazione.
        /// </summary>
        /// <param name="question">Value Object contenente la domanda.</param>
        /// <param name="answer">Value Object contenente la risposta.</param>
        /// <param name="userId">Identificatore tipizzato dell'autore.</param>
        private Joke(QuestionText question, AnswerText answer, UserId userId)
        {
            if (question is null || question.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(question));
            }

            if (answer is null || answer.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(answer));
            }

            if (userId.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(userId));
            }

            EnsureQuestionAndAnswerAreDifferent(question, answer);

            Id = JokeId.New();
            Question = question;
            Answer = answer;
            ApplicationUserId = userId;
            CreatedAt = DateTime.UtcNow;

            // Con Id domain-generated, l'evento "Created" deve nascere già con un identificatore reale.
            AddDomainEvent(new JokeWasCreated(
                Id,
                ApplicationUserId,
                Question,
                Answer,
                CreatedAt));
        }

        /// <summary>
        /// Factory di dominio per la creazione controllata dell'Aggregate.
        /// Centralizza validazioni, invarianti e generazione dei Domain Events.
        /// </summary>
        public static Joke Create(QuestionText question, AnswerText answer, UserId userId)
            => new(question, answer, userId);

        #endregion

        #region Author management

        /// <summary>
        /// Imposta l'autore della barzelletta verificando che:
        /// - l'istanza non sia nulla;
        /// - l'identificativo dell'autore non sia vuoto;
        /// - non sia già stato impostato un autore;
        /// - l'identificativo dell'autore corrisponda a quello previsto dal dominio.
        /// </summary>
        /// <param name="author">Istanza di <see cref="ApplicationUser"/> da associare.</param>
        public void SetAuthor(ApplicationUser author)
        {
            EnsureIdIsInitialized();
            if (author is null)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AuthorNull,
                    nameof(author));
            }

            if (author.Id.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(author.Id)); // oppure nameof(ApplicationUser.Id)
            }

            if (Author is not null)
            {
                throw new DomainOperationException(JokeErrorMessages.AuthorAlreadySet);
            }

            if (!author.Id.Equals(ApplicationUserId))
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AuthorIdMismatch,
                    nameof(author.Id)); // oppure nameof(ApplicationUserId)
            }

            Author = author;
        }

        #endregion

        #region Domain behavior

        /// <summary>
        /// Determina se la barzelletta è stata creata dal determinato utente.
        /// </summary>
        /// <param name="userId">Identificatore dell'utente da verificare.</param>
        public bool IsAuthoredBy(UserId userId)
            => ApplicationUserId.Equals(userId);

        /// <summary>
        /// Aggiorna la barzelletta sostituendo domanda e risposta dopo le opportune validazioni.
        /// Genera un evento di aggiornamento.
        /// </summary>
        /// <param name="userId">Identificatore dell'utente che richiede l'aggiornamento.</param>
        /// <param name="question">Nuovo testo della domanda.</param>
        /// <param name="answer">Nuovo testo della risposta.</param>
        public void Update(UserId userId, QuestionText question, AnswerText answer)
        {
            EnsureIdIsInitialized();

            if (question is null || question.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionNullOrEmpty,
                    nameof(question));
            }

            if (answer is null || answer.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.AnswerNullOrEmpty,
                    nameof(answer));
            }

            if (userId.IsEmpty)
            {
                throw new DomainValidationException(
                    ApplicationUserErrorMessages.UserIdNullOrEmpty,
                    nameof(userId));
            }

            if (!IsAuthoredBy(userId))
            {
                throw new UnauthorizedDomainOperationException(JokeErrorMessages.UpdateNotAllowed);
            }

            EnsureQuestionAndAnswerAreDifferent(question, answer);

            Question = question;
            Answer = answer;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new JokeWasUpdated(
                Id,
                Question,
                Answer,
                UpdatedAt.Value));
        }

        /// <summary>
        /// Incrementa il numero di like garantendo che non si verifichino overflow.
        /// Genera un evento di like.
        /// </summary>
        public void AddLike()
        {
            EnsureIdIsInitialized();

            if (Likes == int.MaxValue)
            {
                throw new DomainOperationException(JokeErrorMessages.MaximumLikeOfJokeReached);
            }

            Likes++;

            AddDomainEvent(new JokeWasLiked(
                Id,
                Likes));
        }

        /// <summary>
        /// Decrementa il numero di like garantendo che non si scenda sotto zero.
        /// Genera un evento di dominio <see cref="JokeWasUnliked"/>.
        /// </summary>
        public void RemoveLike()
        {
            EnsureIdIsInitialized();

            if (Likes == 0)
            {
                throw new DomainOperationException(JokeErrorMessages.MinimumLikeOfJokeReached);
            }

            Likes--;

            AddDomainEvent(new JokeWasUnliked(
                Id,
                Likes));
        }

        #endregion

        #region Validation helpers

        /// <summary>
        /// Verifica che domanda e risposta non siano identiche ignorando le differenze di maiuscole/minuscole.
        /// </summary>
        /// <param name="q">Testo della domanda.</param>
        /// <param name="a">Testo della risposta.</param>
        private static void EnsureQuestionAndAnswerAreDifferent(QuestionText q, AnswerText a)
        {
            if (string.Equals(q.Value, a.Value, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainValidationException(
                    JokeErrorMessages.QuestionAndAnswerCannotMatch,
                    nameof(Question));
            }
        }

        /// <summary>
        /// Verifica che l'entità abbia un identificatore valido.
        /// Utile come guard interna per evitare l'uso "accidentale" di istanze non inizializzate (Id vuoto).
        /// </summary>
        private void EnsureIdIsInitialized()
        {
            if (Id.IsEmpty)
            {
                throw new DomainValidationException(
                    JokeErrorMessages.JokeIdEmpty,
                    nameof(Id));
            }
        }

        /// <summary>
        /// Controlla che lo stato interno dell'entità sia coerente e valido.
        /// Utile per test, importazioni o verifiche interne.
        /// </summary>
        public void ValidateIntegrity()
        {
            EnsureIdIsInitialized();
            EnsureQuestionAndAnswerAreDifferent(Question, Answer);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Ritorna una rappresentazione testuale sintetica dell'entità utile per logging o debugging.
        /// </summary>
        public override string ToString()
            => $"Joke(Id={Id}, UserId={ApplicationUserId}, CreatedAt={CreatedAt:O})";

        #endregion
    }
}
