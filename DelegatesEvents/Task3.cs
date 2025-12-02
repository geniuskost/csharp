using System;
using System.Linq;

namespace DelegatesEvents
{
    public class Task3Runner
    {
        public static void Run()
        {
            int[] numbers = { 1, 7, 14, 21, 5, 10, 35, 49, 8, 0 };

            Func<int[], int, int> countDivisible = (arr, divisor) =>
            {
                if (divisor == 0) return 0; 
                return arr.Count(n => n % divisor == 0);
            };

            int div7 = 7;
            Console.WriteLine($"Array: {string.Join(", ", numbers)}");
            Console.WriteLine($"Count divisible by {div7}: {countDivisible(numbers, div7)}");

            int div5 = 5;
            Console.WriteLine($"Count divisible by {div5}: {countDivisible(numbers, div5)}");
        }
    }
}