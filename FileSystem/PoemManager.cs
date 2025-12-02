using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace PoemCollection
{
    public class PoemManager
    {
        private List<Poem> poems = new List<Poem>();

        public void AddPoem(Poem poem)
        {
            poems.Add(poem);
            Console.WriteLine("Poem added successfully.");
        }

        public void RemovePoem(string title)
        {
            var poem = poems.FirstOrDefault(p => p.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (poem != null)
            {
                poems.Remove(poem);
                Console.WriteLine("Poem removed successfully.");
            }
            else
            {
                Console.WriteLine("Poem not found.");
            }
        }

        public void UpdatePoem(string title, Poem newPoem)
        {
            var index = poems.FindIndex(p => p.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
            if (index != -1)
            {
                poems[index] = newPoem;
                Console.WriteLine("Poem updated successfully.");
            }
            else
            {
                Console.WriteLine("Poem not found.");
            }
        }

        public void Search(string query, string searchType)
        {
            Regex regex = new Regex(query, RegexOptions.IgnoreCase);
            List<Poem> results = new List<Poem>();

            switch (searchType.ToLower())
            {
                case "title":
                    results = poems.Where(p => regex.IsMatch(p.Title)).ToList();
                    break;
                case "author":
                    results = poems.Where(p => regex.IsMatch(p.Author)).ToList();
                    break;
                case "text":
                    results = poems.Where(p => regex.IsMatch(p.Text)).ToList();
                    break;
                case "theme":
                    results = poems.Where(p => regex.IsMatch(p.Theme)).ToList();
                    break;
                case "year":
                    results = poems.Where(p => regex.IsMatch(p.Year.ToString())).ToList();
                    break;
                default:
                    Console.WriteLine("Invalid search type.");
                    return;
            }

            if (results.Count > 0)
            {
                Console.WriteLine($"Found {results.Count} poems:");
                foreach (var p in results)
                {
                    Console.WriteLine(p);
                }
            }
            else
            {
                Console.WriteLine("No poems found matching the criteria.");
            }
        }

        public void SaveToFile(string filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Poem>));
                using (TextWriter writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, poems);
                }
                Console.WriteLine("Collection saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        public void LoadFromFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Poem>));
                    using (TextReader reader = new StreamReader(filename))
                    {
                        poems = (List<Poem>)serializer.Deserialize(reader);
                    }
                    Console.WriteLine("Collection loaded successfully.");
                }
                else
                {
                    Console.WriteLine("File not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file: {ex.Message}");
            }
        }

        public void DisplayAll()
        {
            if (poems.Count == 0)
            {
                Console.WriteLine("Collection is empty.");
                return;
            }

            foreach (var p in poems)
            {
                Console.WriteLine(p);
            }
        }
    }
}