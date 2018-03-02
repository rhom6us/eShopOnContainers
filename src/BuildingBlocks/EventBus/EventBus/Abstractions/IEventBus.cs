using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using System;

namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);

        IEventBus Subscribe<TEvent, THanedler>()
            where TEvent : IntegrationEvent
            where THanedler : IIntegrationEventHandler<TEvent>;

        IEventBus SubscribeDynamic<THandler>(string eventName)
            where THandler : IDynamicIntegrationEventHandler;

        IEventBus UnsubscribeDynamic<THandler>(string eventName)
            where THandler : IDynamicIntegrationEventHandler;

        IEventBus Unsubscribe<TEvent, THandler>()
            where THandler : IIntegrationEventHandler<TEvent>
            where TEvent : IntegrationEvent;
    }
}
