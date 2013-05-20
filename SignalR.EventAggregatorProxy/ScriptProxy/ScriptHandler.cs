using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;

namespace SignalR.EventAggregatorProxy.ScriptProxy
{
    public class ScriptHandler<TEvent> : IHttpHandler
    {
        private static string js;
        private static DateTime scriptBuildDate;

        public ScriptHandler()
        {
            if (js == null)
            {
                RenderScript();
            }            
        }

        public void ProcessRequest(HttpContext context)
        {
            var response = context.Response;
            response.ContentType = "text/javascript";

            if (ClientCached(context, scriptBuildDate))
            {
                response.StatusCode = 304;
                response.StatusDescription = "Not Modified";
                response.AddHeader("Content-Length", "0");
                return;
            }

            response.Cache.SetLastModified(scriptBuildDate);

            response.Write(js);
        }

        private void RenderScript()
        {
            var types = GetEventTypes();

            var definitons = types.Select(t => new { @namespace = t.Namespace, name = t.Name });
            var template = GetScriptTemplate();

            js = template.Replace("{{Data}}", Serialize(definitons));
            scriptBuildDate = types.Max(t => t.Assembly.GetBuildDate());
        }

        private string GetScriptTemplate()
        {
            var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("SignalR.EventAggregatorProxy.Resources.EventProxy.js");

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private IEnumerable<Type> GetEventTypes()
        {
            return GlobalHost.DependencyResolver.Resolve<ITypeFinder>()
                .ListEventTypes();
        }

        private string Serialize(object obj)
        {
            var jsonSerializer = GlobalHost.DependencyResolver.Resolve<IJsonSerializer>();
            var stringBuilder = new StringBuilder();
            var writer = new StringWriter(stringBuilder);

            jsonSerializer.Serialize(obj, writer);
            return stringBuilder.ToString();
        }

        private bool ClientCached(HttpContext context, DateTime contentModified)
        {
            string header = context.Request.Headers["If-Modified-Since"];

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

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
