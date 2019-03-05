Vue.use(signalR);

ClientSideEvent = function (message) {
    this.message = message;
};

new Vue({
    el: '#app',
    data: {
        text: "",
        events: []
    },
    computed: {
        canFire: function() {
            return this.text != null && this.text.trim() !== "";
        }
    },
    created: function() {
        this.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.StandardEvent, this.onEvent);
        this.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.GenericEvent.of("System.String"), this.onEvent);
        this.subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.ConstrainedEvent, this.onEvent, { message: "HelloWorld" });
        this.subscribe(ClientSideEvent, this.onEvent);
    },

    methods: {
        onEvent: function(e) {
            this.events.push(e);
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
            this.post("api/service/fireStandardEvent", this.text);
        },
        fireGenericEvent: function () {
            this.post("api/service/fireGenericEvent", this.text);
        },
        fireConstrainedEvent: function () {
            this.post("api/service/fireConstrainedEvent", this.text);
        },
        fireClientSideEvent: function () {
            this.publish(new ClientSideEvent(this.text));
        }
    }
});