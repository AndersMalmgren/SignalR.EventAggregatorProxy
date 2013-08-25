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

        function genericArgumentsCorrect(ga1, ga2) {
            if (ga1 == null) return true;
            if (ga1.length !== ga2.length) return false;

            for (var i = 0; i < ga2.length; i++) {
                if (ga1[i] !== ga2[i]) return false;
            }

            return true;
        }

        function checkConstraintId(subscriber, constraintId) {
            return constraintId == null || (subscriber.constraintId === constraintId);
        }
        
        function getConstructor(type) {
            return type.genericConstructor || type;
        }
        
        function compareConstraint(c1, c2) {
            if (c2 === undefined) return false;

            for (var member in c1) {
                var val1 = c1[member];
                var val2 = c2[member];

                var isObject = typeof val1 === "object";

                if (val1 !== val2 && !isObject)
                    return false;

                if (isObject && !compareConstraint(val1, val2)) return false;
            }

            return true;
        }


        function prepareContext(context) {
            if (context.__getSubscriptions === undefined) {
                var subscriptions = [];

                context.__getSubscriptions = function() {
                    return subscriptions;
                };
            }
            return context.__getSubscriptions();
        }

        function shouldProxySubscription(newSubscription) {
            var subscribers = getSubscribers(newSubscription.type, false);
            if (getConstructor(newSubscription.type).proxyEvent !== true) return false;

            if (subscribers.length === 0) {
                assignNewConstraintId.call(this, newSubscription);
                return true;
            }

            if (subscribers.length > 0 && newSubscription.type.genericArguments == null && newSubscription.constraint == null) return false;

            var should = true;
            $.each(subscribers, function() {
                if ((newSubscription.type.genericArguments != null && genericArgumentsCorrect(newSubscription.type.genericArguments, this.type.genericArguments)) ||
                    (newSubscription.constraint != null && compareConstraint(newSubscription.constraint, this.constraint))) {
                    
                    should = false;
                    newSubscription.constraintId = this.constraintId;
                }
            });

            if (should && newSubscription.constraint) {
                assignNewConstraintId.call(this, newSubscription);
            }

            return should;
        }

        function assignNewConstraintId(subscription) {
            subscription.constraintId = subscription.constraint ? this.constraintId++ : null;
        }

        return {
            unsubscribe: function (context) {
                if (context.__getSubscriptions === undefined) return;
                var subscriptions = context.__getSubscriptions();
                var acutalUnsubscriptions = [];
                $.each(subscriptions, function () {
                    var index = -1;
                    var subscribers = getConstructor(this.type).__subscribers;
                    for (var j = 0; j < subscribers.length; j++) {
                        if (subscribers[j].context == context &&
                            genericArgumentsCorrect(this.type.genericArguments, subscribers[j].type.genericArguments) &&
                            checkConstraintId(this, subscribers[j].constraintId)) {

                            index = j;
                            break;
                        }
                    }
                    if (index != -1) {
                        var subscription = subscribers.splice(index, 1)[0];
                        if (subscribers.length === 0) {
                            acutalUnsubscriptions.push(subscription);
                        }
                    }

                });
                if (this.proxy) {
                    this.proxy.unsubscribe(acutalUnsubscriptions);
                }
            },
            subscribe: function (type, handler, context, constraint) {
                var subscriptions = prepareContext(context);

                var subscribers = getSubscribers(type, false);
                var subscription = { type: type, handler: handler, context: context, constraint: constraint };
                var shouldProxy = shouldProxySubscription.call(this, subscription);

                subscriptions.push(subscription);
                subscribers.push(subscription);
                
                if (this.proxy && shouldProxy) {
                    this.proxy.subscribe(getConstructor(type), type.genericArguments, constraint, subscription.constraintId);
                }
            },
            publish: function (message, genericArguments, constraintId) {
                var subscribers = getSubscribers.call(this, message, true);
                $.each(subscribers, function () {
                    if (genericArgumentsCorrect(this.type.genericArguments, genericArguments) && checkConstraintId(this, constraintId)) {
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