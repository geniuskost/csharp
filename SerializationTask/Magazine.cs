using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SerializationTask
{
    [Serializable]
    public class Magazine
    {
        public string Title { get; set; }
        public string Publisher { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int PageCount { get; set; }
        
        [XmlArray("Articles")]
        [XmlArrayItem("Article")]
        public List<Article> Articles { get; set; } = new List<Article>();

        public Magazine() { }

        public Magazine(string title, string publisher, DateTime releaseDate, int pageCount)
        {
            Title = title;
            Publisher = publisher;
            ReleaseDate = releaseDate;
            PageCount = pageCount;
        }

        public void AddArticle(Article article)
        {
            Articles.Add(article);
        }

        public override string ToString()
        {
            string info = $"Magazine: {Title}\nPublisher: {Publisher}\nReleased: {ReleaseDate.ToShortDateString()}\nPages: {PageCount}\nArticles ({Articles.Count}):";
            foreach (var article in Articles)
            {
                info += "\n" + article.ToString();
            }
            return info;
        }
    }
}