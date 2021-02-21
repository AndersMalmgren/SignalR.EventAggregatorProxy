using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SignalR.EventAggregatorProxy.Demo.AspNetCore.CommandHandlers;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;
using SignalR.EventAggregatorProxy.Demo.Contracts.Events;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class ServiceController : ControllerBase
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IServiceProvider serviceProvider;

        public ServiceController(IEventAggregator eventAggregator, IServiceProvider serviceProvider)
        {
            this.eventAggregator = eventAggregator;
            this.serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Route("FireStandardEvent")]
        public Task FireStandardEvent([FromBody]string text)
        {
            return eventAggregator.Publish(new StandardEvent(text));
        }

        [HttpPost]
        [Route("FireGenericEvent")]
        public Task FireGenericEvent([FromBody]string text)
        {
            return eventAggregator.Publish(new GenericEvent<string>(text));
        }

        [HttpPost]
        [Route("FireConstrainedEvent")]
        public Task FireConstrainedEvent([FromBody] string text)
        {
            return eventAggregator.Publish(new ConstrainedEvent(text));
        }

        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
        private static readonly MethodInfo Method = typeof(ServiceController).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(m => m.Name == nameof(ExecuteInternal) && m.IsGenericMethod);

        [HttpPost]
        [Route("ExecuteCommand")]
        public async Task ExecuteCommand([FromBody] CmdDto cmd)
        {
            var type = Type.GetType(cmd.Type);
            await (Task) Method.MakeGenericMethod(type).Invoke(this, new [] {cmd.Command.GetRawText()});
        }

        private async Task ExecuteInternal<TCommand>(string json) where TCommand : ICommand
        {
            var command = JsonSerializer.Deserialize<TCommand>(json, Options);

            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.Handle(command);
        }

        public class CmdDto
        {
            public string Type { get; set; }
            public JsonElement Command { get; set; }
        }
    }
}