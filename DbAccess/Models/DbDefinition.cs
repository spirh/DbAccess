namespace DbAccess.Models;

public abstract class DbDefinition
{
    public readonly List<ColumnDefinition> Columns = new();
    public readonly List<ForeignKeyDefinition> ForeignKeys = new();
    public readonly List<ConstraintDefinition> UniqueConstraints = new();
    public readonly List<RelationDefinition> Relations = new();
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