using System;

namespace DelegatesEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nSelect task (1-3) or 0 to exit:");
                string input = Console.ReadLine();
                if (input == "0") break;
                
                switch (input)
                {
                    case "1":
                        Console.WriteLine("--- Task 1 ---");
                        Task1Runner.Run();
                        break;
                    case "2":
                        Console.WriteLine("--- Task 2 ---");
                        Task2Runner.Run();
                        break;
                    case "3":
                        Console.WriteLine("--- Task 3 ---");
                        Task3Runner.Run();
                        break;
                    default:
                        Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }
    }
}