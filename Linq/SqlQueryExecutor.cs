// File: Linq/SqlQueryExecutor.cs 
using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;
using MiniOrm.Core;
using MiniOrm.Mapping;

namespace MiniOrm.Linq
{
    public static class SqlQueryExecutor
    {
        public static IList ExecuteQuery(
            MiniDbContext context,
            Type elementType,
            EntityMetadata? metadata,
            List<PropertyInfo> selectedProps,
            SqlWhereClause? where,
            int? limit)
        {
            var sql = new StringBuilder();

            if (metadata != null)
                sql.Append("SELECT ").Append(string.Join(", ", selectedProps.Select(p => metadata.Columns[p])));
            else
                sql.Append("SELECT ").Append(string.Join(", ", selectedProps.Select(p => p.Name)));

            sql.AppendLine();
            if (metadata != null)
                sql.Append("FROM ").Append(metadata.TableName);
            else
                sql.Append("FROM (SELECT 1)");

            if (where != null)
            {
                sql.AppendLine();
                sql.Append("WHERE ").Append(where.Sql);
            }

            if (limit.HasValue)
                sql.AppendLine().Append($"LIMIT {limit.Value}");

            Console.WriteLine("SQL généré:");
            Console.WriteLine(sql.ToString());

            using var connection = new SqliteConnection(context.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql.ToString();

            if (where != null)
                foreach (var param in where.Parameters)
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);

            IList list;
            bool isAnonymousType = elementType.Name.Contains("AnonymousType");
            
            if (metadata != null && !isAnonymousType)
            {
                list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
            }
            else
            {
                list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (metadata != null && !isAnonymousType)
                {
                    var obj = Activator.CreateInstance(elementType)!;
                    foreach (var prop in selectedProps)
                    {
                        var colName = metadata.Columns[prop];
                        var value = reader[colName] is DBNull ? null : reader[colName];
                        if (value != null && prop.PropertyType != value.GetType())
                            value = Convert.ChangeType(value, prop.PropertyType);
                        prop.SetValue(obj, value);
                    }
                    list.Add(obj);
                }
                else
                {
                    var values = new object?[selectedProps.Count];
                    for (int i = 0; i < selectedProps.Count; i++)
                    {
                        var colName = metadata?.Columns[selectedProps[i]] ?? selectedProps[i].Name;
                        var value = reader[colName] is DBNull ? null : reader[colName];
                        
                        if (value != null && selectedProps[i].PropertyType != value.GetType())
                        {
                            value = Convert.ChangeType(value, selectedProps[i].PropertyType);
                        }
                        
                        values[i] = value;
                    }
                    
                    var constructor = elementType.GetConstructors()
                        .FirstOrDefault(c => c.GetParameters().Length == selectedProps.Count) ?? throw new InvalidOperationException($"No constructor found for type {elementType.Name} with {selectedProps.Count} parameters");
                    var obj = constructor.Invoke(values);
                    list.Add(obj);
                }
            }

            return list;
        }
    }
}