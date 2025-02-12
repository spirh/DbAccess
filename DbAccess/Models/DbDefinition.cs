namespace DbAccess.Models;

public class DbDefinition(Type type)
{
    public Type BaseType { get; set; } = type;

    public List<ColumnDefinition> Columns = new();
    public List<ForeignKeyDefinition> ForeignKeys = new();
    public List<ConstraintDefinition> UniqueConstraints = new();
    public List<RelationDefinition> Relations = new();
    public bool HasTranslation = false;
    public bool HasHistory = false;

    public string DefaultLanguage { get; set; } = "nob"; // no-NB?

    public string BaseSchema { get; set; } = "dbo";

    public string TranslationSchema { get; set; } = "translation";
    public string TranslationAliasPrefix { get; set; } = "t_"; // translation view name?

    public string BaseHistorySchema { get; set; } = "dbo_history";
    public string HistoryAliasPrefix { get; set; } = "h_"; //History view name?  nei... bare alias

    public string TranslationHistorySchema { get; set; } = "translation_history";
}
