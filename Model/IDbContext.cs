using System.Data.OleDb;

namespace Citation.Model;

/// <summary>
/// Defines a contract for database context operations that support reading from and writing to a data source using
/// OleDb connections.
/// </summary>
/// <remarks>Implementations of this interface provide methods for persisting and retrieving data using OleDb. The
/// interface is intended for internal use and may not be thread safe.</remarks>
/// <typeparam name="TSource">The type of object that is materialized from a database record. Typically represents an entity or data transfer
/// object.</typeparam>
internal interface IDbContext<out TSource>
{
    public void ToSql(OleDbConnection connection);
    public static abstract TSource? FromSql(OleDbDataReader reader);
    public void DeleteSql(OleDbConnection connection);
}
