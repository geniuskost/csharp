using System;

namespace RealLifeObjects
{
    public class Car
    {
        private string brand;
        private string model;
        private int year;
        private string color;
        private double fuelLevel;
        private bool isEngineOn;

        public Car(string brand, string model, int year, string color)
        {
            this.brand = brand;
            this.model = model;
            this.year = year;
            this.color = color;
            this.fuelLevel = 50.0;
            this.isEngineOn = false;
        }

        // Властивості (Properties)
        public string Brand 
        { 
            get { return brand; } 
            set { brand = value; }
        }

        public string Model 
        { 
            get { return model; } 
            set { model = value; }
        }

        public int Year 
        { 
            get { return year; } 
            set { year = value; }
        }

        public string Color 
        { 
            get { return color; } 
            set { color = value; }
        }

        public double FuelLevel 
        { 
            get { return fuelLevel; } 
        }

        public bool IsEngineOn 
        { 
            get { return isEngineOn; } 
        }

        public void StartEngine()
        {
            if (!isEngineOn && fuelLevel > 0)
            {
                isEngineOn = true;
                Console.WriteLine($"{brand} {model}: Двигун запущено!");
            }
            else if (fuelLevel == 0)
            {
                Console.WriteLine($"{brand} {model}: Неможливо запустити - немає палива!");
            }
            else
            {
                Console.WriteLine($"{brand} {model}: Двигун вже працює!");
            }
        }

        public void StopEngine()
        {
            if (isEngineOn)
            {
                isEngineOn = false;
                Console.WriteLine($"{brand} {model}: Двигун зупинено!");
            }
            else
            {
                Console.WriteLine($"{brand} {model}: Двигун вже вимкнено!");
            }
        }

        public void Drive(double distance)
        {
            if (!isEngineOn)
            {
                Console.WriteLine($"{brand} {model}: Спочатку запустіть двигун!");
                return;
            }

            double fuelNeeded = distance * 0.08;
            if (fuelLevel >= fuelNeeded)
            {
                fuelLevel -= fuelNeeded;
                Console.WriteLine($"{brand} {model}: Проїхано {distance} км. Залишок палива: {fuelLevel:F2}%");
            }
            else
            {
                Console.WriteLine($"{brand} {model}: Недостатньо палива для поїздки!");
            }
        }

        public void Refuel(double amount)
        {
            if (fuelLevel + amount <= 100)
            {
                fuelLevel += amount;
                Console.WriteLine($"{brand} {model}: Заправлено! Рівень палива: {fuelLevel:F2}%");
            }
            else
            {
                fuelLevel = 100;
                Console.WriteLine($"{brand} {model}: Бак повний! Рівень палива: 100%");
            }
        }

        public void DisplayInfo()
        {
            Console.WriteLine("\n========== Інформація про автомобіль ==========");
            Console.WriteLine($"Марка: {brand}");
            Console.WriteLine($"Модель: {model}");
            Console.WriteLine($"Рік випуску: {year}");
            Console.WriteLine($"Колір: {color}");
            Console.WriteLine($"Рівень палива: {fuelLevel:F2}%");
            Console.WriteLine($"Стан двигуна: {(isEngineOn ? "Запущено" : "Вимкнено")}");
            Console.WriteLine("===============================================\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Car myCar = new Car("Toyota", "Camry", 2022, "Сірий");

            myCar.DisplayInfo();

            myCar.StartEngine();
            myCar.Drive(100);
            myCar.Drive(200);
            
            myCar.Refuel(30);
            myCar.Drive(150);
            
            myCar.DisplayInfo();
            
            myCar.StopEngine();

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }
    }
}
