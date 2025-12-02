using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            // Seed Data
            List<Firm> firms = new List<Firm>
            {
                new Firm { Name = "Food World", FoundationDate = DateTime.Now.AddYears(-3), BusinessProfile = "Marketing", DirectorName = "John White", EmployeeCount = 150, Address = "London" },
                new Firm { Name = "IT Solutions", FoundationDate = DateTime.Now.AddYears(-1), BusinessProfile = "IT", DirectorName = "James Black", EmployeeCount = 50, Address = "New York" },
                new Firm { Name = "White Food Inc", FoundationDate = DateTime.Now.AddYears(-5), BusinessProfile = "Marketing", DirectorName = "Alice Black", EmployeeCount = 200, Address = "London" },
                new Firm { Name = "Tech Corp", FoundationDate = DateTime.Now.AddYears(-10), BusinessProfile = "IT", DirectorName = "Bob Smith", EmployeeCount = 350, Address = "Paris" },
                new Firm { Name = "Fresh Foods", FoundationDate = DateTime.Now.AddDays(-120), BusinessProfile = "Retail", DirectorName = "Charlie Brown", EmployeeCount = 120, Address = "Berlin" },
                new Firm { Name = "Marketing Pro", FoundationDate = DateTime.Now.AddDays(-200), BusinessProfile = "Marketing", DirectorName = "David White", EmployeeCount = 80, Address = "London" }
            };

            // Add Employees for Task 3
            firms[0].Employees.Add(new Employee { FullName = "Lionel Messi", Position = "Manager", Phone = "23123456", Email = "lionel@foodworld.com", Salary = 5000 });
            firms[0].Employees.Add(new Employee { FullName = "John Doe", Position = "Developer", Phone = "12345678", Email = "john@foodworld.com", Salary = 3000 });
            firms[1].Employees.Add(new Employee { FullName = "Jane Doe", Position = "Manager", Phone = "23987654", Email = "di_jane@itsolutions.com", Salary = 6000 });
            firms[1].Employees.Add(new Employee { FullName = "Lionel Richie", Position = "Singer", Phone = "55555555", Email = "lionel@music.com", Salary = 10000 });
            firms[2].Employees.Add(new Employee { FullName = "Mark Spencer", Position = "Clerk", Phone = "11111111", Email = "mark@whitefood.com", Salary = 2000 });

            Console.WriteLine("=== Task 1: Query Syntax ===");
            RunTask1(firms);

            Console.WriteLine("\n=== Task 2: Method Syntax ===");
            RunTask2(firms);

            Console.WriteLine("\n=== Task 3: Employee Queries ===");
            RunTask3(firms);
        }

        static void RunTask1(List<Firm> firms)
        {
            Console.WriteLine("1. All firms:");
            var q1 = from f in firms select f;
            Print(q1);

            Console.WriteLine("2. Name contains 'Food':");
            var q2 = from f in firms where f.Name.Contains("Food") select f;
            Print(q2);

            Console.WriteLine("3. Profile is Marketing:");
            var q3 = from f in firms where f.BusinessProfile == "Marketing" select f;
            Print(q3);

            Console.WriteLine("4. Profile is Marketing or IT:");
            var q4 = from f in firms where f.BusinessProfile == "Marketing" || f.BusinessProfile == "IT" select f;
            Print(q4);

            Console.WriteLine("5. Employees > 100:");
            var q5 = from f in firms where f.EmployeeCount > 100 select f;
            Print(q5);

            Console.WriteLine("6. Employees between 100 and 300:");
            var q6 = from f in firms where f.EmployeeCount >= 100 && f.EmployeeCount <= 300 select f;
            Print(q6);

            Console.WriteLine("7. Address is London:");
            var q7 = from f in firms where f.Address.Contains("London") select f;
            Print(q7);

            Console.WriteLine("8. Director surname is White:");
            var q8 = from f in firms where f.DirectorName.Contains("White") select f;
            Print(q8);

            Console.WriteLine("9. Founded > 2 years ago:");
            var q9 = from f in firms where (DateTime.Now - f.FoundationDate).TotalDays > 365 * 2 select f;
            Print(q9);

            Console.WriteLine("10. Founded >= 123 days ago:");
            var q10 = from f in firms where (DateTime.Now - f.FoundationDate).TotalDays >= 123 select f;
            Print(q10);

            Console.WriteLine("11. Director Black and Firm Name contains White:");
            var q11 = from f in firms where f.DirectorName.Contains("Black") && f.Name.Contains("White") select f;
            Print(q11);
        }

        static void RunTask2(List<Firm> firms)
        {
            Console.WriteLine("1. All firms:");
            Print(firms);

            Console.WriteLine("2. Name contains 'Food':");
            Print(firms.Where(f => f.Name.Contains("Food")));

            Console.WriteLine("3. Profile is Marketing:");
            Print(firms.Where(f => f.BusinessProfile == "Marketing"));

            Console.WriteLine("4. Profile is Marketing or IT:");
            Print(firms.Where(f => f.BusinessProfile == "Marketing" || f.BusinessProfile == "IT"));

            Console.WriteLine("5. Employees > 100:");
            Print(firms.Where(f => f.EmployeeCount > 100));

            Console.WriteLine("6. Employees between 100 and 300:");
            Print(firms.Where(f => f.EmployeeCount >= 100 && f.EmployeeCount <= 300));

            Console.WriteLine("7. Address is London:");
            Print(firms.Where(f => f.Address.Contains("London")));

            Console.WriteLine("8. Director surname is White:");
            Print(firms.Where(f => f.DirectorName.Contains("White")));

            Console.WriteLine("9. Founded > 2 years ago:");
            Print(firms.Where(f => (DateTime.Now - f.FoundationDate).TotalDays > 365 * 2));

            Console.WriteLine("10. Founded >= 123 days ago:");
            Print(firms.Where(f => (DateTime.Now - f.FoundationDate).TotalDays >= 123));

            Console.WriteLine("11. Director Black and Firm Name contains White:");
            Print(firms.Where(f => f.DirectorName.Contains("Black") && f.Name.Contains("White")));
        }

        static void RunTask3(List<Firm> firms)
        {
            var firm = firms[0]; // Food World
            Console.WriteLine($"1. All employees of {firm.Name}:");
            Print(firm.Employees);

            decimal salaryThreshold = 4000;
            Console.WriteLine($"2. Employees of {firm.Name} with salary > {salaryThreshold}:");
            Print(firm.Employees.Where(e => e.Salary > salaryThreshold));

            Console.WriteLine("3. Employees of ALL firms with position 'Manager':");
            var managers = firms.SelectMany(f => f.Employees).Where(e => e.Position == "Manager");
            Print(managers);

            Console.WriteLine("4. Phone starts with '23':");
            var phone23 = firms.SelectMany(f => f.Employees).Where(e => e.Phone.StartsWith("23"));
            Print(phone23);

            Console.WriteLine("5. Email starts with 'di':");
            var emailDi = firms.SelectMany(f => f.Employees).Where(e => e.Email.StartsWith("di"));
            Print(emailDi);

            Console.WriteLine("6. Name is 'Lionel':");
            var lionels = firms.SelectMany(f => f.Employees).Where(e => e.FullName.Contains("Lionel"));
            Print(lionels);
        }

        static void Print<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("----------------");
        }
    }
}