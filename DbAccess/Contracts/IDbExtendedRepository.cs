using DbAccess.Models;

namespace DbAccess.Contracts;

public interface IDbExtendedRepository<T, TExtended> : IDbBasicRepository<T>
{
    Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter, RequestOptions? options = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<TExtended>> GetExtended(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default);
}
