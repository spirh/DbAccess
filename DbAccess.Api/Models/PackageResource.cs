namespace DbAccess.Api.Models;

public class PackageResource
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public Guid ResourceId { get; set; }
}

public class ExtPackageResource : PackageResource
{
    public Package Package { get; set; }
    public Resource Resource { get; set; }
}