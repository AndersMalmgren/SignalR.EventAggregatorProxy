using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalR.EventAggregatorProxy.AspNetCore.Middlewares;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Extensions;


namespace SignalR.EventAggregatorProxy.Tests.Server
{
    public class ProxyScriptMiddlewareTest : Test
    {
        protected string Script;
        protected DateTime LastModified;
        protected DateTime LastWriteToAssembly;

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            serviceCollection
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x => x.ListEventsTypes()).Returns(new[] { typeof(NoMembersEvent), typeof(MembersEvent) }));

            LastWriteToAssembly = File.GetLastWriteTimeUtc(typeof(NoMembersEvent).Assembly.Location).StripMilliseconds();
            Console.WriteLine($"Assembly last write: {LastWriteToAssembly}");
        }

        public void Setup(DateTime? cached = null)
        {
            var context = new DefaultHttpContext();
            var mem = new MemoryStream();
            context.Response.Body = mem;
            if (cached.HasValue)
            {
                context.Request.Headers["If-Modified-Since"] = cached.Value.StripMilliseconds().ToString("r");
            }

            var eventScriptMiddleware = new EventScriptMiddleware(null);
            eventScriptMiddleware.Invoke(context, Get<IEventTypeFinder>());
            Script = Encoding.UTF8.GetString(mem.ToArray());
            DateTime.TryParse(context.Response.Headers["Last-Modified"], out LastModified);
            LastModified = LastModified.ToUniversalTime();
        }
    }

    [TestClass]
    public class When_rendering_proxy_script : ProxyScriptMiddlewareTest
    {
        private const string Expected = "[{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"NoMembersEvent\",\"generic\":false},{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"MembersEvent\",\"generic\":false}]";
        
        [TestInitialize]
        public void Context()
        {
            Setup();
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(Script.Contains(Expected));
            Assert.AreEqual(LastWriteToAssembly, LastModified);
        }
    }

    [TestClass]
    public class When_rendering_proxy_script_with_stale_cache : ProxyScriptMiddlewareTest
    {
        [TestInitialize]
        public void Context()
        {
            Setup(LastWriteToAssembly.AddHours(-1));
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(Script));
        }
    }

    [TestClass]
    public class When_rendering_proxy_script_with_up_to_date_cache : ProxyScriptMiddlewareTest
    {
        [TestInitialize]
        public void Context()
        {
            Setup(LastWriteToAssembly.AddHours(1));
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(string.IsNullOrEmpty(Script));
        }
    }

    [TestClass]
    public class When_rendering_proxy_script_with_up_to_date_cache_same_date : ProxyScriptMiddlewareTest
    {
        [TestInitialize]
        public void Context()
        {
            Setup(LastWriteToAssembly);
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(string.IsNullOrEmpty(Script));
        }
    }
}
