using System;

class Program
{
    static void Main()
    {
        var rnd = new Random();
        Console.WriteLine("Завдання 1. Підрахунок парних чисел");
        int[] arr1 = CreateRandomArray(rnd, 10, 1, 50);
        PrintArray(arr1, "Масив:");
        int evenCount = CountEven(arr1);
        Console.WriteLine($"Кількість парних чисел: {evenCount}");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("Завдання 2. Заміна від’ємних чисел");
        int[] arr2 = CreateRandomArray(rnd, 12, -20, 20);
        PrintArray(arr2, "Масив до змін:");
        for (int i = 0; i < arr2.Length; i++)
        {
            if (arr2[i] < 0) arr2[i] = 0;
        }
        PrintArray(arr2, "Масив після заміни від'ємних на 0:");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("Завдання 3. Максимальний елемент");
        int[] arr3 = CreateRandomArray(rnd, 15, 10, 99);
        PrintArray(arr3, "Масив:");
        FindMaxWithIndex(arr3, out int maxValue, out int maxIndex);
        Console.WriteLine($"Найбільший елемент: {maxValue}, його індекс (перший): {maxIndex}");
    }

    static int[] CreateRandomArray(Random rnd, int length, int minValue, int maxValue)
    {
        int[] a = new int[length];
        for (int i = 0; i < length; i++)
        {
            a[i] = rnd.Next(minValue, maxValue + 1);
        }
        return a;
    }

    static void PrintArray(int[] a, string title = "")
    {
        if (!string.IsNullOrEmpty(title)) Console.WriteLine(title);
        for (int i = 0; i < a.Length; i++)
        {
            Console.Write(a[i]);
            if (i < a.Length - 1) Console.Write(", ");
        }
        Console.WriteLine();
    }

    static int CountEven(int[] a)
    {
        int count = 0;
        foreach (var x in a)
        {
            if (x % 2 == 0) count++;
        }
        return count;
    }

    static void FindMaxWithIndex(int[] a, out int maxValue, out int maxIndex)
    {
        maxValue = a[0];
        maxIndex = 0;
        for (int i = 1; i < a.Length; i++)
        {
            if (a[i] > maxValue)
            {
                maxValue = a[i];
                maxIndex = i;
            }
        }
    }
}
