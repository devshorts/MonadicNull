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

                var final = BuildFinalStatement();

                return Expression.Lambda(final, node.Parameters);
            }
            
            return Expression.Lambda(BuildFinalStatement());
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {            
            _expressions.Push(node);

            return Visit(node.Object);
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

        private int count = 0;

        private string NextVarName
        {
            get
            {
                count++;
                return "var" + count;
            }
        }

        private Expression BuildIfs(Expression top, Expression next = null)
        {
            var stringRepresentation = Expression.Constant(top.ToString(), typeof(string));

            var variable = Expression.Parameter(top.Type, NextVarName);

            Expression evaluatedExpression = EvaluateExpression(top, next);

            var assignment = Expression.Assign(variable, evaluatedExpression);

            var trueVal = Expression.Constant(true);

            var falseVal = Expression.Constant(false);

            var nullValue = Expression.Constant(default(T), _finalExpression.Type);

            var methodValueConstructor = typeof(MethodValue<T>).GetConstructor(new[] { typeof(T), typeof(string), typeof(bool) });

            var returnNull = Expression.New(methodValueConstructor, new [] { nullValue, stringRepresentation, falseVal });

            var ifNull = Expression.ReferenceEqual(variable, Expression.Constant(null));
            
            var finalReturn = Expression.New(methodValueConstructor, new []{ _finalExpression, stringRepresentation, trueVal });
            
            // ignore the last element since we only care about the path to get to it, not the actual element
            var nextExpression =
                _expressions.Count <= 1
                    ? finalReturn
                    : BuildIfs(_expressions.Pop(), variable);

            var condition = Expression.Condition(ifNull, returnNull, nextExpression);
            
            return Expression.Block(new [] { variable }, new Expression [] { assignment, condition });           
        }

        private Expression EvaluateExpression(Expression top, Expression next)
        {
            Expression evaluatedExpression = top;

            if (next == null)
            {
                return top;
            }

            if (top is MethodCallExpression)
            {
                var method = top as MethodCallExpression;

                evaluatedExpression = Expression.Call(next, method.Method, method.Arguments);
            }
            else if (top is MemberExpression)
            {
                var member = top as MemberExpression;

                evaluatedExpression = Expression.MakeMemberAccess(next, member.Member);
            }

            return evaluatedExpression;
        }
    }
}