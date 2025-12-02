using System;

namespace ClassesStructures2
{
    public struct RGBColor
    {
        public byte R;
        public byte G;
        public byte B;

        public RGBColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public string ToHex()
        {
            return $"#{R:X2}{G:X2}{B:X2}";
        }

        public void ToHSL(out double h, out double s, out double l)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            l = (max + min) / 2.0;

            if (max == min)
            {
                h = 0;
                s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (max == r)
                {
                    h = (g - b) / d + (g < b ? 6 : 0);
                }
                else if (max == g)
                {
                    h = (b - r) / d + 2;
                }
                else
                {
                    h = (r - g) / d + 4;
                }
                h /= 6.0;
            }
            
            h *= 360; 
        }

        public void ToCMYK(out double c, out double m, out double y, out double k)
        {
            double r = R / 255.0;
            double g = G / 255.0;
            double b = B / 255.0;

            k = 1.0 - Math.Max(r, Math.Max(g, b));
            if (k == 1.0)
            {
                c = 0;
                m = 0;
                y = 0;
            }
            else
            {
                c = (1.0 - r - k) / (1.0 - k);
                m = (1.0 - g - k) / (1.0 - k);
                y = (1.0 - b - k) / (1.0 - k);
            }
        }
    }

    public class Task3Runner
    {
        public static void Run()
        {
            RGBColor color = new RGBColor(255, 0, 0); 
            Console.WriteLine($"Color RGB: {color.R}, {color.G}, {color.B}");
            
            Console.WriteLine($"HEX: {color.ToHex()}");

            color.ToHSL(out double h, out double s, out double l);
            Console.WriteLine($"HSL: H={h:F2}, S={s:F2}, L={l:F2}");

            color.ToCMYK(out double c, out double m, out double y, out double k);
            Console.WriteLine($"CMYK: C={c:F2}, M={m:F2}, Y={y:F2}, K={k:F2}");
        }
    }
}