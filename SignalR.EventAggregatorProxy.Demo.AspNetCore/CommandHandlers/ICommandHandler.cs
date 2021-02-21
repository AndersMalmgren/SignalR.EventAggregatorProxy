using System.Threading.Tasks;
using SignalR.EventAggregatorProxy.Demo.Contracts.Commands;

namespace SignalR.EventAggregatorProxy.Demo.AspNetCore.CommandHandlers
{
        public interface ICommandHandler<in TCommand> where TCommand : ICommand
        {
            Task Handle(TCommand cmd);
        }
}