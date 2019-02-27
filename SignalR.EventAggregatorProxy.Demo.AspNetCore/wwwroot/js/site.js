var model = function() {
    signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.AspNetCore.DemoEvent, this.onmessage, this);

    this.hub = new signalR.HubConnectionBuilder().withUrl("/myhub").build();
    this.hub.on("foo", function(message) {
        console.log("Ftom other hub");
    });
    this.hub.start();
}

model.prototype = {
    onmessage: function(message) {
        console.log(message)
    }
};


new model();