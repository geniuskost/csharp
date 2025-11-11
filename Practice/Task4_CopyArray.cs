using System;

namespace Practice
{
    class Task4_CopyArray
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача 4: Копіювання масиву за правилом ===\n");
            Console.WriteLine("Правило: спочатку < 0, потім == 0, потім > 0\n");

            int[] arr = new int[15];
            Random random = new Random();

            Console.WriteLine("Оригінальний масив:");
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = random.Next(-20, 21);
                Console.Write(arr[i] + " ");
            }
            Console.WriteLine("\n");

            int[] newArr = new int[arr.Length];
            int index = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < 0)
                {
                    newArr[index] = arr[i];
                    index++;
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == 0)
                {
                    newArr[index] = arr[i];
                    index++;
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > 0)
                {
                    newArr[index] = arr[i];
                    index++;
                }
            }

            Console.WriteLine("Скопійований масив (< 0, == 0, > 0):");
            for (int i = 0; i < newArr.Length; i++)
            {
                Console.Write(newArr[i] + " ");
            }
            Console.WriteLine();

            Console.WriteLine("\nСтатистика:");
            int negativeCount = 0, zeroCount = 0, positiveCount = 0;
            foreach (int num in arr)
            {
                if (num < 0) negativeCount++;
                else if (num == 0) zeroCount++;
                else positiveCount++;
            }
            Console.WriteLine($"Від'ємних: {negativeCount}, Нулів: {zeroCount}, Додатніх: {positiveCount}");

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
