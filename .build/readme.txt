Please see https://github.com/AndersMalmgren/SignalR.EventAggregatorProxy for more information on using this lib with SignalR.
 
Mapping the events
----------------------------
You need to Map the server side events to client side events. To register the events, call  RouteTable.Routes.MapEventProxy in  your application's
Application_Start method, e.g.:
 
using System;
using System.Web;
using System.Web.Routing;
 
namespace MyWebApplication
{
    public class Global : System.Web.HttpApplication
    {
        public void Application_Start()
        {
            // Register the default hubs route: ~/signalr
            RouteTable.Routes.MapHubs();
			RouteTable.Routes.MapEventProxy<EventBase>();
        }
    }
}

The generic argument is the Base class of all events that can be sent to client, this is used to render the event proxy javascript

Do not forget to also run RouteTable.Routes.MapHubs(); to register the event proxy hub and your own hubs if you have any

You need to include the event proxy script on the site.

@Scripts.Render("~/eventAggregation/events")

You also need to include the api script file: jquery.signalR.eventAggregator-$version$.js

EventAggregator Proxy
----------------------------
This library is not dependant on a specific EventAggregator, you need to create a proxy between your EventAggregator and ours, implemnet IEventAggragor, e.g.:

public class EventAggregatorProxy : IEventAggregator
{
    private Action<object> handler;	  

    public void Subscribe(Action<object> handler)
    {
        this.handler = handler;
    }
}

When your EventAggregator sends you a message call handler(message) to forward the message.
To let the library know which IEventAggregator to use Register it in your IoC.

Client
----------------------------
To subscribe to a server side event do

signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Events.TestEvent, this.onEvent, this);

You can also supply constraint info that are parsed server side by your Constraint handler like

signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Events.TestEvent, this.onEvent, this, { test: "TestData" });

When a connection disconnects the library will unsubscribe the listener automatic, but you can also unsubscribe a listener like

signalR.eventAggregator.unsubscribe(this);

This has to be the same reference that you sent in to subscribe.

Constraint handlers
----------------------------
We have also included constraint handlers so that you can deny certain events to reach certain clients, its used like

public class TestEventConstraintHandler : EventConstraintHandler<TestEvent>
{
    public override bool Allow(TestEvent message, string username, dynamic constraint)
    {
        return true;
    }
}

the dynamic object sent into the Alllow method is the same that you supplied in the subscribe function client side you can use this to constraint the events