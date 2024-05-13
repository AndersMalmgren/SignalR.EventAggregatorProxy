using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Extensions;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public abstract class EventProxyTest : Test
    {
        protected ConcurrentBag<string> ids;
        protected string typeName;
        protected ConcurrentBag<object> events;
        protected Func<object, Task> handler;
        private List<EventType> typeNames;
        private List<string> genericArguments;

        protected void SetupProxy(IServiceCollection collection, Type eventType, IEnumerable<Type> constraintHandlerTypes = null)
        {
            ids = new ConcurrentBag<string>();
            events = new ConcurrentBag<object>();
            typeName = eventType.FullName;
            typeNames = new[] { typeName }.Select(t => new EventType { Type = t }).ToList();
            genericArguments = eventType.GetGenericArguments().Select(ga => ga.FullName).ToList();


            collection.AddSingleton<EventProxy>()
            .MockSingleton<ITypeFinder>(mock =>
            {
                eventType.GetGenericArguments().ForEach(ga => mock.Setup(x => x.GetType(ga.FullName)).Returns(ga));
                mock.Setup(x => x.ListEventTypes()).Returns(new[] {eventType});
                mock.Setup(x => x.GetEventType(It.IsAny<string>())).Returns(eventType);
                mock.Setup(x => x.GetConstraintHandlerTypes(It.IsAny<Type>())).Returns(constraintHandlerTypes ?? Enumerable.Empty<Type>());
            })
            .MockSingleton<IEventAggregator>(mock => mock.Setup(x => x.Subscribe(It.IsAny<Func<object, Task>>())).Callback((Func<object, Task> h) => handler = h))
            .MockTransiant<HubCallerContext>(mock =>
            {
                var id = CreateNewConnectionId();
                mock.Setup(x => x.ConnectionId).Returns(id);
                mock.Setup(x => x.User.Identity.Name).Returns("foobar");
            })
            .MockSingleton<IHubContext<EventAggregatorProxyHub>>(mock => mock.Setup(x => x.Clients.Client(It.IsAny<string>()).SendCoreAsync("onEvent", It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Callback((string method, object[] obj, CancellationToken cancel) => events.Add(obj[0])).Returns(Task.CompletedTask))
            .MockSingleton<ILogger<EventProxy>>();



            constraintHandlerTypes?.ForEach(t => collection.AddSingleton(t));
            //WhenAccessing<IRequest, IPrincipal>(x => x.User).Return(Thread.CurrentPrincipal);

            //var client = new Client(events);

            //WhenCalling<IHubConnectionContext<dynamic>>(x => x.Client(Arg<string>.Is.Anything)).Return(client);
            //WhenAccessing<IHubContext, IHubConnectionContext<dynamic>>(x => x.Clients).Return(Get<IHubConnectionContext<dynamic>>());
            //Mock<IConnectionManager>();
            //WhenCalling<IConnectionManager>(x => x.GetHubContext<EventAggregatorProxyHub>()).Return(Get<IHubContext>());
            
        
        }

        private string CreateNewConnectionId()
        {
           var id = Guid.NewGuid().ToString();
           ids.Add(id);
           return id;
        }

        protected EventProxy EventProxy => Get<EventProxy>();

        protected void Subscribe()
        {
            EventProxy.Subscribe(Get<HubCallerContext>(), typeName, genericArguments, new JsonElement(), null);
        }

        protected void Unsubscribe(string id)
        {
            EventProxy.Unsubscribe(id, typeNames);
        }

        protected void UnsubscribeConnection(string id)
        {
            EventProxy.UnsubscribeConnection(id);
        }
    }
}