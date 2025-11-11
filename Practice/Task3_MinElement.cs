using System;

namespace Practice
{
    class Task3_MinElement
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача 3: Мінімальний елемент масиву ===\n");

            int[] arr = new int[15];
            Random random = new Random();

            Console.WriteLine("Згенерований масив:");
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = random.Next(-50, 51);
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine("\n");

            int minValue = arr[0];
            int minIndex = 0;

            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] < minValue)
                {
                    minValue = arr[i];
                    minIndex = i;
                }
            }

            Console.WriteLine($"Найменший елемент: {minValue}");
            Console.WriteLine($"Індекс найменшого елемента: {minIndex}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
