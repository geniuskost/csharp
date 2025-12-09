using System;

namespace DesignPatterns.Behavioral
{
    public interface IRouteStrategy
    {
        void BuildRoute(string a, string b);
    }

    public class WalkingStrategy : IRouteStrategy
    {
        public void BuildRoute(string a, string b)
        {
            Console.WriteLine($"Маршрут пішки від {a} до {b}: через парки (30 хв).");
        }
    }

    public class DrivingStrategy : IRouteStrategy
    {
        public void BuildRoute(string a, string b)
        {
            Console.WriteLine($"Маршрут авто від {a} до {b}: по шосе (10 хв).");
        }
    }

    public class Navigator
    {
        private IRouteStrategy _strategy;

        public void SetStrategy(IRouteStrategy strategy)
        {
            _strategy = strategy;
        }

        public void BuildRoute(string start, string end)
        {
            if (_strategy == null)
            {
                Console.WriteLine("Стратегію не обрано!");
                return;
            }
            
            _strategy.BuildRoute(start, end);
        }
    }

    class Program
    {
        static void Main()
        {
            Navigator navigator = new Navigator();
            string start = "Дім";
            string end = "Робота";

            // Користувач вибрав "Авто"
            navigator.SetStrategy(new DrivingStrategy());
            navigator.BuildRoute(start, end);

            navigator.SetStrategy(new WalkingStrategy());
            navigator.BuildRoute(start, end);
        }
    }
}