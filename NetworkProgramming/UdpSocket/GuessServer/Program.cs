using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GuessServer;

internal static class Program
{
    private const int Port = 5055;

    private static readonly HashSet<IPEndPoint> Players = new();

    private static async Task Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        int secretNumber = Random.Shared.Next(1, 101);
        Console.WriteLine("=== Хто перший вгадає ===");
        Console.WriteLine($"Сервер запущено на порту {Port}.");
        Console.WriteLine("Загадано число від 1 до 100.");

        using var udp = new UdpClient(Port);

        while (true)
        {
            UdpReceiveResult received = await udp.ReceiveAsync();
            string guessText = Encoding.UTF8.GetString(received.Buffer).Trim();
            IPEndPoint playerEndpoint = received.RemoteEndPoint;

            Players.Add(playerEndpoint);

            if (!int.TryParse(guessText, out int guess))
            {
                await SendTextAsync(udp, playerEndpoint, "Некоректне число.");
                Console.WriteLine($"[{playerEndpoint}] некоректна спроба: \"{guessText}\"");
                continue;
            }

            if (guess < secretNumber)
            {
                await SendTextAsync(udp, playerEndpoint, "Занадто мало.");
                Console.WriteLine($"[{playerEndpoint}] спроба {guess}: замало.");
                continue;
            }

            if (guess > secretNumber)
            {
                await SendTextAsync(udp, playerEndpoint, "Занадто багато.");
                Console.WriteLine($"[{playerEndpoint}] спроба {guess}: забагато.");
                continue;
            }

            await SendTextAsync(udp, playerEndpoint, "Вгадав!");
            string winMessage = $"Гравець із {playerEndpoint} переміг!";
            Console.WriteLine($"[{playerEndpoint}] вгадав число {secretNumber}. Завершення гри.");

            foreach (IPEndPoint endpoint in Players)
            {
                await SendTextAsync(udp, endpoint, winMessage);
            }

            break;
        }
    }

    private static async Task SendTextAsync(UdpClient udp, IPEndPoint endpoint, string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        await udp.SendAsync(data, data.Length, endpoint);
    }
}
