// File: Core/MiniDbContext.cs
using MiniOrm.Mapping;
using System.Collections.Concurrent;

namespace MiniOrm.Core
{
    public class MiniDbContext
    {
        private readonly SqlQueryProvider _provider;
        private readonly ConcurrentDictionary<Type, object> _sets = new();

        public string ConnectionString { get; }

        public MiniDbContext(string connectionString = "Data Source=miniorm.db")
        {
            ConnectionString = connectionString;
            _provider = new SqlQueryProvider(this);
        }

        internal EntityMetadata GetMetadata(Type type)
            => EntityMapper.GetMetadata(type);

        public DbSet<T> Set<T>()
        {
            return (DbSet<T>)_sets.GetOrAdd(
                typeof(T),
                _ => new DbSet<T>(_provider)
            );
        }
    }
}