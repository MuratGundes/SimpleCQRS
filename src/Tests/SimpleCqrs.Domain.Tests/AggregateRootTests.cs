﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleCqrs.Events;

namespace SimpleCqrs.Domain.Tests
{
    [TestClass]
    public class AggregateRootTests
    {
        private Mock<MyAggregateRoot> mockAggregateRoot;

        [TestInitialize]
        public void SetupMockForAllTests()
        {
            mockAggregateRoot = new Mock<MyAggregateRoot> {CallBase = true};
        }

        [TestMethod]
        public void HandlerIsCalledWhenHandlerMeetsTheConventionAndEventIsApplied()
        {
            var domainEvent = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.ApplyEvent(domainEvent);

            mockAggregateRoot.Verify(ar => ar.OnHandlerThatMeetsConventionEvent(domainEvent), Times.Once());
        }

        [TestMethod]
        public void HandlerIsNotCalledWhenHandlerDoesNotMeetTheConventionAndEventIsApplied()
        {
            var domainEvent = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.ApplyEvent(domainEvent);

            mockAggregateRoot.Verify(ar => ar.OnHandlerThatDoesMeetsConventionEvent(domainEvent), Times.Never());
        }

        [TestMethod]
        public void HandlerIsNotCalledWhenHandlerHasTwoParametersAndEventIsApplied()
        {
            var domainEvent = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.ApplyEvent(domainEvent);

            mockAggregateRoot.Verify(ar => ar.OnHandlerThatMeetsConventionEvent(domainEvent, "test"), Times.Never());
        }

        [TestMethod]
        public void HandlerIsCalledWhenHandlerIsPrivateAndEventIsApplied()
        {
            var domainEvent = new PrivateHandlerThatMeetsConventionEvent();
            var aggregateRoot = new MyAggregateRoot();

            aggregateRoot.ApplyEvent(domainEvent);

            Assert.IsTrue(aggregateRoot.OnPrivateHandlerThatMeetsConventionEventCalled);
        }

        [TestMethod]
        public void HandlerIsCalledWhenHandlerIsProtectedAndEventIsApplied()
        {
            var domainEvent = new ProtectedHandlerThatMeetsConventionEvent();
            var aggregateRoot = new MyAggregateRoot();

            aggregateRoot.ApplyEvent(domainEvent);

            Assert.IsTrue(aggregateRoot.OnProtectedHandlerThatMeetsConventionEventCalled);
        }

        [TestMethod]
        public void UncommittedEventsAreClearedWhenCommitEventsIsCalled()
        {
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.PublishMyDomainEvent(new HandlerThatMeetsConventionEvent());
            aggregateRoot.PublishMyDomainEvent(new HandlerThatMeetsConventionEvent());
            aggregateRoot.PublishMyDomainEvent(new HandlerThatMeetsConventionEvent());

            aggregateRoot.CommitEvents();

            Assert.AreEqual(0, aggregateRoot.UncommittedEvents.Count);
        }

        [TestMethod]
        public void DomainEventIsPlacedInTheUncommittedEventsPropertyIfPublished()
        {
            var domainEvent = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.PublishMyDomainEvent(domainEvent);

            Assert.AreSame(domainEvent, aggregateRoot.UncommittedEvents.First());
        }

        [TestMethod]
        public void MultipleDomainEventsArePlacedInTheUncommittedEventsPropertyInTheCorrectOrderIfPublished()
        {
            var domainEvent1 = new HandlerThatMeetsConventionEvent();
            var domainEvent2 = new HandlerThatMeetsConventionEvent();
            var domainEvent3 = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.PublishMyDomainEvent(domainEvent1);
            aggregateRoot.PublishMyDomainEvent(domainEvent2);
            aggregateRoot.PublishMyDomainEvent(domainEvent3);

            Assert.AreEqual(3, aggregateRoot.UncommittedEvents.Count);
            Assert.AreSame(domainEvent1, aggregateRoot.UncommittedEvents.ElementAt(0));
            Assert.AreSame(domainEvent2, aggregateRoot.UncommittedEvents.ElementAt(1));
            Assert.AreSame(domainEvent3, aggregateRoot.UncommittedEvents.ElementAt(2));
        }

        [TestMethod]
        public void EventThatIsPublishedIsAssignedTheNextEventId()
        {
            var domainEvent = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.ApplyEvent(new HandlerThatMeetsConventionEvent {EventId = 203});
            aggregateRoot.PublishMyDomainEvent(domainEvent);

            Assert.AreEqual(204, domainEvent.EventId);
        }

        [TestMethod]
        public void DomainEventsAreAssignedSequentialEventIdWhenPublished()
        {
            var domainEvent1 = new HandlerThatMeetsConventionEvent();
            var domainEvent2 = new HandlerThatMeetsConventionEvent();
            var domainEvent3 = new HandlerThatMeetsConventionEvent();
            var aggregateRoot = mockAggregateRoot.Object;

            aggregateRoot.PublishMyDomainEvent(domainEvent1);
            aggregateRoot.PublishMyDomainEvent(domainEvent2);
            aggregateRoot.PublishMyDomainEvent(domainEvent3);

            Assert.AreEqual(1, domainEvent1.EventId);
            Assert.AreEqual(2, domainEvent2.EventId);
            Assert.AreEqual(3, domainEvent3.EventId);
        }
    }

    public class MyAggregateRoot : AggregateRoot
    {
        public virtual void OnHandlerThatMeetsConventionEvent(HandlerThatMeetsConventionEvent domainEvent)
        {
        }

        public virtual void OnHandlerThatDoesMeetsConventionEvent(HandlerThatMeetsConventionEvent domainEvent)
        {
        }

        public virtual void OnHandlerThatMeetsConventionEvent(HandlerThatMeetsConventionEvent domainEvent, string test)
        {
        }

        public bool OnPrivateHandlerThatMeetsConventionEventCalled { get; set; }

        private void OnPrivateHandlerThatMeetsConventionEvent(PrivateHandlerThatMeetsConventionEvent domainEvent)
        {
            OnPrivateHandlerThatMeetsConventionEventCalled = true;
        }

        public bool OnProtectedHandlerThatMeetsConventionEventCalled { get; set; }

        protected void OnProtectedHandlerThatMeetsConventionEvent(ProtectedHandlerThatMeetsConventionEvent domainEvent)
        {
            OnProtectedHandlerThatMeetsConventionEventCalled = true;
        }

        public void PublishMyDomainEvent(HandlerThatMeetsConventionEvent domainEvent)
        {
            PublishEvent(domainEvent);
        }
    }

    public class HandlerThatMeetsConventionEvent : DomainEvent
    {
    }

    public class PrivateHandlerThatMeetsConventionEvent : DomainEvent
    {
    }
    
    public class ProtectedHandlerThatMeetsConventionEvent : DomainEvent
    {
    }
}