using System;
using System.IO;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.ScriptProxy;

namespace SignalR.EventAggregatorProxy.Tests
{
    public abstract class TestEventBase
    {
        
    }

    public class NoMembersEvent : TestEventBase
    {
        
    }

    public class MembersEvent : TestEventBase
    {
        public string TestPropety { get; set; }
    }


    [TestClass]
    public class When_rendering_proxy_script
    {
        private string script;
        private const string Expected = "[{\"namespace\":\"SignalR.EventAggregatorProxy.Tests\",\"name\":\"NoMembersEvent\"},{\"namespace\":\"SignalR.EventAggregatorProxy.Tests\",\"name\":\"MembersEvent\"}]";

        [TestInitialize]
        public void Context()
        {
            var typeFinder = MockRepository.GenerateMock<ITypeFinder>();
            typeFinder.Stub(x => x.ListEventTypes()).Return(new[] {typeof (NoMembersEvent), typeof (MembersEvent)});

            GlobalHost.DependencyResolver.Register(typeof(ITypeFinder), () => typeFinder);
            var handler = new ScriptHandler<TestEventBase>();
            var context = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            handler.ProcessRequest(context);
            script = context.Response.Output.ToString();
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(script.Contains(Expected));
        }
    }
}
