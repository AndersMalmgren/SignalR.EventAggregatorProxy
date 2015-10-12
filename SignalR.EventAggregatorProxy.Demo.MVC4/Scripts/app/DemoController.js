angular.module("demo", ["signalR.eventAggregator"])
	.controller("DemoController", [
	"$scope", "$http", function($scope, $http) {
        $scope.text = null;
        $scope.events = [];

        $scope.canFire = function() {
            return $scope.text != null && $scope.text.trim() != "";
        };

        $scope.fireStandardEvent = function() {
            post("/api/service/fireStandardEvent", $scope.text);
        };
        $scope.fireGenericEvent = function() {
            post("/api/service/fireGenericEvent", $scope.text);
        };
        $scope.fireConstrainedEvent = function() {
            post("/api/service/fireConstrainedEvent", $scope.text);
        }
        $scope.fireClientSideEvent = function() {
            $scope.eventAggregator().publish(new ClientSideEvent($scope.text));
        };

        function post(url, data) {
            $http.post(url, angular.toJson(data));
        }

        function onEvent(e) {
            $scope.events.push(e);
        };

        $scope.eventAggregator().subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.StandardEvent, onEvent);
        $scope.eventAggregator().subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.GenericEvent.of("System.String"), onEvent);
        $scope.eventAggregator().subscribe(SignalR.EventAggregatorProxy.Demo.Contracts.Events.ConstrainedEvent, onEvent, { message: "HelloWorld" });
	    $scope.eventAggregator().subscribe(ClientSideEvent, onEvent);
        
    }
]);