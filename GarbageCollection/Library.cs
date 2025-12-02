using System;
using System.Collections.Generic;

namespace GarbageCollection
{
    public class Library : IDisposable
    {
        private List<Book> books = new List<Book>();

        public void AddBook(Book book)
        {
            books.Add(book);
        }

        public void DisplayAll()
        {
            Console.WriteLine("Library contents:");
            foreach (var book in books)
            {
                book.Display();
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose called for Library. Clearing books...");
            foreach (var book in books)
            {
                book.Dispose();
            }
            books.Clear();
            GC.SuppressFinalize(this);
        }

        ~Library()
        {
            Console.WriteLine("Finalizer called for Library");
        }
    }
}