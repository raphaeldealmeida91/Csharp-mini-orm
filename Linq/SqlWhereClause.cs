// File: Linq/SqlWhereClause.cs
namespace MiniOrm.Linq
{
    public class SqlWhereClause
    {
        public string Sql { get; set; } = string.Empty;
        public Dictionary<string, object?> Parameters { get; } = [];
    }
}