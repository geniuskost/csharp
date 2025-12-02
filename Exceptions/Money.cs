using System;

namespace MoneyApp
{
    public class BankruptException : ApplicationException
    {
        public BankruptException(string message) : base(message) { }
    }

    public class Money
    {
        private long totalKopiykas;

        public int Hryvnias => (int)(totalKopiykas / 100);
        public int Kopiykas => (int)(totalKopiykas % 100);

        public Money(int hryvnias, int kopiykas)
        {
            if (hryvnias < 0 || kopiykas < 0)
                throw new BankruptException("Amount cannot be negative.");
            
            totalKopiykas = (long)hryvnias * 100 + kopiykas;
        }

        private Money(long totalKopiykas)
        {
            if (totalKopiykas < 0)
                throw new BankruptException("Bankrupt! Amount cannot be negative.");
            this.totalKopiykas = totalKopiykas;
        }

        public static Money operator +(Money m1, Money m2)
        {
            return new Money(m1.totalKopiykas + m2.totalKopiykas);
        }

        public static Money operator -(Money m1, Money m2)
        {
            return new Money(m1.totalKopiykas - m2.totalKopiykas);
        }

        public static Money operator *(Money m, int multiplier)
        {
            return new Money(m.totalKopiykas * multiplier);
        }

        public static Money operator /(Money m, int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            return new Money(m.totalKopiykas / divisor);
        }

        public static Money operator ++(Money m)
        {
            return new Money(m.totalKopiykas + 1);
        }

        public static Money operator --(Money m)
        {
            return new Money(m.totalKopiykas - 1);
        }

        public static bool operator <(Money m1, Money m2)
        {
            return m1.totalKopiykas < m2.totalKopiykas;
        }

        public static bool operator >(Money m1, Money m2)
        {
            return m1.totalKopiykas > m2.totalKopiykas;
        }

        public static bool operator ==(Money m1, Money m2)
        {
            if (ReferenceEquals(m1, null) && ReferenceEquals(m2, null)) return true;
            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null)) return false;
            return m1.totalKopiykas == m2.totalKopiykas;
        }

        public static bool operator !=(Money m1, Money m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Money m)
            {
                return this.totalKopiykas == m.totalKopiykas;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return totalKopiykas.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Hryvnias} UAH {Kopiykas:D2} kopiykas";
        }
    }
}