using System;
using System.Linq.Expressions;

namespace Devshorts.MonadicNull
{
    public static class Option
    {
        /// <summary>
        /// Safely executes the supplied function and returns a Function that takes an input
        /// and returns a MethodType type that gives you
        /// the result, whether the item has a result, and if it doesn't have a result where
        /// the chain failed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Func<Y, MethodValue<T>> CompileChain<Y, T>(Expression<Func<Y, T>> input)
        {
            var transform = (Expression<Func<Y, MethodValue<T>>>)new NullVisitor<T>().Visit(input);

            return transform.Compile();
        }

        /// <summary>
        /// Transforms the input expression and builds if not null checks for each property
        /// and returns a MethodValue object that lets you know whether the item has a result,
        /// or if it doesn't have a result what in the chain failed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static MethodValue<T> Safe<T>(Expression<Func<T>> input)
        {
            var transform = (Expression<Func<MethodValue<T>>>)new NullVisitor<T>().Visit(input);

            return transform.Compile()();
        }
    }
}
