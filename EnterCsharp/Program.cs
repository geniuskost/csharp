using System;

namespace Task1
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
            Console.WriteLine("Enter temperature:");
            if (double.TryParse(Console.ReadLine(), out double temp))
            {
                Console.WriteLine("1. Fahrenheit to Celsius");
                Console.WriteLine("2. Celsius to Fahrenheit");
                string choice = Console.ReadLine();
                if (choice == "1")
                {
                    double c = (temp - 32) * 5 / 9;
                    Console.WriteLine($"Result: {c}");
                }
                else if (choice == "2")
                {
                    double f = (temp * 9 / 5) + 32;
                    Console.WriteLine($"Result: {f}");
                }
                else
                {
                    Console.WriteLine("Invalid choice");
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }

        static void RunTask2()
        {
            Console.WriteLine("Enter first number:");
            if (int.TryParse(Console.ReadLine(), out int n1))
            {
                Console.WriteLine("Enter second number:");
                if (int.TryParse(Console.ReadLine(), out int n2))
                {
                    if (n1 > n2)
                    {
                        int temp = n1;
                        n1 = n2;
                        n2 = temp;
                    }
                    for (int i = n1; i <= n2; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Console.WriteLine(i);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input");
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }

        static void RunTask3()
        {
            Console.WriteLine("Enter number:");
            if (int.TryParse(Console.ReadLine(), out int num))
            {
                int temp = num;
                int count = 0;
                while (temp > 0)
                {
                    temp /= 10;
                    count++;
                }
                temp = num;
                double sum = 0;
                while (temp > 0)
                {
                    int digit = temp % 10;
                    sum += Math.Pow(digit, count);
                    temp /= 10;
                }
                if ((int)sum == num)
                {
                    Console.WriteLine("It is an Armstrong number");
                }
                else
                {
                    Console.WriteLine("It is not an Armstrong number");
                }
            }
            else
            {
                Console.WriteLine("Invalid input");
            }
        }
    }
}
