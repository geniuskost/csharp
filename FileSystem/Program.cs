using System;

namespace PoemCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            PoemManager manager = new PoemManager();
            string filename = "poems.xml";

            while (true)
            {
                Console.WriteLine("\n--- Poem Collection Manager ---");
                Console.WriteLine("1. Add Poem");
                Console.WriteLine("2. Remove Poem");
                Console.WriteLine("3. Update Poem");
                Console.WriteLine("4. Search Poem");
                Console.WriteLine("5. Display All Poems");
                Console.WriteLine("6. Save to File");
                Console.WriteLine("7. Load from File");
                Console.WriteLine("0. Exit");
                Console.Write("Select option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        manager.AddPoem(CreatePoem());
                        break;
                    case "2":
                        Console.Write("Enter title of poem to remove: ");
                        string removeTitle = Console.ReadLine();
                        manager.RemovePoem(removeTitle);
                        break;
                    case "3":
                        Console.Write("Enter title of poem to update: ");
                        string updateTitle = Console.ReadLine();
                        Console.WriteLine("Enter new details:");
                        manager.UpdatePoem(updateTitle, CreatePoem());
                        break;
                    case "4":
                        Console.WriteLine("Search by: Title, Author, Year, Text, Theme");
                        Console.Write("Enter search type: ");
                        string type = Console.ReadLine();
                        Console.Write("Enter search query (Regex supported): ");
                        string query = Console.ReadLine();
                        manager.Search(query, type);
                        break;
                    case "5":
                        manager.DisplayAll();
                        break;
                    case "6":
                        manager.SaveToFile(filename);
                        break;
                    case "7":
                        manager.LoadFromFile(filename);
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static Poem CreatePoem()
        {
            Console.Write("Enter Title: ");
            string title = Console.ReadLine();
            Console.Write("Enter Author: ");
            string author = Console.ReadLine();
            Console.Write("Enter Year: ");
            int year;
            while (!int.TryParse(Console.ReadLine(), out year))
            {
                Console.Write("Invalid year. Enter again: ");
            }
            Console.Write("Enter Theme: ");
            string theme = Console.ReadLine();
            Console.WriteLine("Enter Text (end with empty line):");
            string text = "";
            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                text += line + Environment.NewLine;
            }

            return new Poem(title, author, year, text.Trim(), theme);
        }
    }
}