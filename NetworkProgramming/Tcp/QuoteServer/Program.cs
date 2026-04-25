using System.Net;
using System.Net.Sockets;
using System.Text;

const int port = 9000;
var ipAddress = IPAddress.Any;
var listener = new TcpListener(ipAddress, port);

var quotes = new[]
{
    "Success is not final, failure is not fatal: it is the courage to continue that counts. — Winston Churchill",
    "The best way to predict the future is to create it. — Peter Drucker",
    "The only way to do great work is to love what you do. — Steve Jobs",
    "What we think, we become. — Buddha",
    "Do one thing every day that scares you. — Eleanor Roosevelt",
    "In the middle of every difficulty lies opportunity. — Albert Einstein"
};

listener.Start();
Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Quote server started on port {port}.");

while (true)
{
    var client = await listener.AcceptTcpClientAsync();
    _ = Task.Run(() => HandleClientAsync(client, quotes));
}

static async Task HandleClientAsync(TcpClient client, string[] quotes)
{
    var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown-client";
    Log($"{endpoint} connected.");

    try
    {
        await using var networkStream = client.GetStream();
        using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);
        await using var writer = new StreamWriter(networkStream, Encoding.UTF8, leaveOpen: true)
        {
            AutoFlush = true
        };

        while (client.Connected)
        {
            var command = await reader.ReadLineAsync();
            if (command is null)
            {
                break;
            }

            if (command.Equals("GET_QUOTE", StringComparison.OrdinalIgnoreCase))
            {
                var quote = quotes[Random.Shared.Next(quotes.Length)];
                await writer.WriteLineAsync(quote);
                Log($"{endpoint} requested quote. Sent: \"{quote}\"");
                continue;
            }

            if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                await writer.WriteLineAsync("BYE");
                break;
            }

            await writer.WriteLineAsync("UNKNOWN_COMMAND");
        }
    }
    catch (IOException ex)
    {
        Log($"{endpoint} connection error: {ex.Message}");
    }
    finally
    {
        Log($"{endpoint} disconnected.");
        client.Close();
    }
}

static void Log(string message)
{
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
}
