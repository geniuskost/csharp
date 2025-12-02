using System;

namespace PoemCollection
{
    [Serializable]
    public class Poem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
        public string Text { get; set; }
        public string Theme { get; set; }

        public Poem() { }

        public Poem(string title, string author, int year, string text, string theme)
        {
            Title = title;
            Author = author;
            Year = year;
            Text = text;
            Theme = theme;
        }

        public override string ToString()
        {
            return $"Title: {Title}\nAuthor: {Author}\nYear: {Year}\nTheme: {Theme}\nText:\n{Text}\n-------------------";
        }
    }
}