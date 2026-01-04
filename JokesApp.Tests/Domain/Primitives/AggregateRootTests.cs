using System;
using FluentAssertions;
using JokesApp.Server.Domain.Events;
using JokesApp.Server.Domain.Primitives;
using Xunit;

namespace JokesApp.Tests.Domain.Primitives
{
    /// <summary>
    /// Test di unità per <see cref="AggregateRoot"/>.
    ///
    /// Obiettivi:
    /// - verificare la corretta gestione della coda eventi,
    /// - garantire che <see cref="AggregateRoot.PullDomainEvents"/> svuoti la lista,
    /// - assicurare l'ordine di inserimento e la protezione da input null.
    /// </summary>
    public sealed class AggregateRootTests
    {
        #region AddDomainEvent

        /// <summary>
        /// Verifica che l'aggiunta di un evento nullo causi un fail-fast con <see cref="ArgumentNullException"/>
        /// e con il nome del parametro coerente con l'implementazione.
        /// </summary>
        [Fact]
        public void AddDomainEvent_ShouldThrow_WhenEventIsNull()
        {
            // Arrange
            var aggregate = new FakeAggregateRoot();

            // Act
            var act = () => aggregate.AddEvent(null!);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("domainEvent");
        }

        /// <summary>
        /// Verifica che la coda eventi mantenga l'ordine di inserimento (FIFO) quando vengono aggiunti più eventi.
        /// </summary>
        [Fact]
        public void AddDomainEvent_ShouldPreserveOrder_WhenMultipleEventsAreAdded()
        {
            // Arrange
            var aggregate = new FakeAggregateRoot();
            var firstEvent = new FakeDomainEvent("first");
            var secondEvent = new FakeDomainEvent("second");

            // Act
            aggregate.AddEvent(firstEvent);
            aggregate.AddEvent(secondEvent);

            // Assert
            aggregate.DomainEvents.Should().ContainInOrder(firstEvent, secondEvent);
        }

        #endregion

        #region PullDomainEvents

        /// <summary>
        /// Verifica che <see cref="AggregateRoot.PullDomainEvents"/> ritorni una sequenza vuota
        /// e lasci la coda vuota quando non sono presenti eventi.
        /// </summary>
        [Fact]
        public void PullDomainEvents_ShouldReturnEmpty_WhenNoEventsAreQueued()
        {
            // Arrange
            var aggregate = new FakeAggregateRoot();

            // Act
            var events = aggregate.PullDomainEvents();

            // Assert
            events.Should().BeEmpty();
            aggregate.DomainEvents.Should().BeEmpty();
        }

        /// <summary>
        /// Verifica che <see cref="AggregateRoot.PullDomainEvents"/> ritorni uno snapshot degli eventi
        /// e svuoti la coda interna.
        /// </summary>
        [Fact]
        public void PullDomainEvents_ShouldReturnSnapshotAndClearQueue_WhenEventsExist()
        {
            // Arrange
            var aggregate = new FakeAggregateRoot();
            var firstEvent = new FakeDomainEvent("first");
            var secondEvent = new FakeDomainEvent("second");

            aggregate.AddEvent(firstEvent);
            aggregate.AddEvent(secondEvent);

            // Act
            var events = aggregate.PullDomainEvents();

            // Assert
            events.Should().HaveCount(2);
            events.Should().ContainInOrder(firstEvent, secondEvent);
            aggregate.DomainEvents.Should().BeEmpty();

            aggregate.PullDomainEvents().Should().BeEmpty();
        }

        #endregion

        #region ClearDomainEvents

        /// <summary>
        /// Verifica che la cancellazione esplicita degli eventi svuoti completamente la coda.
        /// </summary>
        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEvents_WhenCalled()
        {
            // Arrange
            var aggregate = new FakeAggregateRoot();
            aggregate.AddEvent(new FakeDomainEvent("first"));
            aggregate.AddEvent(new FakeDomainEvent("second"));

            // Act
            aggregate.ClearEvents();

            // Assert
            aggregate.DomainEvents.Should().BeEmpty();
            aggregate.PullDomainEvents().Should().BeEmpty();
        }

        #endregion

        #region Test Doubles

        /// <summary>
        /// Aggregate di test che espone wrapper pubblici per invocare i metodi protetti di <see cref="AggregateRoot"/>.
        /// </summary>
        private sealed class FakeAggregateRoot : AggregateRoot
        {
            /// <summary>
            /// Wrapper per <see cref="AggregateRoot.AddDomainEvent"/>.
            /// </summary>
            /// <param name="domainEvent">Evento di dominio da accodare.</param>
            public void AddEvent(IDomainEvent domainEvent)
            {
                AddDomainEvent(domainEvent);
            }

            /// <summary>
            /// Wrapper per <see cref="AggregateRoot.ClearDomainEvents"/>.
            /// </summary>
            public void ClearEvents()
            {
                ClearDomainEvents();
            }
        }

        /// <summary>
        /// Evento di dominio fittizio per testare l'accodamento e l'ordine della coda eventi.
        /// </summary>
        private sealed class FakeDomainEvent : DomainEvent
        {
            /// <summary>
            /// Inizializza un evento con un nome identificativo utile al test.
            /// </summary>
            /// <param name="name">Nome dell'evento.</param>
            public FakeDomainEvent(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Nome dell'evento (payload minimale per i test).
            /// </summary>
            public string Name { get; }
        }

        #endregion
    }
}
