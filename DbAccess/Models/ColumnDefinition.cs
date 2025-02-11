namespace DbAccess.Models;

public class ColumnDefinition
{
    public Type Type { get; set; }
    public string Name { get; set; }
    public bool IsNullabe { get; set; }
    public string? DefaultValue { get; set; }
    public int? Length { get; set; }
}
