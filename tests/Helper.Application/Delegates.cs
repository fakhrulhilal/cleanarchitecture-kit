namespace DevKit.Application;

/// <summary>
/// Synchronous command handler without response
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public delegate void SyncCommandCallback<in TRequest>(TRequest request) where TRequest : IRequest<Unit>;

/// <summary>
/// Asynchronous command handler without response
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public delegate Task CommandCallback<in TRequest>(TRequest request, CancellationToken cancellationToken)
    where TRequest : IRequest<Unit>;

public delegate TResponse SyncRequestCallback<in TRequest, out TResponse>(TRequest request)
    where TRequest : IRequest<TResponse>;

public delegate Task<TResponse> RequestCallback<in TRequest, TResponse>(TRequest request,
    CancellationToken cancellationToken) where TRequest : IRequest<TResponse>;
