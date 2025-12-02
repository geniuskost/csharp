using System;

namespace ClassesStructures2
{
    public struct DecimalNumber
    {
        public int Value;

        public DecimalNumber(int value)
        {
            Value = value;
        }

        public string ToBinary()
        {
            return Convert.ToString(Value, 2);
        }

        public string ToOctal()
        {
            return Convert.ToString(Value, 8);
        }

        public string ToHex()
        {
            return Convert.ToString(Value, 16).ToUpper();
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            Console.WriteLine("Enter a decimal number:");
            if (int.TryParse(Console.ReadLine(), out int val))
            {
                DecimalNumber num = new DecimalNumber(val);
                Console.WriteLine($"Binary: {num.ToBinary()}");
                Console.WriteLine($"Octal: {num.ToOctal()}");
                Console.WriteLine($"Hex: {num.ToHex()}");
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}