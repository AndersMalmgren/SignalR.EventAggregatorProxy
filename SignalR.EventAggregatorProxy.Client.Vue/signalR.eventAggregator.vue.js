(function (signalR, vue) {
    signalR.install = function (vue) {
        vue.mixin({
            destroyed: function () {
                signalR.eventAggregator.unsubscribe(this);
            }
        });

        vue.prototype.subscribe = function (type, handler, constraint) {
            signalR.eventAggregator.subscribe(type, handler, this, constraint);
        };

        vue.prototype.publish = function (event) {
            signalR.eventAggregator.publish(event);
        };
    };
})(signalR || {}, Vue);