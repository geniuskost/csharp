using System;

namespace Practice
{
    class Task5_SumInRange
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача 5: Сума чисел у діапазоні [30, 70] ===\n");

            int[] arr = new int[20];
            Random random = new Random();

            Console.WriteLine("Згенерований масив:");
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = random.Next(1, 101);
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine("\n");

            int sum = 0;
            int count = 0;
            Console.WriteLine("Числа у діапазоні [30, 70]:");

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] >= 30 && arr[i] <= 70)
                {
                    Console.Write(arr[i] + " ");
                    sum += arr[i];
                    count++;
                }
            }

            Console.WriteLine("\n");
            Console.WriteLine($"Кількість чисел у діапазоні: {count}");
            Console.WriteLine($"Сума чисел у діапазоні [30, 70]: {sum}");

            if (count > 0)
            {
                double average = (double)sum / count;
                Console.WriteLine($"Середнє арифметичне: {average:F2}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
