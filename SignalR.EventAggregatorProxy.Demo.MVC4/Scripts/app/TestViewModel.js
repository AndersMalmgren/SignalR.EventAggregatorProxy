ClientSideEvent = function(message) {
    this.message = message;
};

TestViewModel = function () {
    this.text = ko.observable();
    this.events = ko.observableArray();
    this.canFire = ko.computed(this.getCanFire, this);

    this.subscribe();
};

TestViewModel.prototype = {
    subscribe: function () {
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.StandardEvent, this.onEvent, this);
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.GenericEvent.of("System.String"), this.onEvent, this);
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.ConstrainedEvent, this.onEvent, this, { message: "HelloWorld" });
        signalR.eventAggregator.subscribe(ClientSideEvent, this.onEvent, this);

        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.ConnectionStateChangedEvent, this.onEvent, this);

        this.hub = $.connection.connectionListenerHub;
        this.hub.client.dummy = function() {};
    },
    onEvent: function (message) {
        this.events.push(message);
    },
    post: function (url, data) {
        $.ajax({
            url: url,
            type: 'POST',
            data: ko.toJSON(data),
            contentType: "application/json;charset=utf-8"
        });
    },
    getCanFire: function() {
        return this.text() != null && this.text().trim() != "";
    },
    fireStandardEvent: function () {
        this.post("api/service/fireStandardEvent", this.text());
    },
    fireGenericEvent: function () {
        this.post("api/service/fireGenericEvent", this.text());
    },
    fireConstrainedEvent: function () {
        this.post("api/service/fireConstrainedEvent", this.text());
    },
    fireClientSideEvent: function () {
        signalR.eventAggregator.publish(new ClientSideEvent(this.text()));
    }
};

ko.applyBindings(new TestViewModel());