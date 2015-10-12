angular.module("signalR.eventAggregator", [])
	.run([
		"$rootScope", function($rootScope) {
        function createScope(scope) {
            scope.$on('$destroy', function() {
                signalR.eventAggregator.unsubscribe(scope);
            });

            return {
                subscribe: function(type, handler, constraint) {
                    signalR.eventAggregator.subscribe(type, function(e) {
                        handler(e);
                        if (scope.$$phase == null) {
                            scope.$digest();
                        }
                    }, scope, constraint);
                },
                publish: function(event) {
                    signalR.eventAggregator.publish(event);
                }
            }
        }

        $rootScope.eventAggregator = function() {
            return this.__eventAggregator = this.__eventAggregator || createScope(this);
        };
    }
]);