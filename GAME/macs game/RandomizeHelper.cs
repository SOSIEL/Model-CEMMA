using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Helpers
{
    using Randoms;

    public static class RandomizeHelper
    {
        public static T RandomizeOne<T>(this IEnumerable<T> source)
        {
            return RandomizeOne(source.ToArray());
        }

        public static T RandomizeOne<T>(this IList<T> source)
        {
            int position = LinearUniformRandom.GetInstance.Next(source.Count);

            return source[position];
        }

        public static T RandomizeOne<T>(this List<T> source)
        {
            int position = LinearUniformRandom.GetInstance.Next(source.Count);

            return source[position];
        }

        private static IEnumerable<T> RandomizeEnumeration<T>(this IEnumerable<T> original)
        {
            List<T> temp = new List<T>(original);

            while (temp.Count > 0)
            {
                T item = temp[LinearUniformRandom.GetInstance.Next(temp.Count)];

                temp.Remove(item);

                yield return item;
            }
        }


        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> original, bool randomize = true)
        {
            if (randomize)
            {
                return RandomizeEnumeration(original);
            }
            else
            {
                return original;
            }
        }
    }
}
