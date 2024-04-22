﻿(function (signalR) {
    signalR.EventAggregator = function (enableProxy) {
        this.constraintId = 0;
        this.contextSubscriptions = new Map();
        this.eventSubscriptions = new Map();

        if (enableProxy) {
            this.proxy = new Proxy(this);
        }
    };

    signalR.EventAggregator.prototype = function () {
        function getSubscribers(message, isInstance) {
            message = getConstructor(message);
            var constructor = isInstance ? message.constructor : message;

            if(!this.eventSubscriptions.has(constructor)) {
                this.eventSubscriptions.set(constructor, []);
            }

            return this.eventSubscriptions.get(constructor);
        }

        function checkConstraintId(subscriber, constraintId) {
            return constraintId == null || (subscriber.constraintId === constraintId);
        }

        function compareSubscriptions(s1, s2) {
            return genericArgumentsCorrect(s1.type.genericArguments, s2.type.genericArguments) &&
                checkConstraintId(s1, s2.constraintId);
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


        function getContextSubscriptions(context) {
            if(!this.contextSubscriptions.has(context)) {
                this.contextSubscriptions.set(context, []);
            }

            return this.contextSubscriptions.get(context);

        }

        function shouldProxySubscription(newSubscription) {
            var subscribers = getSubscribers.call(this, newSubscription.type, false);
            if (getConstructor(newSubscription.type).proxyEvent !== true) return false;

            if (subscribers.length === 0) return true;

            if (newSubscription.type.genericArguments == null && newSubscription.constraint == null) return false;

            var should = true;
            subscribers.every(function (s) {
                if (newSubscription.type.genericArguments != null &&
                    !genericArgumentsCorrect(newSubscription.type.genericArguments, s.type.genericArguments)) return true;

                if (newSubscription.constraint != null && !compareConstraint(newSubscription.constraint, s.constraint)) return true;

                should = false;
                newSubscription.constraintId = s.constraintId;

                return false;
            });

            return should;
        }

        function assignNewConstraintId(subscription) {
            subscription.constraintId = subscription.constraint ? this.constraintId++ : null;
        }

        return {
            unsubscribe: function (context) {
                if(!this.contextSubscriptions.has(context)) return;                

                var subscriptions = this.contextSubscriptions.get(context);
                var actualUnsubscriptions = [];                
                subscriptions.forEach(function (s) {
                    var index = -1;
                    var subscribers = getSubscribers.call(this, s.type, false);
                    for (var i = 0; i < subscribers.length; i++) {
                        if (subscribers[i].context == context && compareSubscriptions(s, subscribers[i])) {
                            index = i;
                            break;
                        }
                    }
                    if (index != -1) {
                        var subscription = subscribers.splice(index, 1)[0];
                        var found = false;
                        for (var j = 0; j < subscribers.length; j++) {
                            if (compareSubscriptions(subscription, subscribers[j])) {
                                found = true;
                                break;
                            }

                        }
                        if (!found) {
                            actualUnsubscriptions.push(subscription);
                        }
                    }

                }.bind(this));

                if (this.proxy) {
                    this.proxy.unsubscribe(actualUnsubscriptions);
                }
            },
            subscribe: function (type, handler, context, constraint) {
                var subscriptions = getContextSubscriptions.call(this, context);

                var subscribers = getSubscribers.call(this, type, false);
                var subscription = { type: type, handler: handler, context: context, constraint: constraint };
                var shouldProxy = shouldProxySubscription.call(this, subscription);

                subscriptions.push(subscription);
                subscribers.push(subscription);

                if (this.proxy && shouldProxy) {
                    assignNewConstraintId.call(this, subscription);
                    this.proxy.subscribe(getConstructor(type), type.genericArguments, constraint, subscription.constraintId);
                }
            },
            publish: function (message, genericArguments, constraintId) {
                var subscribers = getSubscribers.call(this, message, true);
                subscribers.forEach(function (s) {
                    if (genericArgumentsCorrect(s.type.genericArguments, genericArguments) && checkConstraintId(s, constraintId)) {
                        s.handler.call(s.context, message);
                    }
                });
            }
        };
    }();

    var Proxy = function (eventAggregator) {
        this.eventAggregator = eventAggregator;

		if (signalR.HubConnectionBuilder == null) throw "Ensure that SignalR client side library is included and before Signal.EventAggregator cilent library.";
        this.hub = new signalR.HubConnectionBuilder().withUrl("/EventAggregatorProxyHub").build();
        this.hub.on("onEvent", this.onEvent.bind(this));

        this.queueSubscriptions = true;
        this.isConnected = false;
        this.queuedSubscriptions = [];
        this.activeSubscriptions = [];
        this.hub.onclose(this.start.bind(this));
        this.start();
    };

    Proxy.prototype = {
        start: function () {
            this.hub.start()
                .catch(function () { setTimeout(this.start.bind(this), 5000); }.bind(this))
                .then(this.isConnected ? this.reconnected.bind(this) : this.sendSubscribeQueue.bind(this));
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
            var typeNames = eventTypes.map(function (typeData) {
                var constructor = getConstructor(typeData.type);
                if (constructor.proxyEvent !== true) return null;

                return this.removeActiveSubscription({
                    type: constructor.type,
                    genericArguments: typeData.type.genericArguments,
                    id: typeData.constraintId
                });

            }.bind(this)).filter(function (name) { return name !== null });

            if (typeNames.length > 0) {
                this.queueSubscriptions = true;
                this.hub.invoke("Unsubscribe", typeNames).then(this.sendSubscribeQueue.bind(this));
            }
        },
        removeActiveSubscription: function (unsub) {
            for (var i = 0; i < this.activeSubscriptions.length; i++) {
                var e = this.activeSubscriptions[i];
                if (e.type === unsub.type && genericArgumentsCorrect(e.genericArguments, unsub.genericArguments) && e.constraintId === unsub.id) {
                    this.activeSubscriptions.splice(i, 1);
                    break;
                }
            }

            return unsub;
        },
        sendSubscribeQueue: function (arg) {
            var reconnected = typeof (arg) === "boolean" ? arg : false;

            this.isConnected = true;
            this.queueSubscriptions = false;
            if (this.queuedSubscriptions.length === 0) return;

            var temp = this.queuedSubscriptions;
            this.queuedSubscriptions = [];
            this.pushRange(this.activeSubscriptions, temp);
            this.hub.invoke("Subscribe", temp, reconnected);
        },
        pushRange: function (arr, arr2) {
            arr.push.apply(arr, arr2);
        },
        reconnected: function () {
            var temp = this.activeSubscriptions;
            this.activeSubscriptions = [];
            this.queuedSubscriptions = temp;
            this.sendSubscribeQueue(true);
        }
    };

    function getConstructor(type) {
        return type.genericConstructor || type;
    }

    function genericArgumentsCorrect(ga1, ga2) {
        if (ga1 == null) return true;
        if (ga1.length !== ga2.length) return false;

        for (var i = 0; i < ga2.length; i++) {
            if (ga1[i] !== ga2[i]) return false;
        }

        return true;
    }

    signalR.eventAggregator = new signalR.EventAggregator(true);
})(window.signalR = window.signalR || {});