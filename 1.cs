
static void Main()
{
    for (int i = 0; i < 5; i++)
    {
        Console.WriteLine(new string(' ', i * 2) + "Hello, World!");
    }

    Console.WriteLine();

    PrintRow("Іван", "Іванович", "Петренко");
    PrintRow("Олена", "Михайлівна", "Сидоренко");
    PrintRow("Павло", "Олегович", "Ковальчук");
    PrintRow("Марія", "Андріївна", "Шевченко");
}

	static void PrintRow(string first, string patronymic, string surname)
	{
		Console.WriteLine(first.PadRight(12) + patronymic.PadRight(16) + surname.PadRight(16));
	}