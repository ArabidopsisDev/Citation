using System.Data.OleDb;

namespace Citation.Model
{
    internal interface IDbContext<out T>
    {
        public string ToSql();
        public abstract static T FromSql(OleDbDataReader reader);
    }
}
