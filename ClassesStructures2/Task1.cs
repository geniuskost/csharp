using System;

namespace ClassesStructures2
{
    public struct Vector3D
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3D Multiply(double scalar)
        {
            return new Vector3D(X * scalar, Y * scalar, Z * scalar);
        }

        public Vector3D Add(Vector3D other)
        {
            return new Vector3D(X + other.X, Y + other.Y, Z + other.Z);
        }

        public Vector3D Subtract(Vector3D other)
        {
            return new Vector3D(X - other.X, Y - other.Y, Z - other.Z);
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }

    public class Task1Runner
    {
        public static void Run()
        {
            Vector3D v1 = new Vector3D(1, 2, 3);
            Vector3D v2 = new Vector3D(4, 5, 6);

            Console.WriteLine($"v1: {v1}");
            Console.WriteLine($"v2: {v2}");

            Console.WriteLine($"v1 * 2: {v1.Multiply(2)}");
            Console.WriteLine($"v1 + v2: {v1.Add(v2)}");
            Console.WriteLine($"v2 - v1: {v2.Subtract(v1)}");
        }
    }
}