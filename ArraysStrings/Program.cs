using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArraysStrings
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Select task (1-3) or 0 to exit:");
                string input = Console.ReadLine();
                if (input == "0") break;
                switch (input)
                {
                    case "1":
                        RunTask1();
                        break;
                    case "2":
                        RunTask2();
                        break;
                    case "3":
                        RunTask3();
                        break;
                    default:
                        Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }

        static void RunTask1()
        {
            int[,] array = new int[5, 5];
            Random rand = new Random();
            int min = 101, max = -101;
            int minPos = -1, maxPos = -1;

            Console.WriteLine("Array:");
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    array[i, j] = rand.Next(-100, 101);
                    Console.Write($"{array[i, j],4} ");
                    
                    if (array[i, j] < min)
                    {
                        min = array[i, j];
                        minPos = i * 5 + j;
                    }
                    if (array[i, j] > max)
                    {
                        max = array[i, j];
                        maxPos = i * 5 + j;
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine($"Min: {min} (pos {minPos}), Max: {max} (pos {maxPos})");

            int start = Math.Min(minPos, maxPos);
            int end = Math.Max(minPos, maxPos);
            int sum = 0;

            for (int k = start + 1; k < end; k++)
            {
                int r = k / 5;
                int c = k % 5;
                sum += array[r, c];
            }

            Console.WriteLine($"Sum of elements between min and max: {sum}");
        }

        static void RunTask2()
        {
            Console.WriteLine("Enter arithmetic expression (only + and -):");
            string expression = Console.ReadLine();
            
            // Remove spaces
            expression = expression.Replace(" ", "");

            // Parse numbers and operators
            List<double> numbers = new List<double>();
            List<char> operators = new List<char>();

            string currentNumber = "";
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (char.IsDigit(c) || c == '.')
                {
                    currentNumber += c;
                }
                else if (c == '+' || c == '-')
                {
                    if (currentNumber != "")
                    {
                        numbers.Add(double.Parse(currentNumber));
                        currentNumber = "";
                    }
                    operators.Add(c);
                }
            }
            if (currentNumber != "")
            {
                numbers.Add(double.Parse(currentNumber));
            }

            if (numbers.Count == 0)
            {
                Console.WriteLine("Invalid expression");
                return;
            }

            double result = numbers[0];
            for (int i = 0; i < operators.Count; i++)
            {
                if (i + 1 < numbers.Count)
                {
                    if (operators[i] == '+')
                    {
                        result += numbers[i + 1];
                    }
                    else if (operators[i] == '-')
                    {
                        result -= numbers[i + 1];
                    }
                }
            }

            Console.WriteLine($"Result: {result}");
        }

        static void RunTask3()
        {
            string text = "To be, or not to be, that is the question,\n" +
                          "Whether 'tis nobler in the mind to suffer\n" +
                          "The slings and arrows of outrageous fortune,\n" +
                          "Or to take arms against a sea of troubles,\n" +
                          "And by opposing end them? To die: to sleep;\n" +
                          "No more; and by a sleep to say we end\n" +
                          "The heart-ache and the thousand natural shocks\n" +
                          "That flesh is heir to, 'tis a consummation\n" +
                          "Devoutly to be wish'd. To die, to sleep";

            Console.WriteLine("Text:");
            Console.WriteLine(text);
            Console.WriteLine("\nEnter forbidden word:");
            string forbiddenWord = Console.ReadLine();

            if (string.IsNullOrEmpty(forbiddenWord)) return;

            string replacement = new string('*', forbiddenWord.Length);
            int count = 0;

            string result = Regex.Replace(text, Regex.Escape(forbiddenWord), match =>
            {
                count++;
                return replacement;
            }, RegexOptions.IgnoreCase);

            Console.WriteLine("\nResult:");
            Console.WriteLine(result);
            Console.WriteLine($"\nStatistics: {count} replacements of word '{forbiddenWord}'.");
        }
    }
}