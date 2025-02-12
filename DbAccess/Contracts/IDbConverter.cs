using System.Data;

namespace DbAccess.Contracts
{
    public interface IDbConverter
    {
        List<T> ConvertToObjects<T>(IDataReader reader) where T : new();
    }
}