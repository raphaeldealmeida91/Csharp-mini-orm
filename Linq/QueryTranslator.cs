// File: Linq/QueryTranslator.cs
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MiniOrm.Core;
using MiniOrm.Mapping;

namespace MiniOrm.Linq
{
    public class QueryTranslator(MiniDbContext context)
    {
        private readonly MiniDbContext _context = context;

        public Type? ElementType { get; private set; }
        public EntityMetadata? Metadata { get; private set; }
        public List<PropertyInfo> SelectedProperties { get; private set; } = [];
        public SqlWhereClause? WhereClause { get; private set; }
        public int? Limit { get; private set; }

        public void Translate(Expression expression)
        {
            Expression current = expression;
            
            while (current is MethodCallExpression call)
            {
                switch (call.Method.Name)
                {
                    case "Where":
                        HandleWhere(call);
                        break;
                    case "Select":
                        HandleSelect(call);
                        break;
                    case "Take":
                    case "First":
                        HandleLimit(call);
                        break;
                }

                current = call.Arguments[0];
            }

            if (Metadata == null && current is ConstantExpression constExpr)
            {
                var queryableType = constExpr.Type;
                if (queryableType.IsGenericType && 
                    queryableType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    ElementType = queryableType.GetGenericArguments()[0];
                    Metadata = _context.GetMetadata(ElementType);
                    SelectedProperties = [.. Metadata.Columns.Keys];
                }
            }
        }

        private void HandleWhere(MethodCallExpression call)
        {
            var lambda = ExtractLambda(call.Arguments[1]);
            if (Metadata == null)
            {
                ElementType = call.Arguments[0].Type.GetGenericArguments()[0];
                Metadata = _context.GetMetadata(ElementType);
                SelectedProperties = [.. Metadata.Columns.Keys];
            }

            var visitor = new SqlExpressionVisitor(Metadata);
            WhereClause = visitor.Translate(lambda);
        }

        private void HandleSelect(MethodCallExpression call)
        {
            var lambda = ExtractLambda(call.Arguments[1]);
            
            if (lambda.Body is NewExpression newExpr && newExpr.Members != null)
            {
                ElementType = lambda.Body.Type;
                SelectedProperties = [];
                
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    if (newExpr.Arguments[i] is MemberExpression memberExpr &&
                        memberExpr.Member is PropertyInfo prop)
                    {
                        SelectedProperties.Add(prop);
                        Metadata ??= _context.GetMetadata(memberExpr.Expression!.Type);
                    }
                }
            }
            else if (lambda.Body is MemberExpression singleMember && 
                     singleMember.Member is PropertyInfo singleProp)
            {
                ElementType = singleProp.PropertyType;
                SelectedProperties = [singleProp];
                
                if (Metadata == null)
                {
                    Metadata = _context.GetMetadata(singleMember.Expression!.Type);
                }
            }
        }

        private void HandleLimit(MethodCallExpression call)
        {
            if (call.Method.Name == "First")
            {
                Limit = 1;
            }
            else if (call.Arguments.Count == 2 && call.Arguments[1] is ConstantExpression constExpr)
            {
                Limit = (int)constExpr.Value!;
            }
        }

        private static LambdaExpression ExtractLambda(Expression expr)
        {
            if (expr is LambdaExpression lambda) return lambda;
            if (expr is UnaryExpression unary && unary.Operand is LambdaExpression inner) return inner;
            throw new NotSupportedException($"Impossible d'extraire lambda de {expr.NodeType}.");
        }
    }
}