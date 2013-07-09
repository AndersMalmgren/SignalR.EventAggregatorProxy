(function(signalR, $) {
    signalR.EventAggregator = function (enableProxy) {
        if (enableProxy) {
            this.proxy = new Proxy(this);
        }
    };

    signalR.EventAggregator.prototype = function () {
        function getSubscribers(message, isInstance) {
            message = message.genericConstructor || message;
            var constructor = isInstance ? message.constructor : message;
            if (constructor.__subscribers === undefined) {
                constructor.__subscribers = [];
            }

            return constructor.__subscribers;
        }
        
        function genericArgumentsCorrect(subscriber, genericArguments) {
            if (subscriber.genericArguments == null) return true;
            if (subscriber.genericArguments.length !== genericArguments.length) return false;

            for (var i = 0; i < genericArguments.length; i++) {
                if (subscriber.genericArguments[i] !== genericArguments[i]) return false;
            }

            return true;
        }

        return {
            unsubscribe: function (context) {
                if (context.__subscribedMessages === undefined) return;
                
                $.each(context.__subscribedMessages, function () {
                    var index = -1;
                    var subscribers = (this.genericConstructor || this).__subscribers;
                    for (var j = 0; j < subscribers.length; j++) {
                        if (subscribers[j].context == context) {
                            index = j;
                            break;
                        }
                    }
                    if (index != -1) {
                        subscribers.splice(index, 1);
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
                subscribers.push({ handler: handler, context: context, genericArguments: type.genericArguments });
                
                if (this.proxy) {
                    this.proxy.subscribe(type.genericConstructor || type, type.genericArguments, constraint);
                }
            },
            publish: function (message, genericArguments) {
                var subscribers = getSubscribers.call(this, message, true);
                $.each(subscribers, function () {
                    if (genericArgumentsCorrect(this, genericArguments)) {
                        this.handler.call(this.context, message);
                    }
                });
            }
        };
    }();

    var Proxy = function (eventAggregator) {
        this.eventAggregator = eventAggregator;
        
        this.hub = $.connection.eventAggregatorProxyHub;
        this.hub.client.onEvent = this.onEvent.bind(this);
        this.queueSubscriptions = true;
        this.queuedSubscriptions = [];
        $.connection.hub.start().done(this.onStarted.bind(this));
    };

    Proxy.prototype = {
        onStarted: function () {
            this.queueSubscriptions = false;
            this.sendSubscribeQueue();
        },
        onEvent: function (message) {
            var type = signalR.getEvent(message.type);
            var event = new type();
            for (var member in message.event) {
                event[member] = message.event[member];
            }

            this.eventAggregator.publish(event, message.genericArguments);
        },
        subscribe: function (eventType, genericArguments, constraint) {
            if (eventType.proxyEvent !== true) return;

            if (this.queueSubscriptions) {
                this.queuedSubscriptions.push({ eventType: eventType, genericArguments: genericArguments, constraint: constraint });
            } else {
                this.hub.server.subscribe(eventType.type, genericArguments, constraint);
            }
        },
        unsubscribe: function (eventTypes) {
            var typeNames = $.map(eventTypes, function (eventType) {
                var constructor = eventType.genericConstructor || eventType;
                if (constructor.proxyEvent !== true) return null;
                
                return {
                    type: constructor.type,
                    genericArguments: eventType.genericArguments
                };
            });
            
            if (typeNames.length > 0) {
                this.queueSubscriptions = true;
                this.hub.server.unsubscribe(typeNames).done(function() {
                    this.queueSubscriptions = false;
                    this.sendSubscribeQueue();
                }.bind(this));
            }
        },
        sendSubscribeQueue: function () {
            while (this.queuedSubscriptions.length > 0) {
                var subscription = this.queuedSubscriptions.shift();
                this.subscribe(subscription.eventType, subscription.genericArguments, subscription.constraint);
            }
        }
    };

    signalR.eventAggregator = new signalR.EventAggregator(true);
})(window.signalR = window.signalR || {}, jQuery);