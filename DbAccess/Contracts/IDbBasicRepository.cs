using DbAccess.Models;

namespace DbAccess.Contracts;

public interface IDbBasicRepository<T>
{
    Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filterBuilder, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default);


    Task<int> Ingest(List<T> data, CancellationToken cancellationToken = default);

    Task<int> Create(T entity, CancellationToken cancellationToken = default);

    Task<int> Upsert(Guid id, T entity, CancellationToken cancellationToken = default);

    Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default);
    Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default);

    Task<int> Delete(Guid id, CancellationToken cancellationToken = default);

    Task<int> CreateTranslation(T obj, string language, CancellationToken cancellationToken = default);
    Task<int> UpdateTranslation(Guid id, T obj, string language, CancellationToken cancellationToken = default);

    //void Define();
}
