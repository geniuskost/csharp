using System;

namespace Practice
{
    class Task1_OddNumbers
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача 1: Підрахунок непарних чисел ===\n");

            int[] arr = new int[10];
            Random random = new Random();

            Console.WriteLine("Згенерований масив:");
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = random.Next(5, 56);
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine("\n");

            int oddCount = 0;
            Console.WriteLine("Непарні числа:");
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] % 2 != 0)
                {
                    Console.Write(arr[i] + " ");
                    oddCount++;
                }
            }

            Console.WriteLine($"\n\nКількість непарних чисел: {oddCount}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
