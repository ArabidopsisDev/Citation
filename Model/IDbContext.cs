using System.Data.OleDb;

namespace Citation.Model
{
    internal interface IDbContext<out TSource>
    {
        public void ToSql(OleDbConnection connection);
        public abstract static TSource? FromSql(OleDbDataReader reader);
        public void DeleteSql(OleDbConnection connection);
    }
}
