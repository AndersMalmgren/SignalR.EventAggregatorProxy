(function (signalR) {
    signalR.install = function (vue) {
        vue.mixin({
            destroyed: function () {
                signalR.eventAggregator.unsubscribe(this);
            }
        });

        vue.config.globalProperties.subscribe = function (type, handler, constraint) {
            signalR.eventAggregator.subscribe(type, handler, this, constraint);
        };

        vue.config.globalProperties.publish = function (event) {
            signalR.eventAggregator.publish(event);
        };
    };
})(signalR || {});