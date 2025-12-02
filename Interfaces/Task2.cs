using System;
using System.Linq;

namespace Interfaces
{
    public interface IValidator
    {
        bool Validate();
    }

    public class EmailValidator : IValidator
    {
        private string _email;

        public EmailValidator(string email)
        {
            _email = email;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_email)) return false;
            return _email.Contains("@") && _email.Contains(".");
        }
    }

    public class PasswordValidator : IValidator
    {
        private string _password;

        public PasswordValidator(string password)
        {
            _password = password;
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_password)) return false;
            // Criteria: Length >= 8 and contains at least one digit
            return _password.Length >= 8 && _password.Any(char.IsDigit);
        }
    }

    public class Task2Runner
    {
        public static void Run()
        {
            Console.WriteLine("Enter email to validate:");
            string email = Console.ReadLine();
            IValidator emailValidator = new EmailValidator(email);
            Console.WriteLine($"Email valid: {emailValidator.Validate()}");

            Console.WriteLine("\nEnter password to validate (min 8 chars, 1 digit):");
            string password = Console.ReadLine();
            IValidator passwordValidator = new PasswordValidator(password);
            Console.WriteLine($"Password valid: {passwordValidator.Validate()}");
        }
    }
}