using System;

namespace Polimorfism
{
    public class MusicalInstrument
    {
        protected string Name;
        protected string Description;
        protected string HistoryText;

        public MusicalInstrument(string name, string description, string historyText)
        {
            Name = name;
            Description = description;
            HistoryText = historyText;
        }

        public virtual void Sound()
        {
            Console.WriteLine("Playing generic sound...");
        }

        public void Show()
        {
            Console.WriteLine($"Instrument: {Name}");
        }

        public void Desc()
        {
            Console.WriteLine($"Description: {Description}");
        }

        public void History()
        {
            Console.WriteLine($"History: {HistoryText}");
        }
    }

    public class Violin : MusicalInstrument
    {
        public Violin() : base("Violin", "A wooden string instrument in the violin family.", "The violin was first known in 16th-century Italy.")
        {
        }

        public override void Sound()
        {
            Console.WriteLine("*Violin screeches*");
            try
            {
                Console.Beep(659, 1000); 
            }
            catch { }
        }
    }

    public class Trombone : MusicalInstrument
    {
        public Trombone() : base("Trombone", "A musical instrument in the brass family.", "The trombone developed from the trumpet in the 15th century.")
        {
        }

        public override void Sound()
        {
            Console.WriteLine("*Trombone blasts*");
            try
            {
                Console.Beep(110, 1000);
            }
            catch { }
        }
    }

    public class Ukulele : MusicalInstrument
    {
        public Ukulele() : base("Ukulele", "A member of the lute family of instruments.", "Introduced to Hawaii by Portuguese immigrants in the late 19th century.")
        {
        }

        public override void Sound()
        {
            Console.WriteLine("*Ukulele strums*");
            try
            {
                Console.Beep(440, 1000);
            }
            catch { }
        }
    }

    public class Cello : MusicalInstrument
    {
        public Cello() : base("Cello", "A bowed string instrument of the violin family.", "The cello name is derived from the ending of the Italian violoncello.")
        {
        }

        public override void Sound()
        {
            Console.WriteLine("*Cello hums*");
            try
            {
                Console.Beep(130, 1000);
            }
            catch { }
        }
    }
}