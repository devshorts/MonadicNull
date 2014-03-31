using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devshorts.MonadicNull;

namespace NoNulls.Tests.Extensions
{
    public class Split<T>
    {
        public IList<T> Success { get; private set; }
        public IList<T> Failure { get; private set; }

        public Split(IList<T> success, IList<T> failure)
        {
            Success = success;
            Failure = failure;
        }
    }
    public static class Extensions
    {
        public static Split<T> Protect<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var split = new Split<T>(new List<T>(), new List<T>());

            foreach (var item in source)
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
