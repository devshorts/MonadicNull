using System;
using System.Linq.Expressions;

namespace Devshorts.MonadicNull
{
    public static class Option
    {
        /// <summary>
        /// Safely executes the supplied function and returns a MethodValue type that gives you
        /// the result, whether the item has a result, and if it doesn't have a result where
        /// the chain failed.
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
