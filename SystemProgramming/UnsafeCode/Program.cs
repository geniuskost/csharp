namespace UnsafeCode;

internal static class Program
{
	private static void Main()
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;

		Console.WriteLine("Завдання 1");
		InfoMessenger.ShowMessage("DLL-модуль успішно підключено до проєкту.");

		Console.WriteLine();
		Console.WriteLine("Завдання 2");

		var number = 7;
		var factorial = NumberTools.Factorial(number);

		Console.WriteLine($"{number}! = {factorial}");
		Console.WriteLine($"{number} просте: {NumberTools.IsPrime(number)}");
		Console.WriteLine($"{number} парне: {NumberTools.IsEven(number)}");
		Console.WriteLine($"{number} непарне: {NumberTools.IsOdd(number)}");

		Console.WriteLine();
		Console.WriteLine("Завдання 3");

		var firstFraction = new Fraction(3, 4);
		var secondFraction = new Fraction(5, 6);

		Console.WriteLine($"Перший дріб: {firstFraction}");
		Console.WriteLine($"Другий дріб: {secondFraction}");
		Console.WriteLine($"Сума: {firstFraction.Add(secondFraction)}");
		Console.WriteLine($"Різниця: {firstFraction.Subtract(secondFraction)}");
		Console.WriteLine($"Добуток: {firstFraction.Multiply(secondFraction)}");
		Console.WriteLine($"Частка: {firstFraction.Divide(secondFraction)}");
		Console.WriteLine($"Скорочений дріб 18/24: {new Fraction(18, 24).Simplify()}");
	}
}
