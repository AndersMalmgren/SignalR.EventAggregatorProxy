using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.AspNetCore.Middlewares
{
    public class EventScriptMiddleware
    {
        private readonly RequestDelegate _next;
        private static string js;
        private static DateTime scriptBuildDate;

        public EventScriptMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, IEventTypeFinder  eventTypeFinder)
        {
            if (js == null)
            {
                RenderScript(eventTypeFinder.ListEventsTypes());
            }

            var response = context.Response;
            response.ContentType = "application/javascript";
            response.StatusCode = 200;

            if (ClientCached(context.Request, scriptBuildDate))
            {
                response.StatusCode = 304;
                response.Headers["Content-Length"] = "0";
                response.Body.Close();
                response.Body = Stream.Null;

                return Task.FromResult<Object>(null);
            }

            response.Headers["Last-Modified"] = scriptBuildDate.ToUniversalTime().ToString("r");
            response.Headers["Cache-Control"] = "must-revalidate";
            return response.WriteAsync(js);
        }

        private bool ClientCached(HttpRequest request, DateTime contentModified)
        {
            string header = request.Headers["If-Modified-Since"];

            if (header != null)
            {
                DateTime isModifiedSince;
                if (DateTime.TryParse(header, out isModifiedSince))
                {
                    return isModifiedSince >= contentModified;
                }
            }

            return false;
        }


        private static void RenderScript(IEnumerable<Type> types)
        {
            var definitons = types.Select(t => new { @namespace = t.Namespace, name = t.GetNameWihoutGenerics(), generic = t.ContainsGenericParameters });
            var template = GetScriptTemplate();

            js = template.Replace("{{Data}}", JsonSerializer.Serialize(definitons));
            scriptBuildDate = types.Max(t => t.Assembly.GetBuildDate());
        }

        private static string GetScriptTemplate()
        {
            var stream = typeof(EventScriptMiddleware)
                .Assembly
                .GetManifestResourceStream("SignalR.EventAggregatorProxy.AspNetCore.Resources.EventProxy.js");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
