using System.ComponentModel.DataAnnotations;

namespace DbAccess.Models;

public class DbAccessConfig
{
    public string DatabaseType { get; set; } = "Postgres";
    public string ConnectionString { get; set; } = string.Empty;
}
