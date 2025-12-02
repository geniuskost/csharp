using System;
using System.Linq;

namespace ClassesStructures3
{
    public class TemperatureArray
    {
        private double[] temperatures = new double[7];

        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= 7)
                    throw new IndexOutOfRangeException("Index must be between 0 and 6");
                return temperatures[index];
            }
            set
            {
                if (index < 0 || index >= 7)
                    throw new IndexOutOfRangeException("Index must be between 0 and 6");
                temperatures[index] = value;
            }
        }

        public double CalculateAverage()
        {
            return temperatures.Average();
        }
    }

    public class Task1Runner
    {
        public static void Run()
        {
            TemperatureArray weekTemps = new TemperatureArray();
            
            weekTemps[0] = 20.5;
            weekTemps[1] = 22.0;
            weekTemps[2] = 19.5;
            weekTemps[3] = 21.0;
            weekTemps[4] = 23.5;
            weekTemps[5] = 25.0;
            weekTemps[6] = 24.0;

            for (int i = 0; i < 7; i++)
            {
                Console.WriteLine($"Day {i}: {weekTemps[i]}");
            }

            Console.WriteLine($"Average temperature: {weekTemps.CalculateAverage():F2}");
        }
    }
}