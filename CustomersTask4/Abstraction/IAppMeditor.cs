using Wolverine;

namespace CustomersTask4.Abstraction
{
    public interface IAppMeditor
    {
        Task<TResponse> Send<TResponse>(object command, CancellationToken cancellationToken = default);
        Task PublishAsync<T>(T message);
        Task Send(object command, CancellationToken cancellationToken = default);
    }

    public class AppMediator(IMessageBus bus) : IAppMeditor
    {
        public async Task PublishAsync<T>(T message)
        {
            await bus.PublishAsync(message);
        }

        public Task<TResponse> Send<TResponse>(object command, CancellationToken cancellationToken = default)
        {
            return bus.InvokeAsync<TResponse>(command, cancellationToken);
        }

        public Task Send(object command, CancellationToken cancellationToken = default)
        {
            return bus.InvokeAsync(command, cancellationToken);
        }
    }
}
