// File: Mapping/EntityMapper.cs
using System.Collections.Concurrent;
using System.Reflection;

namespace MiniOrm.Mapping
{
    public static class EntityMapper
    {
        private static readonly ConcurrentDictionary<Type, EntityMetadata> _cache
            = new();

        public static EntityMetadata GetMetadata<T>()
        {
            return GetMetadata(typeof(T));
        }

        public static EntityMetadata GetMetadata(Type type)
        {
            return _cache.GetOrAdd(type, BuildMetadata);
        }

        private static EntityMetadata BuildMetadata(Type type)
        {
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (tableAttr == null)
                throw new InvalidOperationException(
                    $"Le type {type.Name} est manquant dans la table d'attribut.");

            var columns = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    ColumnAttr = p.GetCustomAttribute<ColumnAttribute>()
                })
                .Where(x => x.ColumnAttr != null)
                .ToDictionary(
                    x => x.Property,
                    x => x.ColumnAttr!.Name
                );

            if (columns.Count == 0)
                throw new InvalidOperationException(
                    $"Le type {type.Name} n'a pas de colonnes mappées.");

            return new EntityMetadata(
                type,
                tableAttr.Name,
                columns
            );
        }
    }
}