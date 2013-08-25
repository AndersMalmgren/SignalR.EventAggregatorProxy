window.TestEvent = function() {

};

constructEvent = function(genericArgument, event, proxyEvent) {
    event = event || (event || {}).contructor ||
        function() {
    };

    var constructor = event;

    if (proxyEvent == null || proxyEvent === true) {
        event.proxyEvent = true;
    }

    if (genericArgument != null) {
        event = {
            genericConstructor: constructor,
            genericArguments: [genericArgument]
        };
    }

    return genericArgument ? {
        event: event,
        constructor: constructor
    }  : event;
};

throttleTest = function (name, setup) {
    asyncTest(name, function () {
        
        var throttleAssert = setup();
        start();
        
        if (throttleAssert) {
            throttleAssert();
        }
    });
};

test("Event aggregator class should have correct casing on closure (signalR) Issue #1 ", function () {
    ok(window.signalR !== undefined);
    ok(window.signalR.eventAggregator !== undefined);
});

throttleTest("When subscribing to client side event Issue #2", function () {
    expect(0);
    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
        ok(false, "Server side subscribe should not be called for client side events");
    };

    signalR.eventAggregator.subscribe(TestEvent, function() {
    }, {});
});

test("When unsubscribing to client side event Issue #5", function () {
    var context = {};
    $.connection.eventAggregatorProxyHub.server.unsubscribe = function () {
        ok(false, "Server side unsubscribe should not be called for client side events");
    };

    signalR.eventAggregator.subscribe(TestEvent, function () {
    }, context);
    signalR.eventAggregator.unsubscribe(context);
    ok(true, "Server side unsubscribe was not called");
});

test("When subscribing to client side event", function () {
    var context = {};
    var event = new TestEvent();
    var failIfPublished = false;
    signalR.eventAggregator.subscribe(TestEvent, function (e) {
        if (failIfPublished) {
            ok(false, "Listener should not have been called");
        }
        equal(event, e);
    }, context);

    signalR.eventAggregator.publish(event);
    failIfPublished = true;
    signalR.eventAggregator.unsubscribe(context);
    signalR.eventAggregator.publish(event);
});

test("When unsubscribing a context that never were subscribed", function () {
    var context = {};
    try {
        signalR.eventAggregator.unsubscribe(context);
    } catch (err) {
        ok(false, "Should not crash");
    }

    ok(true, "It should exit function call without exception");
});

throttleTest("When unsubscribing and subscribing directly after to server side events", function () {
    var event = constructEvent();
    var eventData = { type: event };

    var unsubscribeDone = false;
    var doneCallback = null;

    $.connection.eventAggregatorProxyHub.server.unsubscribe = function () {
        return {
            done: function (callback) {
                doneCallback = callback;
            }
        };
    };

    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
        ok(unsubscribeDone, "It should not call subscribe while unsubscribe is working");
    };

    signalR.eventAggregator.proxy.unsubscribe([eventData]);
    signalR.eventAggregator.proxy.subscribe(event);

    return function() {
        unsubscribeDone = true;
        doneCallback();
    };
});

throttleTest("When doing subsequent calls to subscribe Issue #12", function () {
    $.connection.eventAggregatorProxyHub.server.subscribe = function (arr) {
        equal(arr.length, 2, "Should throttle subscriptions");
    };

    var eventAggregator = new signalR.EventAggregator(true);

    for (var i = 0; i < 2; i++) {
        var event = constructEvent();
        
        eventAggregator.subscribe(event, function() {
        }, {});
    }
});

test("When not subscribing to any events from start", function () {
    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
        ok(false, "Should not call subscribe");
    };

    var eventAggregator = new signalR.EventAggregator(true);

    ok(true, "Did not call subscribe");
});

throttleTest("When a third party lib is traversing object tree that has reference to eventAggregator", function () {
    expect(0);
    var traverse = function (obj) {
        for (var member in obj) {
            var child = obj[member];

            if (child !== null && typeof child === "object") {
                traverse(obj[member]);
            }
        }
    };

    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
    };

    var event = constructEvent();

    var ViewModel = function () {
        signalR.eventAggregator.subscribe(event, this.onEvent, this);
        traverse(this);
    };

    ViewModel.prototype = {
        onEvent: function () {

        }
    };

    var vm = new ViewModel();
});

