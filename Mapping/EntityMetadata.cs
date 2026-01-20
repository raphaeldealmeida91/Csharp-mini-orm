// File: Mapping/EntityMetadata.cs
using System.Reflection;

namespace MiniOrm.Mapping
{
    public class EntityMetadata(
        Type entityType,
        string tableName,
        Dictionary<PropertyInfo, string> columns)
    {
        public Type EntityType { get; } = entityType;
        public string TableName { get; } = tableName;
        public IReadOnlyDictionary<PropertyInfo, string> Columns { get; } = columns;
    }
}