$.connection = {
    eventAggregatorProxyHub: {
        client: {},
        server: {}
        
    },
    hub: {
        start: function() {
            return {
                done: function(callback) {
                    callback();
                }
            };
        },
        reconnected: function() {
            
        }
    }
};
