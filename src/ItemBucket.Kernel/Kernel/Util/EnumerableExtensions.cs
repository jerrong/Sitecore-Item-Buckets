using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sitecore.Collections;
using Sitecore.ItemBucket.Kernel.Kernel.Util;

namespace Sitecore.ItemBucket.Kernel.Util
{
    public static class EnumerableExtensions
    {
        public static T FirstOrLazy<T>(this IEnumerable<T> sequence, Func<T> lazy)
        {
            foreach (T item in sequence)
                return item;
            return lazy();
        }

        public static bool Has<T>(this IEnumerable<T> source, Expression<Func<long, bool>> countExpression)
        {
            if (source.IsNull()) throw new ArgumentNullException("source");
            if (countExpression.IsNull()) throw new ArgumentNullException("countExpression");

            long? leftBoundOffset, rightBoundOffset;

            var operation = countExpression.Body as BinaryExpression;

            if (operation.IsNull())
                throw new ArgumentException("Invalid expression.", "countExpression");

            bool isInverted = false, isNegated = false;
            Expression valueExpression;

            var parameterExpression = countExpression.Parameters.Single();

            if (operation.Left == parameterExpression)
                valueExpression = operation.Right;
            else if (isInverted = operation.Right == parameterExpression)
                valueExpression = operation.Left;
            else
                throw new ArgumentException("Count parameter is missing.", "countExpression");

            switch (countExpression.Body.NodeType)
            {
                case ExpressionType.NotEqual:
                    isNegated = true;
                    goto case ExpressionType.Equal;

                case ExpressionType.Equal:
                    leftBoundOffset = -1;
                    rightBoundOffset = 1;
                    break;

                case ExpressionType.GreaterThan:
                    if (isInverted)
                    {
                        isInverted = false;
                        goto case ExpressionType.LessThan;
                    }

                    leftBoundOffset = 0;
                    rightBoundOffset = null;
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    if (isInverted)
                    {
                        isInverted = false;
                        goto case ExpressionType.LessThanOrEqual;
                    }

                    leftBoundOffset = -1;
                    rightBoundOffset = null;
                    break;

                case ExpressionType.LessThan:
                    if (isInverted)
                    {
                        isInverted = false;
                        goto case ExpressionType.GreaterThan;
                    }

                    leftBoundOffset = null;
                    rightBoundOffset = 0;
                    break;

                case ExpressionType.LessThanOrEqual:
                    if (isInverted)
                    {
                        isInverted = false;
                        goto case ExpressionType.GreaterThanOrEqual;
                    }

                    leftBoundOffset = null;
                    rightBoundOffset = 1;
                    break;

                default:
                    throw new ArgumentException("Invalid expression.", "countExpression");
            }

            long value;

            try
            {
                value = (long)Expression.Lambda(valueExpression).Compile().DynamicInvoke();
            }
            catch (InvalidOperationException error)
            {
                throw new ArgumentException("Error executing count comparison value expression.", "countExpression", error);
            }

            var leftBound = value + leftBoundOffset;
            var rightBound = value + rightBoundOffset;

            using (var enumerator = source.GetEnumerator())
            {
                long count = 0;

                if (leftBound.HasValue)
                    while (count <= leftBound)
                    {
                        if (!enumerator.MoveNext())
                            return isNegated;

                        count++;
                    }

                if (rightBound.HasValue)
                {
                    while (count < rightBound)
                    {
                        if (!enumerator.MoveNext())
                            return !isNegated;

                        count++;
                    }

                    return isNegated;
                }

                return true;
            }
        }

        public static IDictionary<string, TValue> Map<TValue>(this IDictionary<string, TValue>
    dictionary, Expression<Func<object, TValue>> pair)
        {
            var key = pair.Parameters[0].Name;
            var eval = pair.Compile().Invoke(new object[1]);
            dictionary[key] = eval;
            return dictionary;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T element)
        {
            yield return element;

            foreach (var item in source)
            {
                yield return item;
            }
        }
        public static SafeDictionary<string, TValue> Map<TValue>(this SafeDictionary<string, TValue>
dictionary, Expression<Func<object, TValue>> pair)
        {
            var key = pair.Parameters[0].Name;
            var eval = pair.Compile().Invoke(new object[1]);

            dictionary[key] = eval;

            return dictionary;
        }
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static IEnumerable<T> Once<T>(this IEnumerable<T> collection,
           Func<IEnumerable<T>, bool> predicate, Action<IEnumerable<T>> action)
        {
            if (predicate(collection))
                action(collection);
            return collection;
        }

        public static IEnumerable<T> ForEvery<T>(this IEnumerable<T> collection,
           Func<T, bool> predicate, Action<T> action)
        {
            foreach (var item in collection.Where(predicate))
            {
                action(item);
            }
            return collection;
        }

        public static void AddAll<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, IEnumerable<TItem> additions)
            where TCol : ICollection<TItem>, new()
        {
            if (dictionary.IsNull())
                throw new ArgumentNullException("dictionary");

            if (key.IsNull())
                throw new ArgumentNullException("key");

            if (additions.IsNull())
                throw new ArgumentNullException("additions");

            TCol col;
            if (!dictionary.TryGetValue(key, out col))
                dictionary.Add(key, col = new TCol());

            foreach (var item in additions)
                col.Add(item);
        }

        public static void Remove<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
            where TCol : ICollection<TItem>
        {
            if (dictionary.IsNull())
                throw new ArgumentNullException("dictionary");

            if (key.IsNull())
                throw new ArgumentNullException("key");

            TCol col;
            if (dictionary.TryGetValue(key, out col))
                col.Remove(item);
        }

        public static IEnumerable<T> RemoveWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
                yield break;

            foreach (T t in source.Where(t => !predicate(t)))
            {
                yield return t;
            }
        }

        public static bool In<T>(this T source, params T[] list)
        {
            if (source.IsNull()) throw new ArgumentNullException("source");
            return list.Contains(source);
        }

        public static void Add<TKey, TCol, TItem>(this IDictionary<TKey, TCol> dictionary, TKey key, TItem item)
            where TCol : ICollection<TItem>, new()
        {
            if (dictionary.IsNull())
                throw new ArgumentNullException("dictionary");

            if (key.IsNull())
                throw new ArgumentNullException("key");

            TCol col;
            if (dictionary.TryGetValue(key, out col))
            {
                if (col.IsReadOnly)
                    throw new InvalidOperationException("bucket is read only");
            }
            else
                dictionary.Add(key, col = new TCol());

            col.Add(item);
        }
    }
}
