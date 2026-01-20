// File: Database/TableBuilder.cs
using System.Text;
using Microsoft.Data.Sqlite;
using MiniOrm.Mapping;

namespace MiniOrm.Database
{
    public class TableBuilder(string connectionString = "Data Source=miniorm.db")
    {
        private readonly string _connectionString = connectionString;

        public void CreateTableForEntity<T>()
        {
            var metadata = EntityMapper.GetMetadata<T>();
            var sql = GenerateCreateTableSql(metadata);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();

            Console.WriteLine($"Table '{metadata.TableName}' créée ou vérifiée.");
        }

        private static string GenerateCreateTableSql(EntityMetadata metadata)
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS {metadata.TableName} (");

            var columns = new List<string>();

            foreach (var kvp in metadata.Columns)
            {
                var prop = kvp.Key;
                var colName = kvp.Value;
                var sqlType = GetSqlType(prop.PropertyType);

                var columnDef = $"    {colName} {sqlType}";

                if (prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                {
                    columnDef += " PRIMARY KEY AUTOINCREMENT";
                }

                columns.Add(columnDef);
            }

            sql.AppendLine(string.Join(",\n", columns));
            sql.AppendLine(");");

            return sql.ToString();
        }

        private static string GetSqlType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.Name switch
            {
                nameof(Int32) => "INTEGER",
                nameof(Int64) => "INTEGER",
                nameof(String) => "TEXT",
                nameof(Boolean) => "INTEGER",
                nameof(DateTime) => "TEXT",
                nameof(Decimal) => "REAL",
                nameof(Double) => "REAL",
                nameof(Single) => "REAL",
                _ => "TEXT"
            };
        }

        public void DropTable(string tableName)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS {tableName};";
            command.ExecuteNonQuery();

            Console.WriteLine($"Table '{tableName}' supprimée.");
        }
    }
}