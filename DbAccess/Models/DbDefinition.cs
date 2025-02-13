namespace DbAccess.Models;

public class DbDefinition(Type type)
{
    public Type BaseType { get; set; } = type;

    public List<ColumnDefinition> Columns { get; set; } = new();
    public List<ForeignKeyDefinition> ForeignKeys { get; set; } = new();
    public List<ConstraintDefinition> UniqueConstraints { get; set; } = new();
    public List<RelationDefinition> Relations { get; set; } = new();

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
