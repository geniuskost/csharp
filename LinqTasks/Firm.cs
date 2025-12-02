using System;
using System.Collections.Generic;

namespace LinqTasks
{
    public class Firm
    {
        public string Name { get; set; }
        public DateTime FoundationDate { get; set; }
        public string BusinessProfile { get; set; }
        public string DirectorName { get; set; }
        public int EmployeeCount { get; set; } // For Task 1 & 2
        public string Address { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>(); // For Task 3

        public override string ToString()
        {
            return $"Firm: {Name}, Profile: {BusinessProfile}, Director: {DirectorName}, Employees: {EmployeeCount}, Address: {Address}, Founded: {FoundationDate.ToShortDateString()}";
        }
    }
}