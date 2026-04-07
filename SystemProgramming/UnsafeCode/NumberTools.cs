using System.Numerics;

namespace UnsafeCode;

public static class NumberTools
{
    public static BigInteger Factorial(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Факторіал визначений лише для невід'ємних чисел.");
        }

        BigInteger result = BigInteger.One;

        for (var index = 2; index <= value; index++)
        {
            result *= index;
        }

        return result;
    }

    public static bool IsPrime(int value)
    {
        if (value < 2)
        {
            return false;
        }

        if (value == 2)
        {
            return true;
        }

        if (value % 2 == 0)
        {
            return false;
        }

        var limit = (int)Math.Sqrt(value);

        for (var divisor = 3; divisor <= limit; divisor += 2)
        {
            if (value % divisor == 0)
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsEven(int value)
    {
        return value % 2 == 0;
    }

    public static bool IsOdd(int value)
    {
        return !IsEven(value);
    }
}