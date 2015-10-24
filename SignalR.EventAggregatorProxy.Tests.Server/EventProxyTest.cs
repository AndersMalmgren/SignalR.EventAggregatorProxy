using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.Extensions;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.Model;

namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public class EventProxyTest : ServerTest
    {
        private EventProxy eventProxy;
        protected ConcurrentBag<string> ids;
        protected string typeName;
        protected ConcurrentBag<object> events;
        private List<EventType> typeNames;

        protected Action<object> SetupProxy(Type eventType, IEnumerable<Type> constraintHandlerTypes = null)
        {
            ids = new ConcurrentBag<string>();
            events = new ConcurrentBag<object>();
            typeName = eventType.FullName;
            typeNames = new[] { typeName }.Select(t => new EventType { Type = t }).ToList();

            Action<object> handler = null;
            WhenCalling<ITypeFinder>(x => x.ListEventTypes()).Return(new[] { eventType });
            WhenCalling<ITypeFinder>(x => x.GetEventType(Arg<string>.Is.Anything)).Return(eventType);
            if (constraintHandlerTypes != null)
                constraintHandlerTypes.ForEach(t => Register(Activator.CreateInstance((Type) t)));
            WhenCalling<ITypeFinder>(x => x.GetConstraintHandlerTypes(Arg<Type>.Is.Anything))
                    .Return(constraintHandlerTypes != null ? constraintHandlerTypes : Enumerable.Empty<Type>());

            WhenCalling<IEventAggregator>(x => x.Subscribe(Arg<Action<object>>.Is.Anything)).Callback<Action<object>>(h =>
            {
                handler = h;
                return true;
            });
            WhenAccessing<IRequest, IPrincipal>(x => x.User).Return(Thread.CurrentPrincipal);

            var client = new Client(events);

            WhenCalling<IHubConnectionContext<dynamic>>(x => x.Client(Arg<string>.Is.Anything)).Return(client);
            WhenAccessing<IHubContext, IHubConnectionContext<dynamic>>(x => x.Clients).Return(Get<IHubConnectionContext<dynamic>>());
            Mock<IConnectionManager>();
            WhenCalling<IConnectionManager>(x => x.GetHubContext<EventAggregatorProxyHub>()).Return(Get<IHubContext>());

            eventProxy = new EventProxy();

            return handler;
        }

        private HubCallerContext CreateHubContext()
        {
            var id = Guid.NewGuid().ToString();
            ids.Add(id);

            return new HubCallerContext(Get<IRequest>(), id);
        }

        protected void Subscribe()
        {
            eventProxy.Subscribe(CreateHubContext(), typeName, new string[0], null, null);
        }

        protected void Unsubscribe(string id)
        {
            eventProxy.Unsubscribe(id, typeNames);
        }

        protected void UnsubscribeConnection(string id)
        {
            eventProxy.UnsubscribeConnection(id);
        }
           

        public class Client
        {
            private readonly ConcurrentBag<object> events;

            public Client(ConcurrentBag<object> events)
            {
                this.events = events;
            }

            public void onEvent(object message)
            {
                events.Add(message);
            }
        }
    }
}