multipleSameEventSubscriptionTest = function(name, genericArgument, constraint, expectedCount) {
    throttleTest(name, function () {
        $.connection.eventAggregatorProxyHub.server.subscribe = function (s) {
            expectedCount = expectedCount || 1;
            equal(s.length, expectedCount, "It should subscribe " + expectedCount + " times to same event");
        };

        var event = function () {
        };
        event.proxyEvent = true;
        var constructor = event;
        
        var singleGenericArgument = (genericArgument == null || genericArgument.push == null);
        
        var singleConstraint = (constraint == null || constraint.push == null);

        var eventAggregator = new signalR.EventAggregator(true);


        for (var i = 0; i < 2; i++) {
            if (genericArgument != null) {
                event = {
                    genericConstructor: constructor,
                    genericArguments: [singleGenericArgument ? genericArgument : genericArgument[i]]
                };
            }

            eventAggregator.subscribe(event, function() {}, {}, singleConstraint ? constraint : constraint[i]);
        }

        return function () {
            ok(constraint == null || constructor.__subscribers[0].constraintId != null, "ConstraintId should be set");
            (singleConstraint ? equal : notEqual)(constructor.__subscribers[0].constraintId, constructor.__subscribers[1].constraintId, "ConstraintId should be correct");
        };
    });
};

multipleSameEventSubscriptionTest("When subscribing multiple times to same event - Issue #13");
multipleSameEventSubscriptionTest("When subscribing multiple times to same generic event - Issue #13", "One");
multipleSameEventSubscriptionTest("When subscribing multiple times to same constrained event - Issue #13", null, { id: 1 });
multipleSameEventSubscriptionTest("When subscribing multiple times to event with different constraint - Issue #13", null, [{ id: 1 }, { id: 2 }], 2);
multipleSameEventSubscriptionTest("When subscribing multiple times to event with different generic parameters - Issue #13", ["One", "Two"], null, 2);
multipleSameEventSubscriptionTest("When subscribing multiple times to same constrained generic event - Issue #13", "One", { id: 1 });

multipleSameEventUnsubscriptionTest = function (name, genericArgument, constraint, subscribeCount, unsubscribeCount) {
    throttleTest(name, function () {
        subscribeCount = subscribeCount || 2;
        unsubscribeCount = unsubscribeCount || subscribeCount - 1;
        var removeAll = subscribeCount - unsubscribeCount === 0;
        var contexts = [];
        var eventAggregator = new signalR.EventAggregator(true);

        if (removeAll)
            expect(2);

        $.connection.eventAggregatorProxyHub.server.subscribe = function () {
            for (var i = 0; i < unsubscribeCount; i++) {
                eventAggregator.unsubscribe(contexts[i]);
            }
            equal(constructor.__subscribers.length, subscribeCount - unsubscribeCount, "Should remove " + unsubscribeCount + " subscriptions client side");
        };
        
        $.connection.eventAggregatorProxyHub.server.unsubscribe = function (s) {
            ok(removeAll, "Should " + (removeAll ? "" : "not") + " unsubscribe");

            return {
                done: function() {
                }
            };
        };

        var event = function () {
        };
        event.proxyEvent = true;
        var constructor = event;

        var singleGenericArgument = (genericArgument == null || genericArgument.push == null);
        var singleConstraint = (constraint == null || constraint.push == null);
        for (var i = 0; i < 2; i++) {
            if (genericArgument != null) {
                event = {
                    genericConstructor: constructor,
                    genericArguments: [singleGenericArgument ? genericArgument : genericArgument[i]]
                };
            }
            var context = {};
            contexts.push(context);
            eventAggregator.subscribe(event, function () { }, context, singleConstraint ? constraint : constraint[i]);
        }
    });
};

multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same event - Issue #1");
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same event and unsubscribing all - Issue #1", null, null, 2, 2);
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same generic event - Issue #1", "One", null);
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to event with different generic parameters - Issue #13", ["One", "Two"], null);
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to same constrained event - Issue #13", null, { id: 1 });
multipleSameEventUnsubscriptionTest("When unsubscribing multiple times to event with different constraint - Issue #13", null, [{ id: 1 }, { id: 2 }]);

throttleTest("When two context subcribe to different gengeric arguments of same event", function() {
    expect(0);
    $.connection.eventAggregatorProxyHub.server.subscribe = function() {
    };

    var eventOne = constructEvent("One");
    var eventTwo = constructEvent("Two", eventOne);

    var assert = function(e) {
        ok(false, "It should not trigger on wrong generic event");
    };

    var eventAggregator = new signalR.EventAggregator(true);
    eventAggregator.subscribe(eventOne.event, assert, {});
    
    return function() {
        eventAggregator.publish(new eventOne.constructor(), eventTwo.event.genericArguments);
    }
});