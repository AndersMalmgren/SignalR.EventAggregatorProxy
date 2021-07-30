(window.stubHub = function () {
    window.signalR = window.signalR || {};
    window.signalR.HubConnectionBuilder = function() {

    };

    var promiseStub = function () {

    };

    promiseStub.prototype = {
        then: function(callback) {
            callback();
            return this;
        },
        catch: function(callback) {
            return this;
        }
    };

    window.signalR.HubConnectionBuilder.prototype =
    {
        withUrl: function() { return this },
        build: function () {
            return window.currentHub = window.currentHub ||
            {
                invoke: function() {
                    throw "Invoke called without test setting up assert on this";
                },
                start: function() {
                    return new promiseStub();
                },
                on: function() {},
                onclose: function(callback) {
                    window.currentOnClose = callback;
                }
            };
        }
    };
})();
