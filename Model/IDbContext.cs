using System.Data.OleDb;

namespace Citation.Model
{
    internal interface IDbContext<out TSource>
    {
        public void ToSql(OleDbConnection connection);
        public static abstract TSource? FromSql(OleDbDataReader reader);
        public void DeleteSql(OleDbConnection connection);
    }
}
