using System;

namespace Interfaces
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Select task (1 or 2) or 0 to exit:");
                string input = Console.ReadLine();
                if (input == "0") break;
                
                if (input == "1")
                {
                    Task1Runner.Run();
                }
                else if (input == "2")
                {
                    Task2Runner.Run();
                }
                else
                {
                    Console.WriteLine("Invalid selection");
                }
            }
        }
    }
}