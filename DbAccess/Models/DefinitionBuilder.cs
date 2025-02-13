using System.Linq.Expressions;
using System.Reflection;

namespace DbAccess.Models;

public sealed class DefinitionBuilder<T>
{
    private DbDefinition dbDefinition { get; set; } = new(typeof(T));

    public DbDefinition Build()
    {
        return dbDefinition;
    }

    #region Basic
    public DefinitionBuilder<T> SetTranslation(bool value = true)
    {
        dbDefinition.HasTranslation = value;
        return this;
    }
    public DefinitionBuilder<T> SetHistory(bool value = true)
    {
        dbDefinition.HasHistory = value;
        return this;
    }
    public DefinitionBuilder<T> RegisterProperty(Expression<Func<T, object>> column, bool nullable = false, string? defaultValue = null, int? length = null)
    {
        var columnDef = new ColumnDefinition()
        {
            Name = ExtractPropertyInfo(column).Name,
            Property = ExtractPropertyInfo(column),
            DefaultValue = defaultValue,
            IsNullable = nullable,
            Length = length
        };
        dbDefinition.Columns.Add(columnDef);

        return this;
    }
    public DefinitionBuilder<T> RegisterPrimaryKey(IEnumerable<Expression<Func<T, object?>>> properties)
    {
        var propertyNames = new List<string>();
        var propertyInfos = typeof(T).GetProperties().ToList();
        foreach (var property in properties)
        {
            var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
            propertyNames.Add(propertyName);

            if (!propertyInfos.Exists(t => t.Name == propertyName))
            {
                throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
            }
        }

        var name = $"PK_{typeof(T).Name}";

        dbDefinition.UniqueConstraints.Add(new ConstraintDefinition()
        {
            Name = name,
            Type = typeof(T),
            Columns = propertyNames,
            IsUnique = true,
        });

        return this;
    }
    public DefinitionBuilder<T> RegisterUniqueConstraint(IEnumerable<Expression<Func<T, object?>>> properties)
    {
        var propertyNames = new List<string>();
        var propertyInfos = typeof(T).GetProperties().ToList();
        foreach (var property in properties)
        {
            var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
            propertyNames.Add(propertyName);

            if (!propertyInfos.Exists(t => t.Name == propertyName))
            {
                throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
            }
        }

        var name = $"UC_{typeof(T).Name}_{string.Join("-", propertyNames)}";

        dbDefinition.UniqueConstraints.Add(new ConstraintDefinition()
        {
            Name = name,
            Type = typeof(T),
            Columns = propertyNames,
            IsUnique = true,
        });

        return this;
    }
    #endregion

    #region Extended
    public DefinitionBuilder<T> RegisterExtendedProperty<TExtended, TJoin>(
      Expression<Func<T, object>> TProperty,
      Expression<Func<TJoin, object>> TJoinProperty,
      Expression<Func<TExtended, object>> TExtendedProperty,
      bool optional = false,
      bool isList = false,
      bool cascadeDelete = false

      )
    {
        string baseProperty = ExtractPropertyInfo(TProperty).Name;
        string refProperty = ExtractPropertyInfo(TJoinProperty).Name;
        string extendedProperty = ExtractPropertyInfo(TExtendedProperty).Name;

        var join = new ForeignKeyDefinition()
        {
            Name = $"FK_{typeof(T).Name}_{extendedProperty}_{typeof(TJoin).Name}",
            Base = typeof(T),
            Ref = typeof(TJoin),
            BaseProperty = baseProperty,
            RefProperty =  refProperty,
            ExtendedProperty =  extendedProperty,
            IsOptional = optional,
            IsList = isList,
            UseCascadeDelete = cascadeDelete
        };
        dbDefinition.ForeignKeys.Add(join);

        return this;
    }

    public DefinitionBuilder<T> RegisterRelation<TExtended, TJoin>(Expression<Func<T, object>> TProperty, Expression<Func<TJoin, object>> TJoinProperty, Expression<Func<TExtended, object>> TExtendedProperty)
    {
        string baseProperty = ExtractPropertyInfo(TProperty).Name;
        string refProperty = ExtractPropertyInfo(TJoinProperty).Name;
        string extendedProperty = ExtractPropertyInfo(TExtendedProperty).Name;

        var relation = new RelationDefinition()
        {
            Base = typeof(T),
            Ref = typeof(TJoin),
            BaseProperty = baseProperty,
            RefProperty =  refProperty,
            ExtendedProperty =  extendedProperty
        };
        dbDefinition.Relations.Add(relation);

        return this;
    }
    #endregion

    #region Cross

    public DefinitionBuilder<T> RegisterAsCrossReference<TA, TB>(
        Expression<Func<T, object>> TASourceProperty,
        Expression<Func<TA, object>> TAJoinProperty,
        Expression<Func<T, object>> TBSourceProperty,
        Expression<Func<TB, object>> TBJoinProperty)
    {
        // Do stuff

        return this;
    }

    public DefinitionBuilder<T> RegisterAsCrossReferenceExtended<TExtended, TA, TB>
        (
            (Expression<Func<T, object>> Source, Expression<Func<TA, object>> Join, Expression<Func<TExtended, object>> Extended) defineA,
            (Expression<Func<T, object>> Source, Expression<Func<TB, object>> Join, Expression<Func<TExtended, object>> Extended) defineB
        )
    {
        RegisterExtendedProperty(defineA.Source, defineA.Join, defineA.Extended);
        RegisterExtendedProperty(defineB.Source, defineB.Join, defineB.Extended);
        RegisterAsCrossReference(defineA.Source, defineA.Join, defineB.Source, defineB.Join);

        return this;
    }

    #endregion

    #region Helpers
    private PropertyInfo ExtractPropertyInfo<TLocal>(Expression<Func<TLocal, object>> expression)
    {
        MemberExpression memberExpression;

        if (expression.Body is MemberExpression)
        {
            memberExpression = (MemberExpression)expression.Body;
        }
        else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression)
        {
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }
        else
        {
            throw new ArgumentException("Expression must refer to a property.");
        }

        return memberExpression.Member as PropertyInfo ?? throw new ArgumentException("Member is not a property.");
    }
    #endregion
}
