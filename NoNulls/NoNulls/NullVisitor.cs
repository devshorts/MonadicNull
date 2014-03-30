using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Devshorts.MonadicNull
{
    internal class NullVisitor<T> : ExpressionVisitor
    {
        private readonly Stack<Expression> _expressions = new Stack<Expression>();

        private Expression _finalExpression;

        private Expression _built;

        private Boolean IsMethod { get; set; }
        
        private void CaptureFinal(Expression node, bool isMethod)
        {
            if (_finalExpression == null)
            {
                _finalExpression = node;

                IsMethod = isMethod;
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            base.Visit(node.Body);

            if (node.Parameters.Count > 0)
            {
                _expressions.Push(node.Parameters.First());

                BuildFinal(node);

                return Expression.Lambda(_built, node.Parameters);
            }
            
            BuildFinal(node);

            return Expression.Lambda(_built);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            CaptureFinal(node, true);

            _expressions.Push(node);

            var next = Visit(node.Object);

            return next;
        }

        protected override Expression VisitMember(MemberExpression node)
        {            
            _expressions.Push(node);

            CaptureFinal(node, false);

            var exp = Visit(node.Expression);

            return exp;
        }

        private void BuildFinal(Expression exp)
        {
            if (_expressions.Count == 0)
            {
                return;
            }

            var condition = BuildIfs(_expressions.Pop());

            _built = Expression.Block(new[] { condition });
        }

        private Expression BuildIfs(Expression top)
        {
            var stringRepresentation = Expression.Constant(top.ToString(), typeof(string));

            var trueVal = Expression.Constant(true);

            var falseVal = Expression.Constant(false);

            var nullValue = Expression.Constant(default(T), _finalExpression.Type);

            var constructorInfo = typeof(MethodValue<T>).GetConstructor(new[] { typeof(T), typeof(string), typeof(bool) });

            var returnNull = Expression.New(constructorInfo, new [] { nullValue, stringRepresentation, falseVal });

            var ifNull = Expression.ReferenceEqual(top, Expression.Constant(null));
            
            var finalReturn = Expression.New(constructorInfo, new []{ _finalExpression, stringRepresentation, trueVal });
           
            if (_expressions.Count == 1)
            {
                _expressions.Clear();
            }
            
            var condition = Expression.Condition(ifNull, returnNull, 
                    _expressions.Count == 0 ? 
                        finalReturn 
                        : BuildIfs(_expressions.Pop()));

            
            return condition;            
        }
    }
}