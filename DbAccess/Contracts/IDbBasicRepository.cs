using DbAccess.Models;

namespace DbAccess.Contracts;

public interface IDbBasicRepository<T>
{
    Task<T> Get(Guid id);
    Task<IEnumerable<T>> Get();
    Task<IEnumerable<T>> Get(GenericFilterBuilder<T> filter);
    Task<int> Insert(T entity, CancellationToken cancellationToken = default);
    Task Update(Guid id, T entity);
    Task Delete(Guid id);
    void Define();
}
