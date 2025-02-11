using System.Linq.Expressions;
using System.Reflection;

namespace DbAccess.Models;

public class GenericFilterBuilder<T> : IEnumerable<GenericFilter>
{
    private readonly List<GenericFilter> _filters = new();

    public GenericFilterBuilder<T> Add<TProperty>(Expression<Func<T, TProperty>> property, TProperty value, FilterComparer comparer = FilterComparer.Equals)
    {
        var propertyInfo = ExtractPropertyInfo(property);
        _filters.Add(new GenericFilter(propertyInfo.Name, value, comparer));
        return this;
    }

    public GenericFilterBuilder<T> Equal<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
    {
        return Add(property, value, FilterComparer.Equals);
    }

    private PropertyInfo ExtractPropertyInfo<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        MemberExpression? memberExpression = expression.Body switch
        {
            MemberExpression member => member,
            UnaryExpression { Operand: MemberExpression member } => member,
            _ => null
        };

        return memberExpression?.Member as PropertyInfo ?? throw new ArgumentException($"Expression '{expression}' does not refer to a valid property.");
    }

    public IEnumerator<GenericFilter> GetEnumerator()
    {
        return _filters.GetEnumerator();
    }

    // Kreves for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
/// Generic Filter
/// </summary>
public class GenericFilter
{
    /// <summary>
    /// PropertyName
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// FilterComparer
    /// </summary>
    public FilterComparer Comparer { get; set; }

    /// <summary>
    /// GenericFilter
    /// </summary>
    /// <param name="propertyName">propertyName</param>
    /// <param name="value">value</param>
    /// <param name="comparer">comparer</param>
    public GenericFilter(string propertyName, object value, FilterComparer comparer = FilterComparer.Equals)
    {
        PropertyName = propertyName;
        Value = value;
        Comparer = comparer;
    }
}

/// <summary>
/// Specifies the type of comparison to use in a filter.
/// </summary>
public enum FilterComparer
{
    /// <summary>
    /// Check if the property is equal to the value.
    /// </summary>
    Equals,

    /// <summary>
    /// Check if the property starts with the value.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Check if the property ends with the value.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Check if the property contains the value.
    /// </summary>
    Contains,

    /// <summary>
    /// Check if the property is greater than the value.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Check if the property is greater than or equal to the value.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Check if the property is less than the value.
    /// </summary>
    LessThan,

    /// <summary>
    /// Check if the property is less than or equal to the value.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Check if the property is not equal to the value.
    /// </summary>
    NotEqual
}
