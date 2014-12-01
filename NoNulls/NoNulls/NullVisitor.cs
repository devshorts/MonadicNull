using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            if (node.Method.IsStatic)
            {
                if (node.Arguments.Count == 1)
                {
                    _expressions.Push(node);

                    _expressions.Push(node.Arguments[0]);
                }
            }
            else
            {
                _expressions.Push(node);
            }

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

        private int _count;
        private string NextVarName
        {
            get
            {
                _count++;
                return "var" + _count;
            }
        }

        private ConstructorInfo _constructor;
        private ConstructorInfo MethodValueConstructor
        {
            get
            {
                if (_constructor == null)
                {
                    _constructor = typeof(MethodValue<T>).GetConstructor(new[] { typeof(T), typeof(string), typeof(bool) });
                }

                return _constructor;
            }
        }

        private Expression BuildIfs(Expression current, Expression prev = null)
        {
            var stringRepresentation = Expression.Constant(current.ToString(), typeof(string));

            var variable = Expression.Parameter(current.Type, NextVarName);

            Expression evaluatedExpression = EvaluateExpression(current, prev);

            var assignment = Expression.Assign(variable, evaluatedExpression);

            var end = _expressions.Count == 0;

            var nextExpression =
                 !end
                    ? BuildIfs(_expressions.Pop(), variable)
                    : LastExpression(variable, stringRepresentation);

            Expression blockBody;

            if (!end)
            {
                var whenNull = OnNull(stringRepresentation);

                blockBody = CheckForNull(variable, whenNull, nextExpression);
            }
            else
            {
                blockBody = nextExpression;
            }

            return Expression.Block(new [] { variable }, new[] { assignment, blockBody });           
        }

        private Expression OnNull(ConstantExpression stringRepresentation)
        {
            var falseVal = Expression.Constant(false);

            var nullValue = Expression.Constant(default(T), _finalExpression.Type);

            return Expression.New(MethodValueConstructor, new Expression[] { nullValue, stringRepresentation, falseVal });
        }

        private Expression CheckForNull(ParameterExpression variable, Expression whenNull, Expression nextExpression)
        {
            if (variable.Type.IsValueType)
            {
                if (variable.Type.IsGenericType && variable.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var hasValueExpression = Expression.Property(variable, "HasValue");
                    
                    var hasValueIsFalseExpression = Expression.IsFalse(hasValueExpression);

                    return Expression.Condition(hasValueIsFalseExpression, whenNull, nextExpression);
                }
                
                return nextExpression;
            }

            var ifNull = Expression.ReferenceEqual(variable, Expression.Constant(null));

            return Expression.Condition(ifNull, whenNull, nextExpression);
        }

        private Expression LastExpression(ParameterExpression variable, ConstantExpression stringRepresentation)
        {
            var trueVal = Expression.Constant(true);
         
            return Expression.New(MethodValueConstructor, new Expression[] { variable, stringRepresentation, trueVal });
        }

        private Expression EvaluateExpression(Expression current, Expression prev)
        {
            Expression evaluatedExpression = current;

            if (prev == null)
            {
                return current;
            }

            if (current is MethodCallExpression)
            {
                var method = current as MethodCallExpression;

                if (method.Method.IsStatic)
                {
                    evaluatedExpression = Expression.Call(null, method.Method, method.Arguments);
                }
                else
                {
                    evaluatedExpression = Expression.Call(prev, method.Method, method.Arguments);
                }
            }
            else if (current is MemberExpression)
            {
                var member = current as MemberExpression;

                evaluatedExpression = Expression.MakeMemberAccess(prev, member.Member);
            }

            return evaluatedExpression;
        }
    }
}