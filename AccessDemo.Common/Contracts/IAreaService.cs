using AccessDemo.Common.Models;
using DbAccess.Contracts;

namespace AccessDemo.Common.Contracts;

public interface IAreaService : IDbExtendedRepository<Area, ExtArea>
{

}
