using System;

namespace DesignPatterns.Creational
{
    public interface ITransport
    {
        void Deliver();
    }

    public class Truck : ITransport
    {
        public void Deliver()
        {
            Console.WriteLine("Вантажівка: доставка коробками по дорозі.");
        }
    }

    public class Ship : ITransport
    {
        public void Deliver()
        {
            Console.WriteLine("Корабель: доставка контейнерами по морю.");
        }
    }

    public abstract class Logistics
    {
        public abstract ITransport CreateTransport();

        public void PlanDelivery()
        {
            var transport = CreateTransport();
            Console.Write("Логістика: план створено -> ");
            transport.Deliver();
        }
    }

    public class RoadLogistics : Logistics
    {
        public override ITransport CreateTransport()
        {
            return new Truck();
        }
    }

    public class SeaLogistics : Logistics
    {
        public override ITransport CreateTransport()
        {
            return new Ship();
        }
    }

    class Program
    {
        static void Main()
        {
            Logistics logistics = new RoadLogistics();
            logistics.PlanDelivery();

            logistics = new SeaLogistics();
            logistics.PlanDelivery();
        }
    }
}