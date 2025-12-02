using System;

namespace ClassesStructures
{
    public struct Employee
    {
        public string Name;
        public int? Age;
        public decimal? Salary;

        public Employee(string name, int? age, decimal? salary)
        {
            Name = name;
            Age = age;
            Salary = salary;
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Age: {(Age.HasValue ? Age.Value.ToString() : "Unknown")}");
            Console.WriteLine($"Salary: {(Salary.HasValue ? Salary.Value.ToString() : "Not specified")}");
            Console.WriteLine();
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            Employee e1 = new Employee("John Doe", 30, 50000m);
            Employee e2 = new Employee("Jane Smith", null, 60000m);
            Employee e3 = new Employee("Bob Johnson", 45, null);
            Employee e4 = new Employee("Alice Brown", null, null);

            e1.DisplayInfo();
            e2.DisplayInfo();
            e3.DisplayInfo();
            e4.DisplayInfo();
        }
    }
}