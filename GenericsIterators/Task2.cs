using System;
using System.Collections.Generic;
using System.Linq;

namespace GenericsIterators
{
    public class Task2Runner
    {
        public static IEnumerable<T> FilterByTwoCriteria<T>(IEnumerable<T> collection, Predicate<T> pred1, Predicate<T> pred2)
        {
            List<T> result = new List<T>();
            foreach (var item in collection)
            {
                if (pred1(item) && pred2(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static void Run()
        {
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var evenAndGreaterThan5 = FilterByTwoCriteria(numbers, n => n % 2 == 0, n => n > 5);
            Console.WriteLine("Numbers (Even AND > 5): " + string.Join(", ", evenAndGreaterThan5));

            List<string> words = new List<string> { "apple", "banana", "avocado", "cherry", "apricot" };
            var startsWithAAndLongerThan5 = FilterByTwoCriteria(words, w => w.StartsWith("a"), w => w.Length > 5);
            Console.WriteLine("Words (Starts with 'a' AND Length > 5): " + string.Join(", ", startsWithAAndLongerThan5));
        }
    }
}