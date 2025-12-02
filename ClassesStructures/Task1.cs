using System;
using System.Collections.Generic;

namespace ClassesStructures
{
    public enum ItemCategory
    {
        Electronics,
        Furniture,
        Food
    }

    public struct Item
    {
        public string Name;
        public int Quantity;
        public decimal Price;
        public ItemCategory Category;

        public Item(string name, int quantity, decimal price, ItemCategory category)
        {
            Name = name;
            Quantity = quantity;
            Price = price;
            Category = category;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Qty: {Quantity}, Price: {Price}, Cat: {Category}";
        }
    }

    public class Warehouse
    {
        private List<Item> items = new List<Item>();

        public void AddItem(Item item)
        {
            items.Add(item);
        }

        public void RemoveItem(string name)
        {
            items.RemoveAll(i => i.Name == name);
        }

        public void UpdateItem(string name, int newQuantity, decimal newPrice)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                {
                    Item updated = items[i];
                    updated.Quantity = newQuantity;
                    updated.Price = newPrice;
                    items[i] = updated;
                    return;
                }
            }
        }

        public void PrintInventory()
        {
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }
    }

    public class Task1Runner
    {
        public static void Run()
        {
            Warehouse wh = new Warehouse();
            wh.AddItem(new Item("Laptop", 10, 999.99m, ItemCategory.Electronics));
            wh.AddItem(new Item("Chair", 50, 49.99m, ItemCategory.Furniture));
            wh.AddItem(new Item("Apple", 100, 0.50m, ItemCategory.Food));

            Console.WriteLine("Initial Inventory:");
            wh.PrintInventory();

            wh.UpdateItem("Laptop", 8, 950.00m);
            wh.RemoveItem("Apple");

            Console.WriteLine("\nUpdated Inventory:");
            wh.PrintInventory();
        }
    }
}