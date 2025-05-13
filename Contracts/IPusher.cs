namespace MvcMovie.Contracts;

public interface IPusher<T>
{
    public Task PushAsync(T data,int type, CancellationToken cancellationToken = default);
    Task PushManyAsync(IEnumerable<T> products, int type, CancellationToken cancellationToken);
}
