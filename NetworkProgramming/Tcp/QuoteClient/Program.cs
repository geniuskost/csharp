using System.Net.Sockets;
using System.Text;

const string serverHost = "127.0.0.1";
const int serverPort = 9000;

try
{
    using var client = new TcpClient();
    await client.ConnectAsync(serverHost, serverPort);
    Console.WriteLine($"Connected to quote server {serverHost}:{serverPort}");
    Console.WriteLine("Press Enter to request quote, type 'exit' to disconnect.");

    await using var networkStream = client.GetStream();
    using var reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);
    await using var writer = new StreamWriter(networkStream, Encoding.UTF8, leaveOpen: true)
    {
        AutoFlush = true
    };

    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine();

        if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        {
            await writer.WriteLineAsync("EXIT");
            var bye = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(bye))
            {
                Console.WriteLine($"Server: {bye}");
            }
            break;
        }

        await writer.WriteLineAsync("GET_QUOTE");
        var quote = await reader.ReadLineAsync();

        if (quote is null)
        {
            Console.WriteLine("Server closed the connection.");
            break;
        }

        Console.WriteLine($"Quote: {quote}");
    }
}
catch (SocketException ex)
{
    Console.WriteLine($"Could not connect to server: {ex.Message}");
}
