using System;
using System.Linq.Expressions;

namespace PortaleRegione.Persistance
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TItem, bool>> CombineExpressions<TItem>(Expression<Func<TItem, bool>> expr1, Expression<Func<TItem, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(TItem));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<TItem, bool>>(Expression.OrElse(left, right), parameter);
        }

        public class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}