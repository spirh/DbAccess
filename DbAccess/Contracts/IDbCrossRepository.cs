namespace DbAccess.Contracts;

public interface IDbCrossRepository<T, TExtended, TA, TB> : IDbExtendedRepository<T, TExtended>
{
    Task<IEnumerable<TA>> GetA(Guid id);
    Task<IEnumerable<TB>> GetB(Guid id);
}