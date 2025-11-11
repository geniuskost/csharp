using System;

namespace Practice
{
    class Task2_ReplaceNumbers
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача 2: Заміна чисел, більших за 10 ===\n");

            int[] arr = new int[12];
            Random random = new Random();

            Console.WriteLine("Масив ДО заміни:");
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = random.Next(-15, 26);
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine("\n");

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > 10)
                {
                    arr[i] = 10;
                }
            }

            Console.WriteLine("Масив ПІСЛЯ заміни (всі числа > 10 замінено на 10):");
            for (int i = 0; i < arr.Length; i++)
            {
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine();

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
