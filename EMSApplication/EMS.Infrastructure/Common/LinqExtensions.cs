using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace EMS.Infrastructure.Common
{
    public static class LinqExtensions
    {
        [DebuggerNonUserCode]
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> sequence, int size)
        {
            List<T> partition = new List<T>(size);
            foreach (var item in sequence)
            {
                partition.Add(item);
                if (partition.Count == size)
                {
                    yield return partition;
                    partition = new List<T>(size);
                }
            }
            if (partition.Count > 0)
                yield return partition;
        }

        [Pure]
        public static bool AllSame<T, TProperty>(this IEnumerable<T> enumerable, Func<T, TProperty> propertyCall)
        {
            return !enumerable.Select(propertyCall)
                      .Distinct()
                      .Skip(1)
                      .Any();
        }

        /// <summary>
        /// shortcut that checks whether a collection is empty or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>
        ///   <c>true</c> if the specified enumerable is empty; otherwise, <c>false</c>.
        /// </returns>
        [Pure]
        [DebuggerNonUserCode]
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        [Pure]
        [DebuggerNonUserCode]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        /// <summary>
        /// Adds a range of values to an ilist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="items">The items.</param>
        [DebuggerNonUserCode]
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        [DebuggerNonUserCode]
        public static void RemoveRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Remove(item);
            }
        }

        /// <summary>
        /// Determines whether the specified node is last.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if the specified node is last; otherwise, <c>false</c>.
        /// </returns>
        [Pure]
        [DebuggerNonUserCode]
        public static bool IsLast<T>(this LinkedListNode<T> node)
        {
            return node.Next == null;
        }

        /// <summary>
        /// Creates a linked list out of the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> enumerable)
        {
            var linkedList = new LinkedList<T>(enumerable);
            return linkedList;
        }

        /// <summary>
        /// Creates a stack (LIFO) out of the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static Stack<T> ToStack<T>(this IEnumerable<T> enumerable)
        {
            var stack = new Stack<T>(enumerable);
            return stack;
        }

        /// <summary>
        /// Creates a queue (FIFO) out of the enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
        {
            var queue = new Queue<T>(enumerable);
            return queue;
        }

        /// <summary>
        /// Gets the object with the least amount of one of its properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static IEnumerable<T> WhereHasLeast<T, TKey>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector)
        {
            if (enumerable == null) return Enumerable.Empty<T>();
            return enumerable.Where(a => Equals(keySelector(a), enumerable.SelectLeast(keySelector)));
        }

        /// <summary>
        /// Gets the object with the greatest amount of one of its properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static IEnumerable<T> WhereHasGreatest<T, TKey>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector)
        {
            if (enumerable == null) return Enumerable.Empty<T>();
            return enumerable.Where(a => Equals(keySelector(a), enumerable.SelectGreatest(keySelector)));
        }

        /// <summary>
        /// Gets the object with the greatest amount of one of its properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static TKey SelectGreatest<T, TKey>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector)
        {
            if (enumerable.IsNullOrEmpty()) return default;
            return enumerable.Max(keySelector);
        }

        /// <summary>
        /// Gets the object with the least amount of one of its properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        public static TKey SelectLeast<T, TKey>(this IEnumerable<T> enumerable,
            Func<T, TKey> keySelector)
        {
            if (enumerable.IsNullOrEmpty()) return default;
            return enumerable.Min(keySelector);
        }

        /// <summary>
        /// Returns the Sample Standard Deviation
        /// http://stackoverflow.com/a/2253903
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static double StdDevS(this IEnumerable<double> enumerable)
        {
            double ret = 0;
            int count = enumerable.Count();
            if (count > 1)
            {
                //Compute the Average
                double avg = enumerable.Average();

                //Perform the Sum of (value-avg)^2
                double sum = enumerable.Sum(d => (d - avg) * (d - avg));

                //Put it all together
                ret = Math.Sqrt(sum / (count - 1));
            }
            return ret;
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item)
        {
            return enumerable.Except(new List<T> { item });
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T item)
        {
            return enumerable.Concat(new List<T> { item });
        }

        public static void ForEach<T>(this IEnumerable<T> query, Action<T> method)
        {
            foreach (T item in query)
            {
                method(item);
            }
        }
    }
}
