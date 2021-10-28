namespace MediatR.Pipeline
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Behavior for executing all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> instances after handling the request
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestPostProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _postProcessors;

        public RequestPostProcessorBehavior(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> postProcessors) 
            => _postProcessors = postProcessors;

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = await next().ConfigureAwait(false);

            var requestProcessors = _postProcessors;
#if NET
            requestProcessors = requestProcessors.OrderBy(p => p.Order);
#endif
            foreach (var processor in requestProcessors)
            {
                await processor.Process(request, response, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }
    }
}
