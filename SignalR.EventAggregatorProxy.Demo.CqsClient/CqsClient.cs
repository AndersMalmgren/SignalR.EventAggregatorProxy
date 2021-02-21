using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;

namespace SignalR.EventAggregatorProxy.Demo.CqsClient
{
    public class CqsClient : ICqsClient
    {
        private readonly HttpClient client;

        public CqsClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task ExecuteCommand<TCommand>(TCommand cmd) where TCommand : ICommand
        {
            var dto = new { Command = cmd, Type = cmd.GetType().AssemblyQualifiedName };

            var result = await client
                .PostAsJsonAsync(new Uri($"/api/service/executecommand", UriKind.Relative), dto);

            if (!result.IsSuccessStatusCode) throw new Exception(await result.Content.ReadAsStringAsync());
        }
    }
}
