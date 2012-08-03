using System;
using System.Collections.Generic;

namespace Sitecore.ItemBucket.Kernel.Util
{
    public static class RandomSampleExtensions
    {
        public static IEnumerable<T> RandomSample<T>(
           this IEnumerable<T> source, int count, bool allowDuplicates)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return RandomSampleIterator(source, count, -1, allowDuplicates);
        }

        public static IEnumerable<T> RandomSample<T>(this IEnumerable<T> source, int count, int seed, bool allowDuplicates)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return RandomSampleIterator(source, count, seed, allowDuplicates);
        }

        public static IEnumerable<T> RandomSampleIterator<T>(IEnumerable<T> source, int count, int seed, bool allowDuplicates)
        {
            List<T> buffer = new List<T>(source);
            Random random = seed < 0 ? new Random() : new Random(seed);
            count = Math.Min(count, buffer.Count);

            if (count > 0)
            {
                for (var i = 1; i <= count; i++)
                {
                    var randomIndex = random.Next(buffer.Count);
                    yield return buffer[randomIndex];
                    if (!allowDuplicates)
                    {
                        buffer.RemoveAt(randomIndex);
                    }
                }
            }
        }
    }
}
