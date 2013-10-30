using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Owin;

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
            WhenCalling<IOwinResponse>(x => x.WriteAsync(Arg<string>.Is.Anything))
                .Return(null)
                .Callback<string>(s =>
                    {
                        script = s;
                        return true;
                    });

            WhenCalling<ITypeFinder>(x => x.ListEventTypes()).Return(new[] {typeof (NoMembersEvent), typeof (MembersEvent)});
            Mock<IHeaderDictionary>();
            
            WhenAccessing<IOwinRequest, IHeaderDictionary>(x => x.Headers).Return(Get<IHeaderDictionary>());
            WhenAccessing<IOwinResponse, IHeaderDictionary>(x => x.Headers).Return(Get<IHeaderDictionary>());

            WhenAccessing<IOwinContext, IOwinResponse>(x => x.Response).Return(Get<IOwinResponse>());
            WhenAccessing<IOwinContext, IOwinRequest>(x => x.Request).Return(Get<IOwinRequest>());
            

            var eventScriptMiddleware = new EventScriptMiddleware<TestEventBase>(null);

            eventScriptMiddleware.Invoke(Get<IOwinContext>());
            //script = context.Response.Output.ToString();
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(script.Contains(Expected));
        }
    }
}
