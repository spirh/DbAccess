using System.Linq.Expressions;

namespace DbAccess.Models;

public abstract class DbCrossDefinition<T, TExtended, TA, TB> : DbExtendedDefinition<T, TExtended>
{
    public Type CrossAType => typeof(TA);

    public Type CrossBType => typeof(TB);

    public void RegisterAsCrossRefrence(Expression<Func<T, object?>> TASourceProperty, Expression<Func<TA, object?>> TAJoinProperty, Expression<Func<T, object?>> TBSourceProperty, Expression<Func<TB, object?>> TBJoinProperty)
    {
       
    }
}
