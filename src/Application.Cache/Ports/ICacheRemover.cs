namespace DevKit.Application.Ports;

public interface ICacheRemover<in TRequest>
{
    Task RemoveAsync(TRequest request, CancellationToken cancellationToken);
}
