namespace AccessDemo.Common.Models;

public class Area
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
/// <summary>
/// Extended Area
/// </summary>
public class ExtArea : Area
{

    /// <summary>
    /// Packages
    /// </summary>
    public List<Package> Packages { get; set; }
}
