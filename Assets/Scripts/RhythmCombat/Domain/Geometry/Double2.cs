using System;

namespace RhythmCombat.Domain.Geometry
{
    public struct Double2
    {
        public Double2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static Double2 Lerp(Double2 from, Double2 to, double progress)
        {
            return Add(from, Multiply(Subtract(to, from), progress));
        }

        public static double Distance(Double2 a, Double2 b)
        {
            var delta = Subtract(a, b);
            return Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }

        public static Double2 Add(Double2 a, Double2 b)
        {
            return new Double2(a.X + b.X, a.Y + b.Y);
        }

        public static Double2 Subtract(Double2 a, Double2 b)
        {
            return new Double2(a.X - b.X, a.Y - b.Y);
        }

        public static Double2 Multiply(Double2 value, double multiplier)
        {
            return new Double2(value.X * multiplier, value.Y * multiplier);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}
