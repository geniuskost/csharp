using System;

namespace GarbageCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Task 1: Book ---");
            TestBook();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("\n--- Task 2: Library ---");
            TestLibrary();
        }

        static void TestBook()
        {
            Book b1 = new Book("The C# Programming Language", "Anders Hejlsberg", 2010, 800);
            b1.Display();
            b1.Dispose();

            Book b2 = new Book("Forgotten Book", "Unknown", 1900, 100);
            b2.Display();
        }

        static void TestLibrary()
        {
            using (Library lib = new Library())
            {
                lib.AddBook(new Book("1984", "George Orwell", 1949, 328));
                lib.AddBook(new Book("Brave New World", "Aldous Huxley", 1932, 311));
                lib.DisplayAll();
            } 
        }
    }
}