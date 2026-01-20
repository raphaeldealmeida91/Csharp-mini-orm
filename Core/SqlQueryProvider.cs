// File: Linq/SqlQueryProvider.cs
using System.Linq.Expressions;
using MiniOrm.Linq;

namespace MiniOrm.Core
{
    public class SqlQueryProvider(MiniDbContext context) : IQueryProvider
    {
        private readonly MiniDbContext _context = context;

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().First();
            var queryType = typeof(DbSet<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryType, this, expression)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new DbSet<TElement>(this, expression);

        public object Execute(Expression expression)
            => throw new NotImplementedException();

        public TResult Execute<TResult>(Expression expression)
        {
            var translator = new QueryTranslator(_context);
            translator.Translate(expression);

            if (translator.Metadata == null)
                throw new InvalidOperationException("Impossible de déterminer les métadonnées de la table.");

            var list = SqlQueryExecutor.ExecuteQuery(
                _context,
                translator.ElementType!,
                translator.Metadata,
                translator.SelectedProperties,
                translator.WhereClause,
                translator.Limit
            );

            return (TResult)list;
        }
    }
}