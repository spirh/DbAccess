using AccessDemo.Common.Models;
using DbAccess.Contracts;

namespace AccessDemo.Common.Contracts;

public interface IPackageResourceService : IDbCrossRepository<PackageResource, ExtPackageResource, Package, Resource> { }
