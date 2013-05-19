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

    signalR.eventAggregator.subscribe(new TestEvent(), function() {
    }, {});
    ok(true, "Server side subscribe was not called");
});



//subscribe