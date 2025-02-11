using System.Linq.Expressions;

namespace DbAccess.Models;

public abstract class DbExtendedDefinition<T, TExtended> : DbBasicDefinition<T>
{
    public Type ExtendedType => typeof(TExtended);

    public void RegisterExtendedProperty<TJoin>(
       Expression<Func<T, object?>> TProperty,
       Expression<Func<TJoin, object>> TJoinProperty,
       Expression<Func<TExtended, object?>> TExtendedProperty,
       bool optional = false,
       bool isList = false,
       bool cascadeDelete = false
       )
    {
        string baseProperty = ExtractPropertyInfo(TProperty as Expression<Func<T, object>>).Name;
        string refProperty = ExtractPropertyInfo(TJoinProperty as Expression<Func<TJoin, object>>).Name;
        string extendedProperty = ExtractPropertyInfo(TExtendedProperty as Expression<Func<TExtended, object>>).Name;

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
        ForeignKeys.Add(join);
    }

    public void RegisterRelation<TJoin>(Expression<Func<T, object?>> TProperty, Expression<Func<TJoin, object>> TJoinProperty, Expression<Func<TExtended, object?>> TExtendedProperty )
    {
        /* 
         * NAME ????
        RegisterList
        RegisterListProperty
        RegisterRelation
        RegisterReverseLookup
        */

        string baseProperty = ExtractPropertyInfo(TProperty as Expression<Func<T, object>>).Name;
        string refProperty = ExtractPropertyInfo(TJoinProperty as Expression<Func<TJoin, object>>).Name;
        string extendedProperty = ExtractPropertyInfo(TExtendedProperty as Expression<Func<TExtended, object>>).Name;

        var relation = new RelationDefinition()
        {
            Base = typeof(T),
            Ref = typeof(TJoin),
            BaseProperty = baseProperty,
            RefProperty =  refProperty,
            ExtendedProperty =  extendedProperty
        };
        Relations.Add(relation);
    }

}
