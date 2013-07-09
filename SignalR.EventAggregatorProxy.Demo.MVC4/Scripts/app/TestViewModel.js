TestViewModel = function () {
    this.text = ko.observable();
    this.events = ko.observableArray();

    this.subscribe();
};

TestViewModel.prototype = {
    subscribe: function () {
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.MVC4.Events.StandardEvent, this.onEvent, this);
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.MVC4.Events.GenericEvent.of("System.String"), this.onEvent, this);
        signalR.eventAggregator.subscribe(SignalR.EventAggregatorProxy.Demo.MVC4.Events.ConstrainedEvent, this.onEvent, this, { message: "HelloWorld" });
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
    fireStandardEvent: function () {
        this.post("api/service/fireStandardEvent", this.text());
    },
    fireGenericEvent: function () {
        this.post("api/service/fireGenericEvent", this.text());
    },
    fireConstrainedEvent: function () {
        this.post("api/service/fireConstrainedEvent", this.text());
    }
};

ko.applyBindings(new TestViewModel());