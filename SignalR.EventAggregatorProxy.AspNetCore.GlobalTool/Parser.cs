﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.AspNetCore.GlobalTool
{
    internal class Parser
    {
        private readonly IEnumerable<Type> eventTypes;

        public Parser(IEnumerable<Type> eventTypes)
        {
            this.eventTypes = eventTypes;
        }

        public string Parse()
        {
            return $@"/*
 * Content generated by a tool, do not edit. See https://github.com/AndersMalmgren/SignalR.EventAggregatorProxy/wiki/Donet-CLI
 */
(function(signalR) {{
    var events = {{}};
{string.Join(Environment.NewLine, BuildNamespace(eventTypes).Select(ns => $"    {ns}"))}

{string.Join(Environment.NewLine, BuildContracts(eventTypes))}

    function mapArgumentsToArray(genericArguments) {{
            //SignalR does not like function argument arrays so we clone it
            return genericArguments != null ? Array.from(genericArguments) : null;
    }}

    signalR.getEvent = function(type) {{
            return events[type];
    }};

}})(window.signalR = window.signalR || {{}});
";
        }
        public IEnumerable<string> BuildNamespace(IEnumerable<Type> contracts)
        {
            var window = new Namespace();
            foreach (var contract in contracts)
            {
                window.Add(contract.Namespace.Split('.'));
            }

            return window.Render();
        }

        public IEnumerable<string> BuildContracts(IEnumerable<Type> contracts)
        {
            foreach (var contract in contracts)
            {
                var typeName = contract.GetFullNameWihoutGenerics();
                var properties = contract.GetProperties();
                var generics = string.Empty;
                if (contract.ContainsGenericParameters)
                {
                    generics = @"
        $class.of = function() {
            return {
                genericConstructor:  $class,
                genericArguments: mapArgumentsToArray(arguments)
            };
        };
";
                }


                yield return $@"    (function() {{
        var $class=function() {{";

                foreach (var property in properties)
                {
                    yield return $"           this.{CamelCased(property.Name)}=undefined;";
                }

                yield return $@"        }};
        $class.type=""{typeName}"";{generics}      
        $class.proxyEvent = true;
        {typeName} = $class;
        events[$class.type] = $class;
    }})();";
            }
        }

        public string CamelCased(string pascalCased)
        {
            return pascalCased.Substring(0, 1).ToLower() + pascalCased.Substring(1);
        }
    }
}
