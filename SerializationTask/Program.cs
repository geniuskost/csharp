using System;
using System.IO;
using System.Xml.Serialization;

namespace SerializationTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Magazine currentMagazine = null;
            string filename = "magazine_data.xml";

            while (true)
            {
                Console.WriteLine("\n--- Magazine Manager ---");
                Console.WriteLine("1. Create/Edit Magazine");
                Console.WriteLine("2. Add Article to Magazine");
                Console.WriteLine("3. Display Magazine Info");
                Console.WriteLine("4. Save to File (Serialize)");
                Console.WriteLine("5. Load from File (Deserialize)");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        currentMagazine = CreateMagazine();
                        break;
                    case "2":
                        if (currentMagazine != null)
                        {
                            currentMagazine.AddArticle(CreateArticle());
                        }
                        else
                        {
                            Console.WriteLine("Please create a magazine first.");
                        }
                        break;
                    case "3":
                        if (currentMagazine != null)
                        {
                            Console.WriteLine(currentMagazine);
                        }
                        else
                        {
                            Console.WriteLine("No magazine loaded.");
                        }
                        break;
                    case "4":
                        if (currentMagazine != null)
                        {
                            SaveMagazine(currentMagazine, filename);
                        }
                        else
                        {
                            Console.WriteLine("No magazine to save.");
                        }
                        break;
                    case "5":
                        currentMagazine = LoadMagazine(filename);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static Magazine CreateMagazine()
        {
            Console.WriteLine("\nEnter Magazine Details:");
            Console.Write("Title: ");
            string title = Console.ReadLine();
            Console.Write("Publisher: ");
            string publisher = Console.ReadLine();
            
            DateTime date;
            while (true)
            {
                Console.Write("Release Date (yyyy-mm-dd): ");
                if (DateTime.TryParse(Console.ReadLine(), out date)) break;
                Console.WriteLine("Invalid date format.");
            }

            int pages;
            while (true)
            {
                Console.Write("Page Count: ");
                if (int.TryParse(Console.ReadLine(), out pages)) break;
                Console.WriteLine("Invalid number.");
            }

            return new Magazine(title, publisher, date, pages);
        }

        static Article CreateArticle()
        {
            Console.WriteLine("\nEnter Article Details:");
            Console.Write("Title: ");
            string title = Console.ReadLine();
            
            int chars;
            while (true)
            {
                Console.Write("Character Count: ");
                if (int.TryParse(Console.ReadLine(), out chars)) break;
                Console.WriteLine("Invalid number.");
            }

            Console.Write("Preview/Abstract: ");
            string preview = Console.ReadLine();

            return new Article(title, chars, preview);
        }

        static void SaveMagazine(Magazine mag, string filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Magazine));
                using (TextWriter writer = new StreamWriter(filename))
                {
                    serializer.Serialize(writer, mag);
                }
                Console.WriteLine($"Magazine saved to {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving: {ex.Message}");
            }
        }

        static Magazine LoadMagazine(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Console.WriteLine("File not found.");
                    return null;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(Magazine));
                using (TextReader reader = new StreamReader(filename))
                {
                    Magazine mag = (Magazine)serializer.Deserialize(reader);
                    Console.WriteLine("Magazine loaded successfully.");
                    return mag;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading: {ex.Message}");
                return null;
            }
        }
    }
}