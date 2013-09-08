using System.IO;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.ScriptProxy;

namespace SignalR.EventAggregatorProxy.Tests.Server
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
    public class When_rendering_proxy_script : ServerTest
    {
        private string script;
        private const string Expected = "[{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"NoMembersEvent\",\"generic\":false},{\"namespace\":\"SignalR.EventAggregatorProxy.Tests.Server\",\"name\":\"MembersEvent\",\"generic\":false}]";

        [TestInitialize]
        public void Context()
        {
            WhenCalling<ITypeFinder>(x => x.ListEventTypes()).Return(new[] {typeof (NoMembersEvent), typeof (MembersEvent)});

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
