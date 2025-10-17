using System;
using System.Linq;

class Program
{
    static int[] Rearrange(int[] arr)
    {
        return arr
            .Where(x => x > 0)
            .Concat(arr.Where(x => x == 0))
            .Concat(arr.Where(x => x < 0))
            .ToArray();
    }

    static void Main()
    {
        Console.WriteLine("Введіть числа через пробіл або кому (натисніть Enter щоб використати приклад):");
        var line = Console.ReadLine();

        int[] input;
        if (string.IsNullOrWhiteSpace(line))
        {
            input = new int[] { 9, 0, -7, 0, -2, 23, 3 };
            Console.WriteLine("Використано приклад:");
            Console.WriteLine(string.Join(", ", input));
        }
        else
        {
            var tokens = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                input = tokens.Select(t => int.Parse(t.Trim())).ToArray();
            }
            catch (FormatException)
            {
                Console.WriteLine("Невірний формат введення. Використано приклад замість введення.");
                input = new int[] { 9, 0, -7, 0, -2, 23, 3 };
            }
        }

        var result = Rearrange(input);
        Console.WriteLine("Результат:");
        Console.WriteLine(string.Join(", ", result));
    }
}
