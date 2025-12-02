using System;

namespace DelegatesEvents
{
    public delegate string GetRGBDelegate(string color);

    public class Task1Runner
    {
        public static void Run()
        {
            GetRGBDelegate getRGB = delegate (string color)
            {
                switch (color.ToLower())
                {
                    case "red": return "255, 0, 0";
                    case "orange": return "255, 165, 0";
                    case "yellow": return "255, 255, 0";
                    case "green": return "0, 128, 0";
                    case "blue": return "0, 0, 255";
                    case "indigo": return "75, 0, 130";
                    case "violet": return "238, 130, 238";
                    default: return "Unknown color";
                }
            };

            string[] colors = { "Red", "Green", "Blue", "Violet", "Black" };
            foreach (var c in colors)
            {
                Console.WriteLine($"Color: {c}, RGB: {getRGB(c)}");
            }
        }
    }
}