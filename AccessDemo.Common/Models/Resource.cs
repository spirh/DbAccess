namespace AccessDemo.Common.Models;

public class Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
public class ExtResource : Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
