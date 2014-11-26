using System;
using System.Collections.Generic;

namespace NoNulls.Tests.Extensions
{
    public class Split<T>
    {
        public Split(IList<T> success, IList<T> failure)
        {
            Success = success;
            Failure = failure;
        }

        public IList<T> Success { get; private set; }
        public IList<T> Failure { get; private set; }
    }

    public static class Extensions
    {
        public static Split<T> Protect<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var split = new Split<T>(new List<T>(), new List<T>());

            foreach (T item in source)
            {
                if (predicate(item))
                {
                    split.Success.Add(item);
                }
                else
                {
                    split.Failure.Add(item);
                }
            }

            return split;
        }
    }
}