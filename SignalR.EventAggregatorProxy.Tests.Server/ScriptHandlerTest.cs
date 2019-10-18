using System;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SignalR.EventAggregatorProxy.AspNetCore.Middlewares;
using SignalR.EventAggregatorProxy.Event;


namespace SignalR.EventAggregatorProxy.Tests.Server
{
    [TestClass]
    public class When_rendering_proxy_script : Test
    {
        private string script;
        private const string Expected = "[{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"NoMembersEvent\",\"generic\":false},{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"MembersEvent\",\"generic\":false}]";

        protected override void ConfigureCollection(IServiceCollection serviceCollection)
        {
            serviceCollection
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x  => x.ListEventsTypes()).Returns(new[] { typeof(NoMembersEvent), typeof(MembersEvent) }));
        }

        [TestInitialize]
        public void Context()
        {
            var context = new DefaultHttpContext();
            var mem = new MemoryStream();
            context.Response.Body = mem;

            var eventScriptMiddleware = new EventScriptMiddleware(null);
            eventScriptMiddleware.Invoke(context, Get<IEventTypeFinder>());
            script = Encoding.UTF8.GetString(mem.ToArray());
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(script.Contains(Expected));
        }
    }
}
