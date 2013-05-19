(function(signalR) {
    signalR.EventAggregator = function (enableProxy) {
        if (enableProxy) {
            this.proxy = new Proxy(this);
        }
    };

    signalR.EventAggregator.prototype = function () {
        function getSubscribers(message, isInstance) {
            var constructor = isInstance ? message.constructor : message;
            if (constructor.__subscribers === undefined) {
                constructor.__subscribers = [];
            }

            return constructor.__subscribers;
        }

        return {
            unsubscribe: function (context) {
                $.each(context.__subscribedMessages, function () {
                    var index = -1;
                    for (var j = 0; j < this.__subscribers.length; j++) {
                        if (this.__subscribers[j].context == context) {
                            index = j;
                            break;
                        }
                    }
                    if (index != -1) {
                        this.__subscribers.splice(index, 1);
                    }
                });
                if (this.proxy) {
                    this.proxy.unsubscribe(context.__subscribedMessages);
                }
            },
            subscribe: function (type, handler, context, constraint) {
                if (context.__subscribedMessages === undefined) {
                    context.__subscribedMessages = [];
                }
                context.__subscribedMessages.push(type);

                var subscribers = getSubscribers.call(this, type, false);
                subscribers.push({ handler: handler, context: context });
                
                if (this.proxy) {
                    this.proxy.subscribe(type, constraint);
                }
            },
            publish: function (message) {
                var subscribers = getSubscribers.call(this, message, true);
                $.each(subscribers, function () {
                    this.handler.call(this.context, message);
                });
            }
        };
    }();

    var Proxy = function (eventAggregator) {
        this.eventAggregator = eventAggregator;
        
        this.hub = $.connection.eventAggregatorProxyHub;
        this.hub.client.onEvent = this.onEvent.bind(this);
        this.started = false;
        this.quedSubscriptions = [];
        $.connection.hub.start().done(this.onStarted.bind(this));
    };

    Proxy.prototype = {
        onStarted: function() {
            this.started = true;
            $.each(this.quedSubscriptions, function(index, subscription) {
                this.subscribe(subscription.eventType, subscription.constraint);
            }.bind(this));
        },
        onEvent: function(message) {
            var type = signalR.getEvent(message.type);
            var event = new type();
            for (var member in message.event) {
                event[member] = message.event[member];
            }

            this.eventAggregator.publish(event);
        },
        subscribe: function (eventType, constraint) {
            if (eventType.proxyEvent !== true);

            if (this.started === false) {
                this.quedSubscriptions.push({ eventType: eventType, constraint: constraint });
            } else {
                this.hub.server.subscribe(eventType.type, constraint);
            }
        },
        unsubscribe: function (eventTypes) {
            var typeNames = $.map(eventTypes, function (eventType) {
                return eventType.type;
            });
            this.hub.server.unsubscribe(typeNames);
        }
    };

    signalR.eventAggregator = new signalR.EventAggregator(true);
})(window.signalR = window.signalR || {});