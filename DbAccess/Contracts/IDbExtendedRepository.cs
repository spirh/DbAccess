using DbAccess.Models;

namespace DbAccess.Contracts;

public interface IDbExtendedRepository<T, TExtended> : IDbBasicRepository<T>
{
    Task<TExtended> GetExtended(Guid id);
    Task<IEnumerable<TExtended>> GetExtended();
    Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filter);
    Task<IEnumerable<TExtended>> GetExtended(IEnumerable<GenericFilter> filters);
}
