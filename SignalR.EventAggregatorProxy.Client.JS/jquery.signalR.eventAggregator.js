(function (signalR, $) {
    signalR.EventAggregator = function (enableProxy) {
        this.constraintId = 0;
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

        function checkConstraintId(subscriber, constraintId) {
            return constraintId == null || (subscriber.constraintId === constraintId);
        }

        return {
            unsubscribe: function (context) {
                if (context.__subscriptions === undefined) return;

                $.each(context.__subscriptions, function () {
                    var index = -1;
                    var subscribers = (this.type.genericConstructor || this.type).__subscribers;
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
                    this.proxy.unsubscribe(context.__subscriptions);
                }
            },
            subscribe: function (type, handler, context, constraint) {
                if (context.__subscriptions === undefined) {
                    context.__subscriptions = [];
                }

                var constraintId = constraint ? this.constraintId++ : null;

                var subscribers = getSubscribers.call(this, type, false);
                var subscription = { type: type, handler: handler, context: context, constraintId: constraintId };

                context.__subscriptions.push(subscription);
                subscribers.push(subscription);

                if (this.proxy) {
                    this.proxy.subscribe(type.genericConstructor || type, type.genericArguments, constraint, constraintId);
                }
            },
            publish: function (message, genericArguments, constraintId) {
                var subscribers = getSubscribers.call(this, message, true);
                $.each(subscribers, function () {
                    if (genericArgumentsCorrect(this, genericArguments) && checkConstraintId(this, constraintId)) {
                        this.handler.call(this.context, message);
                    }
                });
            }
        };
    } ();

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

            this.eventAggregator.publish(event, message.genericArguments, message.id);
        },
        subscribe: function (eventType, genericArguments, constraint, constraintId) {
            if (eventType.proxyEvent !== true) return;

            this.queuedSubscriptions.push({ type: eventType.type, genericArguments: genericArguments, constraint: constraint, constraintId: constraintId });
            if (!this.queueSubscriptions) {
                clearTimeout(this.throttleTimer);
                this.throttleTimer = setTimeout(this.sendSubscribeQueue.bind(this), 1);
            }
        },
        unsubscribe: function (eventTypes) {
            var typeNames = $.map(eventTypes, function (typeData) {
                var constructor = typeData.type.genericConstructor || typeData.type;
                if (constructor.proxyEvent !== true) return null;

                return {
                    type: constructor.type,
                    genericArguments: typeData.type.genericArguments,
                    id: typeData.constraintId
                };
            });

            if (typeNames.length > 0) {
                this.queueSubscriptions = true;
                this.hub.server.unsubscribe(typeNames).done(function () {
                    this.queueSubscriptions = false;
                    this.sendSubscribeQueue();
                } .bind(this));
            }
        },
        sendSubscribeQueue: function () {
            if (this.queuedSubscriptions.length === 0) return;
            
            var temp = this.queuedSubscriptions;
            this.queuedSubscriptions = [];
            this.hub.server.subscribe(temp);
        }
    };

    signalR.eventAggregator = new signalR.EventAggregator(true);
})(window.signalR = window.signalR || {}, jQuery);