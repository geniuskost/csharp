using System;
using System.Collections.Generic;
using System.Linq;

namespace DelegatesEvents
{
    public class Item
    {
        public string Name { get; set; }
        public double Volume { get; set; }

        public Item(string name, double volume)
        {
            Name = name;
            Volume = volume;
        }
    }

    public class Backpack
    {
        public string Color { get; private set; }
        public string Brand { get; private set; }
        public string Fabric { get; private set; }
        public double Weight { get; private set; }
        public double MaxVolume { get; private set; }
        public List<Item> Contents { get; private set; } = new List<Item>();

        public event EventHandler<string> ItemAdded;
        public event EventHandler<string> ItemRemoved;
        public event EventHandler<string> CharacteristicsChanged;

        public Backpack(double maxVolume)
        {
            MaxVolume = maxVolume;
        }

        public void SetCharacteristics(string color, string brand, string fabric, double weight, double maxVolume)
        {
            double currentVolume = Contents.Sum(i => i.Volume);
            if (maxVolume < currentVolume)
            {
                throw new Exception("New volume is too small for current contents!");
            }

            Color = color;
            Brand = brand;
            Fabric = fabric;
            Weight = weight;
            MaxVolume = maxVolume;

            CharacteristicsChanged?.Invoke(this, "Backpack characteristics updated.");
        }

        public void AddItem(Item item)
        {
            double currentVolume = Contents.Sum(i => i.Volume);
            if (currentVolume + item.Volume > MaxVolume)
            {
                throw new Exception($"Cannot add {item.Name}. Not enough space.");
            }

            Contents.Add(item);
            ItemAdded?.Invoke(this, $"Item added: {item.Name}");
        }

        public void RemoveItem(string itemName)
        {
            var item = Contents.FirstOrDefault(i => i.Name == itemName);
            if (item != null)
            {
                Contents.Remove(item);
                ItemRemoved?.Invoke(this, $"Item removed: {itemName}");
            }
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            Backpack myBackpack = new Backpack(20.0);

            myBackpack.ItemAdded += delegate (object sender, string e)
            {
                Console.WriteLine($"[Event] {e}");
            };

            myBackpack.ItemRemoved += delegate (object sender, string e)
            {
                Console.WriteLine($"[Event] {e}");
            };

            myBackpack.CharacteristicsChanged += delegate (object sender, string e)
            {
                Console.WriteLine($"[Event] {e}");
            };

            try
            {
                myBackpack.SetCharacteristics("Black", "Nike", "Nylon", 1.5, 30.0);
                myBackpack.AddItem(new Item("Laptop", 5.0));
                myBackpack.AddItem(new Item("Water Bottle", 2.0));
                myBackpack.AddItem(new Item("Tent", 25.0)); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Exception] {ex.Message}");
            }

            myBackpack.RemoveItem("Water Bottle");
        }
    }
}