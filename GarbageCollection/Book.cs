using System;

namespace GarbageCollection
{
    public class Book : IDisposable
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public int Pages { get; set; }

        public Book(string title, string author, int year, int pages)
        {
            Title = title;
            Author = author;
            Year = year;
            Pages = pages;
        }

        public void Display()
        {
            Console.WriteLine($"Title: {Title}, Author: {Author}, Year: {Year}, Pages: {Pages}");
        }

        public void Dispose()
        {
            Console.WriteLine($"Dispose called for book: {Title}");
            GC.SuppressFinalize(this);
        }

        ~Book()
        {
            Console.WriteLine($"Finalizer called for book: {Title}");
        }
    }
}