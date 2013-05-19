
(function(signalR, definitions) {
    var events = {};
    $.each(definitions, function(index, defintion) {
        var type = defintion.namespace + "." + defintion.name;
        var closure = getClosure(window, defintion.namespace.split("."));
        var $class = closure[defintion.name] = function() {
        };
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

    signalR.getEvent = function(type) {
        return events[type];
    };

})(window.signalR = window.signalR || {},  {{Data}});  