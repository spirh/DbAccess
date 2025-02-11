using DbAccess.Api.Models;
using DbAccess.Contracts;

namespace DbAccess.Api.Contracts;

public interface IPackageResourceService : IDbCrossRepository<PackageResource, ExtPackageResource, Package, Resource> { }
