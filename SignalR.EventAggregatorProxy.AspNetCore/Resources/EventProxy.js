
(function(signalR, definitions) {
    var events = {};
    definitions.forEach(function (definition) {
        var type = definition.namespace + "." + definition.name;
        var closure = getClosure(window, definition.namespace.split("."));
        var $class = closure[definition.name] = function() {
        };
        
        if (definition.generic) {
            $class.of = function() {
                return {
                    genericConstructor:  $class,
                    genericArguments: mapArgumentsToArray(arguments)
                };
            };
        }
        $class.type = type;
        $class.proxyEvent = true;
        events[type] = $class;
    });

    function getClosure(root, namespace) {
        if (namespace.length == 0) {
            return root;
        }
        var part = namespace[0];
        namespace.splice(0, 1);
        root[part] = root[part] || {};

        return getClosure(root[part], namespace);
    }
    
    function mapArgumentsToArray(genericArguments) {
        //SignalR does not like function argument arrays so we clone it
        return genericArguments != null ? Array.from(genericArguments) : null;
    }

    signalR.getEvent = function(type) {
        return events[type];
    };

})(window.signalR = window.signalR || {},  {{Data}});  