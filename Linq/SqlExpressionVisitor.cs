// File: Linq/SqlExpressionVisitor.cs
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MiniOrm.Mapping;

namespace MiniOrm.Linq
{
    public class SqlExpressionVisitor(EntityMetadata metadata) : ExpressionVisitor
    {
        private readonly EntityMetadata _metadata = metadata;
        private readonly StringBuilder _sql = new();
        private readonly SqlWhereClause _where = new();
        private int _paramIndex = 0;

        public SqlWhereClause Translate(Expression expression)
        {
            Visit(expression);
            _where.Sql = _sql.ToString();
            return _where;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _sql.Append('(');
            Visit(node.Left);

            _sql.Append(node.NodeType switch
            {
                ExpressionType.Equal => " = ",
                ExpressionType.NotEqual => " <> ",
                ExpressionType.GreaterThan => " > ",
                ExpressionType.GreaterThanOrEqual => " >= ",
                ExpressionType.LessThan => " < ",
                ExpressionType.LessThanOrEqual => " <= ",
                ExpressionType.AndAlso => " AND ",
                ExpressionType.OrElse => " OR ",
                _ => throw new NotSupportedException($"L'opération n'est pas supportée {node.NodeType}.")
            });

            Visit(node.Right);
            _sql.Append(')');
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression?.NodeType == ExpressionType.Parameter)
            {
                if (node.Member is not PropertyInfo property)
                    throw new NotSupportedException();

                if (!_metadata.Columns.TryGetValue(property, out var column))
                    throw new NotSupportedException($"La propriété {property.Name} n'est pas mappée à une colonne.");

                _sql.Append(column);
                return node;
            }

            var value = GetValue(node);
            AppendParameter(value);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AppendParameter(node.Value);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                object? argValue = null;

                var argExpr = node.Arguments[0];
                if (argExpr is ConstantExpression constExpr)
                    argValue = constExpr.Value;
                else if (argExpr is MemberExpression memberExpr)
                    argValue = GetValue(memberExpr);
                else
                    argValue = Expression.Lambda<Func<object?>>(Expression.Convert(argExpr, typeof(object))).Compile()();

                if (node.Method.Name == "StartsWith")
                {
                    Visit(node.Object);
                    _sql.Append(" LIKE ");
                    AppendParameter($"{argValue}%");
                    return node;
                }
                if (node.Method.Name == "Contains")
                {
                    Visit(node.Object);
                    _sql.Append(" LIKE ");
                    AppendParameter($"%{argValue}%");
                    return node;
                }
            }

            throw new NotSupportedException($"La méthode {node.Method.Name} n'est pas supportée.");
        }

        private void AppendParameter(object? value)
        {
            var paramName = $"@p{_paramIndex++}";
            _sql.Append(paramName);
            _where.Parameters[paramName] = value;
        }

        private static object? GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object?>>(objectMember);
            return getterLambda.Compile().Invoke();
        }
    }
}