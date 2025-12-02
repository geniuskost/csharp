using System;

namespace GenericsIterators
{
    public class Task1Runner
    {
        public static T Max<T>(T a, T b, T c) where T : IComparable<T>
        {
            T max = a;
            if (b.CompareTo(max) > 0) max = b;
            if (c.CompareTo(max) > 0) max = c;
            return max;
        }

        public static void Run()
        {
            Console.WriteLine($"Max(1, 5, 3): {Max(1, 5, 3)}");
            Console.WriteLine($"Max(10.5, 2.3, 15.7): {Max(10.5, 2.3, 15.7)}");
            Console.WriteLine($"Max(\"Apple\", \"Pear\", \"Banana\"): {Max("Apple", "Pear", "Banana")}");
        }
    }
}