using Azure;
using Azure.Core;
using Mediator;

namespace CustomersTask4.Abstraction
{
    public interface IAppMeditor
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        

    }

  

    public class AppMediator : IAppMeditor
    {
        private readonly IMediator _mediator;

        public AppMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(request, cancellationToken).AsTask();
        }
    }




    public interface ICustomRequest : IRequest<Unit> { }
    public interface ICustomRequest<out TResponse> : IRequest<TResponse> { }

    public interface ICustomRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    { }

    public interface ICustomRequestHandler<TRequest> : IRequestHandler<TRequest, Unit>
        where TRequest : IRequest<Unit>
    { }





}
