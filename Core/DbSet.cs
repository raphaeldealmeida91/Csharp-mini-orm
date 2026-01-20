// File: Core/DbSet.cs
using System.Collections;
using System.Linq.Expressions;

namespace MiniOrm.Core
{
    public class DbSet<T> : IQueryable<T>
    {
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        public Type ElementType => typeof(T);

        public DbSet(IQueryProvider provider)
        {
            Provider = provider;
            Expression = Expression.Constant(this);
        }

        internal DbSet(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public List<T> ToList()
        {
            return Provider.Execute<List<T>>(Expression);
        }
    }
}