namespace AccessDemo.Common.Models;

public class Package
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid AreaId { get; set; }
}

public class ExtPackage : Package
{
    public Area Area { get; set; }
}
