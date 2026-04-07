namespace UnsafeCode;

public sealed class Fraction
{
    public long Numerator { get; }

    public long Denominator { get; }

    public Fraction(long numerator, long denominator)
    {
        if (denominator == 0)
        {
            throw new ArgumentException("Знаменник не може бути нульовим.", nameof(denominator));
        }

        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        var divisor = GreatestCommonDivisor(Math.Abs(numerator), denominator);
        Numerator = numerator / divisor;
        Denominator = denominator / divisor;
    }

    public Fraction Add(Fraction other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new Fraction(
            Numerator * other.Denominator + other.Numerator * Denominator,
            Denominator * other.Denominator);
    }

    public Fraction Subtract(Fraction other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new Fraction(
            Numerator * other.Denominator - other.Numerator * Denominator,
            Denominator * other.Denominator);
    }

    public Fraction Multiply(Fraction other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new Fraction(
            Numerator * other.Numerator,
            Denominator * other.Denominator);
    }

    public Fraction Divide(Fraction other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Numerator == 0)
        {
            throw new DivideByZeroException("Не можна ділити на нульовий дріб.");
        }

        return new Fraction(
            Numerator * other.Denominator,
            Denominator * other.Numerator);
    }

    public Fraction Simplify()
    {
        return new Fraction(Numerator, Denominator);
    }

    public override string ToString()
    {
        return Denominator == 1 ? Numerator.ToString() : $"{Numerator}/{Denominator}";
    }

    private static long GreatestCommonDivisor(long left, long right)
    {
        while (right != 0)
        {
            (left, right) = (right, left % right);
        }

        return left == 0 ? 1 : left;
    }
}