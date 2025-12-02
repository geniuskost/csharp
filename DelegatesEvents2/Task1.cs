using System;

namespace DelegatesEvents2
{
    public class Task1Runner
    {
        public static void Run()
        {
            Action showTime = () => Console.WriteLine($"Current Time: {DateTime.Now.ToShortTimeString()}");
            Action showDate = () => Console.WriteLine($"Current Date: {DateTime.Now.ToShortDateString()}");
            Action showDay = () => Console.WriteLine($"Current Day: {DateTime.Now.DayOfWeek}");

            Func<double, double, double> triangleArea = (b, h) => 0.5 * b * h;
            Func<double, double, double> rectangleArea = (w, h) => w * h;

            Predicate<DayOfWeek> isWeekend = d => d == DayOfWeek.Saturday || d == DayOfWeek.Sunday;

            Console.WriteLine("--- Action Delegates ---");
            showTime();
            showDate();
            showDay();

            Console.WriteLine("\n--- Predicate Delegate ---");
            bool weekend = isWeekend(DateTime.Now.DayOfWeek);
            Console.WriteLine($"Is today a weekend? {weekend}");

            Console.WriteLine("\n--- Func Delegates ---");
            double tBase = 10, tHeight = 5;
            Console.WriteLine($"Triangle Area (b={tBase}, h={tHeight}): {triangleArea(tBase, tHeight)}");

            double rWidth = 4, rHeight = 6;
            Console.WriteLine($"Rectangle Area (w={rWidth}, h={rHeight}): {rectangleArea(rWidth, rHeight)}");
        }
    }
}