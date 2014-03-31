using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Devshorts.MonadicNull
{
    internal class NullVisitor<T> : ExpressionVisitor
    {
        private readonly Stack<Expression> _expressions = new Stack<Expression>();

        private Expression _finalExpression;

        private void CaptureFinalExpression(Expression node)
        {
            if (_finalExpression == null)
            {
                _finalExpression = node;
            }
        }

        protected override Expression VisitLambda<Y>(Expression<Y> node)
        {
            base.Visit(node.Body);

            CaptureFinalExpression(node.Body);

            if (node.Parameters.Count > 0)
            {
                _expressions.Push(node.Parameters.First());

                return Expression.Lambda(BuildFinalStatement(), node.Parameters);
            }
            
            return Expression.Lambda(BuildFinalStatement());
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {            
            _expressions.Push(node);

            var next = Visit(node.Object);

            return next;
        }

        protected override Expression VisitMember(MemberExpression node)
        {            
            _expressions.Push(node);

            return Visit(node.Expression);
        }

        private Expression BuildFinalStatement()
        {
            return Expression.Block(new[] { BuildIfs(_expressions.Pop()) });
        }

        private Expression BuildIfs(Expression top)
        {
            var stringRepresentation = Expression.Constant(top.ToString(), typeof(string));

            var trueVal = Expression.Constant(true);

            var falseVal = Expression.Constant(false);

            var nullValue = Expression.Constant(default(T), _finalExpression.Type);

            var methodValueConstructor = typeof(MethodValue<T>).GetConstructor(new[] { typeof(T), typeof(string), typeof(bool) });

            var returnNull = Expression.New(methodValueConstructor, new [] { nullValue, stringRepresentation, falseVal });

            var ifNull = Expression.ReferenceEqual(top, Expression.Constant(null));
            
            var finalReturn = Expression.New(methodValueConstructor, new []{ _finalExpression, stringRepresentation, trueVal });
            
            // ignore the last element since we only care about the path to get to it, not the actual element
            var nextExpression =
                _expressions.Count <= 1
                    ? finalReturn
                    : BuildIfs(_expressions.Pop());

            var condition = Expression.Condition(ifNull, returnNull, nextExpression);

            
            return condition;            
        }
    }
}