using System.Reflection;

namespace DbAccess.Models;

public class ColumnDefinition
{
    public PropertyInfo Property { get; set; }
    public string Name { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public int? Length { get; set; }
}
