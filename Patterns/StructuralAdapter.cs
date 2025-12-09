using System;

namespace DesignPatterns.Structural
{
    public interface ITarget
    {
        string GetRequest();
    }

    public class LegacyService
    {
        public string GetSpecificRequest()
        {
            return "Специфічні_Дані_З_Старої_Системи";
        }
    }

    public class Adapter : ITarget
    {
        private readonly LegacyService _adaptee;

        public Adapter(LegacyService adaptee)
        {
            _adaptee = adaptee;
        }

        public string GetRequest()
        {
            string original = _adaptee.GetSpecificRequest();
            return $"Адаптер перетворив: '{original}'";
        }
    }

    class Client
    {
        static void Main()
        {
            LegacyService legacy = new LegacyService();

            ITarget target = new Adapter(legacy);

            Console.WriteLine(target.GetRequest());
        }
    }
}