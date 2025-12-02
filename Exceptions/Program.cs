using System;

namespace MoneyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Money m1 = new Money(0, 0);
            Money m2 = new Money(0, 0);

            while (true)
            {
                Console.WriteLine("\n--- Money Class Demo ---");
                Console.WriteLine($"Current Money 1: {m1}");
                Console.WriteLine($"Current Money 2: {m2}");
                Console.WriteLine("1. Set Money 1");
                Console.WriteLine("2. Set Money 2");
                Console.WriteLine("3. Add (M1 + M2)");
                Console.WriteLine("4. Subtract (M1 - M2)");
                Console.WriteLine("5. Multiply M1 by integer");
                Console.WriteLine("6. Divide M1 by integer");
                Console.WriteLine("7. Increment M1 (++)");
                Console.WriteLine("8. Decrement M1 (--)");
                Console.WriteLine("9. Compare (M1 vs M2)");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            m1 = CreateMoney();
                            break;
                        case "2":
                            m2 = CreateMoney();
                            break;
                        case "3":
                            Console.WriteLine($"Result: {m1 + m2}");
                            break;
                        case "4":
                            Console.WriteLine($"Result: {m1 - m2}");
                            break;
                        case "5":
                            Console.Write("Enter multiplier: ");
                            int mul = int.Parse(Console.ReadLine());
                            Console.WriteLine($"Result: {m1 * mul}");
                            break;
                        case "6":
                            Console.Write("Enter divisor: ");
                            int div = int.Parse(Console.ReadLine());
                            Console.WriteLine($"Result: {m1 / div}");
                            break;
                        case "7":
                            m1++;
                            Console.WriteLine("M1 incremented.");
                            break;
                        case "8":
                            m1--;
                            Console.WriteLine("M1 decremented.");
                            break;
                        case "9":
                            if (m1 == m2) Console.WriteLine("M1 is equal to M2");
                            if (m1 != m2) Console.WriteLine("M1 is not equal to M2");
                            if (m1 > m2) Console.WriteLine("M1 is greater than M2");
                            if (m1 < m2) Console.WriteLine("M1 is less than M2");
                            break;
                        case "0":
                            return;
                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (BankruptException ex)
                {
                    Console.WriteLine($"\n!!! EXCEPTION: {ex.Message} !!!");
                }
                catch (FormatException)
                {
                    Console.WriteLine("\n!!! Invalid input format !!!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n!!! Error: {ex.Message} !!!");
                }
            }
        }

        static Money CreateMoney()
        {
            Console.Write("Enter Hryvnias: ");
            int h = int.Parse(Console.ReadLine());
            Console.Write("Enter Kopiykas: ");
            int k = int.Parse(Console.ReadLine());
            return new Money(h, k);
        }
    }
}