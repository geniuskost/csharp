using System;

namespace ClassesStructures3
{
    public class Product
    {
        public string Name { get; set; }
        private int quantity;
        private decimal price;

        public int Quantity
        {
            get { return quantity; }
            set
            {
                if (value < 0)
                    quantity = 0;
                else
                    quantity = value;
            }
        }

        public decimal Price
        {
            get { return price; }
            set
            {
                if (value < 0)
                    price = 0;
                else
                    price = value;
            }
        }

        public Product(string name, int quantity, decimal price)
        {
            Name = name;
            Quantity = quantity;
            Price = price;
        }

        public static Product operator +(Product p, int amount)
        {
            return new Product(p.Name, p.Quantity + amount, p.Price);
        }

        public static Product operator -(Product p, int amount)
        {
            return new Product(p.Name, p.Quantity - amount, p.Price);
        }

        public static bool operator ==(Product p1, Product p2)
        {
            if (ReferenceEquals(p1, null) && ReferenceEquals(p2, null)) return true;
            if (ReferenceEquals(p1, null) || ReferenceEquals(p2, null)) return false;
            return p1.Price == p2.Price;
        }

        public static bool operator !=(Product p1, Product p2)
        {
            return !(p1 == p2);
        }

        public static bool operator >(Product p1, Product p2)
        {
            return p1.Quantity > p2.Quantity;
        }

        public static bool operator <(Product p1, Product p2)
        {
            return p1.Quantity < p2.Quantity;
        }

        public override bool Equals(object obj)
        {
            if (obj is Product p)
            {
                return this.Price == p.Price;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }

        public override string ToString()
        {
            return $"Product: {Name}, Qty: {Quantity}, Price: {Price}";
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            Product p1 = new Product("Laptop", 10, 1000m);
            Product p2 = new Product("Phone", 20, 1000m);

            Console.WriteLine($"P1: {p1}");
            Console.WriteLine($"P2: {p2}");

            p1 = p1 + 5;
            Console.WriteLine($"P1 + 5: {p1}");

            p2 = p2 - 5;
            Console.WriteLine($"P2 - 5: {p2}");

            Console.WriteLine($"P1 == P2 (by price): {p1 == p2}");
            Console.WriteLine($"P1 > P2 (by quantity): {p1 > p2}");
            Console.WriteLine($"P1 < P2 (by quantity): {p1 < p2}");
        }
    }
}