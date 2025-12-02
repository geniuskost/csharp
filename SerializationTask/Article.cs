using System;

namespace SerializationTask
{
    [Serializable]
    public class Article
    {
        public string Title { get; set; }
        public int CharCount { get; set; }
        public string Preview { get; set; }

        public Article() { }

        public Article(string title, int charCount, string preview)
        {
            Title = title;
            CharCount = charCount;
            Preview = preview;
        }

        public override string ToString()
        {
            return $"  - Article: {Title} ({CharCount} chars)\n    Preview: {Preview}";
        }
    }
}