using System;
using System.Collections.Generic;

namespace Polimorfism
{
    class Program
    {
        static void Main(string[] args)
        {
            List<MusicalInstrument> orchestra = new List<MusicalInstrument>
            {
                new Violin(),
                new Trombone(),
                new Ukulele(),
                new Cello()
            };

            foreach (var instrument in orchestra)
            {
                instrument.Show();
                instrument.Desc();
                instrument.History();
                instrument.Sound();
                Console.WriteLine("--------------------------------------------------");
            }
        }
    }
}