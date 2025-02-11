using DbAccess.Api.Models;
using DbAccess.Contracts;

namespace DbAccess.Api.Contracts;
public interface IPackageService : IDbExtendedRepository<Package, ExtPackage>
{

}
