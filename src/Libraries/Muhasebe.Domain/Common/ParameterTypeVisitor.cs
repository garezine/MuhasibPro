using System.Linq.Expressions;

namespace Muhasebe.Domain.Common
{
    public class ParameterTypeVisitor<TFrom, TTo> : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _parameterMap;

        public ParameterTypeVisitor(Dictionary<ParameterExpression, ParameterExpression> parameterMap)
        { _parameterMap = parameterMap ?? new Dictionary<ParameterExpression, ParameterExpression>(); }

        protected override Expression VisitParameter(ParameterExpression node)
        { return _parameterMap.TryGetValue(node, out var replacement) ? replacement : base.VisitParameter(node); }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);
            if (expression.Type != node.Member.DeclaringType)
            {
                var newMember = expression.Type.GetMember(node.Member.Name).FirstOrDefault();
                if (newMember == null)
                    throw new InvalidOperationException(
                        $"Member {node.Member.Name} not found on type {expression.Type.Name}");

                return Expression.MakeMemberAccess(expression, newMember);
            }
            return base.VisitMember(node);
        }

        // 1. OrderBy için dönüşüm metodu
        public static Func<IQueryable<TTo>, IOrderedQueryable<TTo>> ConvertOrderBy(
            Expression<Func<IQueryable<TFrom>, IOrderedQueryable<TFrom>>> orderBy)
        {
            var fromParameter = orderBy.Parameters[0];
            var toParameter = Expression.Parameter(typeof(IQueryable<TTo>), fromParameter.Name);

            var visitor = new ParameterTypeVisitor<TFrom, TTo>(
                new Dictionary<ParameterExpression, ParameterExpression> { { fromParameter, toParameter } });

            var newBody = visitor.Visit(orderBy.Body);
            var convertedExpression = Expression.Lambda<Func<IQueryable<TTo>, IOrderedQueryable<TTo>>>(
                newBody,
                toParameter);

            return convertedExpression.Compile();
        }

        // 2. Predicate dönüşümü (Mevcut)
        public static Expression<Func<TTo, bool>> Convert(Expression<Func<TFrom, bool>> expression)
        {
            var fromParameter = expression.Parameters[0];
            var toParameter = Expression.Parameter(typeof(TTo), fromParameter.Name);
            var visitor = new ParameterTypeVisitor<TFrom, TTo>(
                new Dictionary<ParameterExpression, ParameterExpression> { { fromParameter, toParameter } });

            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<TTo, bool>>(newBody, toParameter);
        }

        // 3. Include dönüşümü (Mevcut)
        public static Expression<Func<TTo, object>> ConvertInclude(Expression<Func<TFrom, object>> include)
        {
            var fromParameter = include.Parameters[0];
            var toParameter = Expression.Parameter(typeof(TTo), fromParameter.Name);
            var visitor = new ParameterTypeVisitor<TFrom, TTo>(
                new Dictionary<ParameterExpression, ParameterExpression> { { fromParameter, toParameter } });

            var newBody = visitor.Visit(include.Body);
            return Expression.Lambda<Func<TTo, object>>(newBody, toParameter);
        }

        public static LambdaExpression ConvertOrderBy(Expression<Func<TFrom, object>> orderBy)
        {
            var fromParameter = orderBy.Parameters[0];
            var toParameter = Expression.Parameter(typeof(TTo), fromParameter.Name);
            var visitor = new ParameterTypeVisitor<TFrom, TTo>(
                new Dictionary<ParameterExpression, ParameterExpression> { { fromParameter, toParameter } });

            var newBody = visitor.Visit(orderBy.Body);

            // Boxing'i kaldır (örneğin: (object)f.Ad → f.Ad)
            if (newBody is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                newBody = unary.Operand;
            }

            var memberType = newBody.Type;
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TTo), memberType);
            return Expression.Lambda(delegateType, newBody, toParameter);
        }
    }
}