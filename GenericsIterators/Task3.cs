using System;
using System.Collections;
using System.Collections.Generic;

namespace GenericsIterators
{
    public class Apartment : IEnumerable<string>
    {
        public int Number { get; set; }
        private List<string> residents = new List<string>();

        public Apartment(int number)
        {
            Number = number;
        }

        public void AddResident(string name)
        {
            residents.Add(name);
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var resident in residents)
            {
                yield return resident;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"Apartment #{Number}";
        }
    }

    public class House : IEnumerable<Apartment>
    {
        public string Address { get; set; }
        private List<Apartment> apartments = new List<Apartment>();

        public House(string address)
        {
            Address = address;
        }

        public void AddApartment(Apartment apt)
        {
            apartments.Add(apt);
        }

        public IEnumerator<Apartment> GetEnumerator()
        {
            for (int i = 0; i < apartments.Count; i++)
            {
                yield return apartments[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Task3Runner
    {
        public static void Run()
        {
            House house = new House("123 Main St");

            Apartment apt1 = new Apartment(101);
            apt1.AddResident("John");
            apt1.AddResident("Mary");

            Apartment apt2 = new Apartment(102);
            apt2.AddResident("Alice");
            apt2.AddResident("Bob");
            apt2.AddResident("Charlie");

            house.AddApartment(apt1);
            house.AddApartment(apt2);

            Console.WriteLine($"House at {house.Address}:");
            foreach (var apt in house)
            {
                Console.WriteLine($"  {apt}:");
                foreach (var resident in apt)
                {
                    Console.WriteLine($"    - {resident}");
                }
            }
        }
    }
}