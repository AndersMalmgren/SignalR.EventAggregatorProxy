using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.Owin;
using Newtonsoft.Json;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.Owin
{
    public class EventScriptMiddleware<TEvent> : OwinMiddleware
    {
        private static string js;
        private static DateTime scriptBuildDate;

        static EventScriptMiddleware()
        {
            if (js == null)
            {
                RenderScript();
            }
        }

        public EventScriptMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            try
            {
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

                return response.WriteAsync(js);
            }
            catch (Exception e)
            {
                File.AppendAllText("c:\\temp\\log.log", string.Format("{0}: {1}", e.Message, e.StackTrace));
                throw;
            }
        }

        private bool ClientCached(IOwinRequest request, DateTime contentModified)
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


        private static void RenderScript()
        {
            var types = GetEventTypes();

            var definitons = types.Select(t => new { @namespace = t.Namespace, name = t.GetNameWihoutGenerics(), generic = t.ContainsGenericParameters });
            var template = GetScriptTemplate();

            js = template.Replace("{{Data}}", Serialize(definitons));
            scriptBuildDate = types.Max(t => t.Assembly.GetBuildDate());
        }

        private static string GetScriptTemplate()
        {
            var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("SignalR.EventAggregatorProxy.Resources.EventProxy.js");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static IEnumerable<Type> GetEventTypes()
        {
            return GlobalHost.DependencyResolver.Resolve<ITypeFinder>()
                .ListEventTypes();
        }

        private static string Serialize(object obj)
        {
            var jsonSerializer = GlobalHost.DependencyResolver.Resolve<JsonSerializer>();
            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);

            jsonSerializer.Serialize(obj, writer);
            return stringBuilder.ToString();
        }
    }
}
