test("Event aggregator class should have correct casing on closure (signalR) Issue #1 ", function() {
    ok(window.signalR !== undefined);
    ok(window.signalR.eventAggregator !== undefined);
});