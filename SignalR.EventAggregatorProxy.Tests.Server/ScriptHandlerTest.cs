using System.IO;
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
                .MockSingleton<IEventTypeFinder>(mock => mock.Setup(x  => x.ListEventsTypes()).Returns(new[] { typeof(NoMembersEvent), typeof(MembersEvent) }))
                .MockSingleton<HttpContext>(mock =>
                {
                    mock.Setup(x => x.Request.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                    mock.Setup(x => x.Response.Headers).Returns(new Mock<IHeaderDictionary>().Object);
                    mock.Setup(x => x.Response.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                        {
                            if (length > 0)
                                script = Encoding.UTF8.GetString(data); 
                        });
                });

        }

        [TestInitialize]
        public void Context()
        {
            var eventScriptMiddleware = new EventScriptMiddleware(null);
            eventScriptMiddleware.Invoke(Get<HttpContext>(), Get<IEventTypeFinder>());
        }

        [TestMethod]
        public void It_should_render_script_correctly()
        {
            Assert.IsTrue(script.Contains(Expected));
        }
    }
}
