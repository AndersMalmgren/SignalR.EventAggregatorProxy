window.TestEvent = function() {

};

test("Event aggregator class should have correct casing on closure (signalR) Issue #1 ", function () {
    ok(window.signalR !== undefined);
    ok(window.signalR.eventAggregator !== undefined);
});

test("When subscribing to client side event Issue #2", function () {
    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
        ok(false, "Server side subscribe should not be called for client side events");
    };

    signalR.eventAggregator.subscribe(TestEvent, function() {
    }, {});
    ok(true, "Server side subscribe was not called");

    $.connection.eventAggregatorProxyHub.server.subscribe = null;
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

test("When unsubscribing and subscribing directly after to server side events", function () {
    var event = function () {

    };
    event.proxyEvent = true;
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

    unsubscribeDone = true;
    doneCallback();
});



test("When not subscribing to any events from start", function () {
    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
        ok(false, "Should not call subscribe");
    };

    var eventAggregator = new signalR.EventAggregator(true);

    ok(true, "Did not call subscribe");
});

asyncTest("When a third party lib is traversing object tree that has reference to eventAggregator", function () {
    var traverse = function (obj) {
        for (var member in obj) {
            traverse(obj[member]);
        }
    };

    $.connection.eventAggregatorProxyHub.server.subscribe = function () {
    };

    var event = function () {

    };
    event.proxyEvent = true;

    var ViewModel = function () {
        signalR.eventAggregator.subscribe(event, this.onEvent, this);
        this.sub = event;
        ko.validation.group(this, { observable: true, deep: true });
    };

    ViewModel.prototype = {
        onEvent: function () {

        }
    };

    var vm = new ViewModel();

    setTimeout(function () {
        start();

        ok("It should not overflow stack");
    }, 5);

});