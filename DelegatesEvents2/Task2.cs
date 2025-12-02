using System;

namespace DelegatesEvents2
{
    public class CreditCard
    {
        public string CardNumber { get; private set; }
        public string OwnerName { get; private set; }
        public DateTime ExpirationDate { get; private set; }
        private string pin;
        public decimal CreditLimit { get; private set; }
        public decimal Balance { get; private set; }
        public decimal TargetBalance { get; set; }

        public event Action<decimal> OnDeposit;
        public event Action<decimal> OnWithdraw;
        public event Action OnCreditUsageStart;
        public event Action<decimal> OnTargetBalanceReached;
        public event Action OnPinChanged;

        public CreditCard(string cardNumber, string ownerName, DateTime expirationDate, string pin, decimal creditLimit, decimal initialBalance)
        {
            CardNumber = cardNumber;
            OwnerName = ownerName;
            ExpirationDate = expirationDate;
            this.pin = pin;
            CreditLimit = creditLimit;
            Balance = initialBalance;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0) return;
            
            bool wasInCredit = Balance < 0;
            Balance += amount;
            OnDeposit?.Invoke(amount);

            if (TargetBalance > 0 && Balance >= TargetBalance)
            {
                OnTargetBalanceReached?.Invoke(Balance);
            }
        }

        public void Withdraw(decimal amount, string enteredPin)
        {
            if (enteredPin != pin)
            {
                Console.WriteLine("Incorrect PIN.");
                return;
            }

            if (amount <= 0) return;

            if (Balance - amount < -CreditLimit)
            {
                Console.WriteLine("Transaction declined. Credit limit exceeded.");
                return;
            }

            bool wasPositive = Balance >= 0;
            Balance -= amount;
            OnWithdraw?.Invoke(amount);

            if (wasPositive && Balance < 0)
            {
                OnCreditUsageStart?.Invoke();
            }
        }

        public void ChangePin(string oldPin, string newPin)
        {
            if (pin == oldPin)
            {
                pin = newPin;
                OnPinChanged?.Invoke();
            }
            else
            {
                Console.WriteLine("Incorrect old PIN.");
            }
        }

        public override string ToString()
        {
            return $"Card: {CardNumber}, Owner: {OwnerName}, Balance: {Balance}, Limit: {CreditLimit}";
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            CreditCard card = new CreditCard("1234-5678-9012-3456", "John Doe", DateTime.Now.AddYears(2), "1234", 1000m, 500m);
            card.TargetBalance = 2000m;

            card.OnDeposit += amount => Console.WriteLine($"[Event] Deposited: {amount}");
            card.OnWithdraw += amount => Console.WriteLine($"[Event] Withdrawn: {amount}");
            card.OnCreditUsageStart += () => Console.WriteLine($"[Event] WARNING: You have started using credit funds!");
            card.OnTargetBalanceReached += balance => Console.WriteLine($"[Event] Target balance reached! Current: {balance}");
            card.OnPinChanged += () => Console.WriteLine($"[Event] PIN code changed successfully.");

            Console.WriteLine(card);
            
            Console.WriteLine("\n--- Actions ---");
            card.Deposit(500); 
            card.Withdraw(200, "1234"); 
            card.Withdraw(1000, "1234"); 
            card.Deposit(1500); 
            card.ChangePin("1234", "4321");
            
            Console.WriteLine("\n" + card);
        }
    }
}