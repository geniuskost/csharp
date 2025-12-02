using System;

namespace Interfaces
{
    public interface IRemoteControl
    {
        void TurnOn();
        void TurnOff();
        void SetChannel(int channel);
    }

    public class TvRemoteControl : IRemoteControl
    {
        public void TurnOn()
        {
            Console.WriteLine("TV is now ON.");
        }

        public void TurnOff()
        {
            Console.WriteLine("TV is now OFF.");
        }

        public void SetChannel(int channel)
        {
            Console.WriteLine($"TV channel set to {channel}.");
        }
    }

    public class RadioRemoteControl : IRemoteControl
    {
        public void TurnOn()
        {
            Console.WriteLine("Radio is now ON.");
        }

        public void TurnOff()
        {
            Console.WriteLine("Radio is now OFF.");
        }

        public void SetChannel(int channel)
        {
            Console.WriteLine($"Radio frequency set to {channel} MHz.");
        }
    }

    public class Task1Runner
    {
        public static void Run()
        {
            IRemoteControl tvRemote = new TvRemoteControl();
            tvRemote.TurnOn();
            tvRemote.SetChannel(5);
            tvRemote.TurnOff();

            Console.WriteLine();

            IRemoteControl radioRemote = new RadioRemoteControl();
            radioRemote.TurnOn();
            radioRemote.SetChannel(101);
            radioRemote.TurnOff();
        }
    }
}