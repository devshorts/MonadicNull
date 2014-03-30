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
        public static Func<Y, MethodValue<T>> CompileChain<Y, T>(Expression<Func<Y, T>> input)
        {
            var transform = (Expression<Func<Y, MethodValue<T>>>)new NullVisitor<T>().Visit(input);

            return transform.Compile();
        }

        public static MethodValue<T> Safe<T>(Expression<Func<T>> input)
        {
            var transform = (Expression<Func<MethodValue<T>>>)new NullVisitor<T>().Visit(input);

            return transform.Compile()();
        }
//
//        public static Func<T, MethodValue<T>> SafeCompile<T>(Expression<Func<T>> input)
//        {
//            var transform = (Expression<Func<MethodValue<T>>>)new NullVisitor<T>().Visit(input);
//
//            return transform.Compile();
//        }
    }
}
