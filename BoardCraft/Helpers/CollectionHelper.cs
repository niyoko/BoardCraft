namespace BoardCraft.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using MathNet.Numerics.Random;

    public static class CollectionHelper
    {
        class RandomEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;
            private readonly Random _random;

            public RandomEnumerable(IEnumerable<T> source, Random random)
            {
                _source = source;
                _random = random;
            }

            public IEnumerator<T> GetEnumerator()
            {
                var buffer = new List<T>(_source);

                if (buffer.Count < 2)
                {
                    return buffer.GetEnumerator();
                }

                for (var i = buffer.Count - 1; i > 0; i--)
                {
                    var j = _random.Next(i + 1);
                    if (i != j)
                    {
                        var temp = buffer[i];
                        buffer[i] = buffer[j];
                        buffer[j] = temp;
                    }
                }

                return buffer.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static T PickRandom<T>(this ICollection<T> source, Random r)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToList();
            var c = list.Count;

            if (c == 0)
            {
                throw new ArgumentException("Count should not larger than collection size");
            }
            
            var index = c == 0 ? 0 : r.Next(c);
            return list[index];
        }

        public static ICollection<T> PickRandom<T>(this ICollection<T> source, Random r, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var list = source.ToList();
            var c = list.Count;

            if (count > c)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count should not larger than collection size");
            }

            if (count == c)
            {
                return list;
            }

            var selected = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                var index = r.Next(c-i);
                var sel = list[index];
                selected.Add(sel);
                list.Remove(sel);
            }

            return selected;
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, Random random)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new RandomEnumerable<T>(source, random);
        }
    }
}
