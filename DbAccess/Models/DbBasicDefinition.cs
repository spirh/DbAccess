using System.Linq.Expressions;
using System.Reflection;

namespace DbAccess.Models;
public abstract class DbBasicDefinition<T> : DbDefinition
{
    public Type EntityType => typeof(T);

    public void SetTranslation(bool value = true)
    {
        HasTranslation = value;
    }
    public void SetHistory(bool value = true)
    {
        HasHistory = value;
    }
    public void RegisterProperty(Expression<Func<T, object>> column, bool nullable = false, string? defaultValue = null, int? length = null)
    {
        var columnDef = new ColumnDefinition()
        {
            Name = ExtractPropertyInfo(column as Expression<Func<T, object>>).Name,
            Type = ExtractPropertyInfo(column as Expression<Func<T, object>>).PropertyType,
            DefaultValue = defaultValue,
            IsNullabe = nullable,
            Length = length
        };
        Columns.Add(columnDef);
    }
    public void RegisterPrimaryKey(IEnumerable<Expression<Func<T, object?>>> properties)
    {
        var propertyNames = new List<string>();
        foreach (var property in properties)
        {
            var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
            propertyNames.Add(propertyName);

            if (!typeof(T).GetProperties().ToList().Exists(t => t.Name == propertyName))
            {
                throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
            }
        }

        var name = $"PK_{typeof(T).Name}";

        UniqueConstraints.Add(new ConstraintDefinition()
        {
            Name = name,
            Type = typeof(T),
            Columns = propertyNames,
            IsUnique = true,
        });
    }
    public void RegisterUniqueConstraint(IEnumerable<Expression<Func<T, object?>>> properties)
    {
        var propertyNames = new List<string>();
        foreach (var property in properties)
        {
            var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
            propertyNames.Add(propertyName);

            if (!typeof(T).GetProperties().ToList().Exists(t => t.Name == propertyName))
            {
                throw new Exception($"{typeof(T).Name} does not contain the property '{propertyName}'");
            }
        }

        var name = $"UC_{typeof(T).Name}_{string.Join("-", propertyNames)}";

        UniqueConstraints.Add(new ConstraintDefinition()
        {
            Name = name,
            Type = typeof(T),
            Columns = propertyNames,
            IsUnique = true,
        });
    }
    protected PropertyInfo ExtractPropertyInfo<TLocal>(Expression<Func<TLocal, object>> expression)
    {
        MemberExpression memberExpression;

        if (expression.Body is MemberExpression)
        {
            // Hvis Body er direkte en MemberExpression, bruk den
            memberExpression = (MemberExpression)expression.Body;
        }
        else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression)
        {
            // Hvis Body er en UnaryExpression (f.eks. ved en typekonvertering), bruk Operand
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }
        else
        {
            throw new ArgumentException("Expression must refer to a property.");
        }

        return memberExpression.Member as PropertyInfo
            ?? throw new ArgumentException("Member is not a property.");
    }
}
