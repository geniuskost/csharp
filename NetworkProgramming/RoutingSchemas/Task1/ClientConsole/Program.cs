using RoutingSchemas.Shared;

namespace RoutingSchemas.Task1.ClientConsole;

internal static class Program
{
    private const int Port = 5055;

    private static async Task Main()
    {
        Console.Write("Server host (default 127.0.0.1): ");
        string? hostInput = Console.ReadLine();
        string host = string.IsNullOrWhiteSpace(hostInput) ? "127.0.0.1" : hostInput.Trim();

        Console.Write($"Server port (default {Port}): ");
        string? portInput = Console.ReadLine();
        int port = int.TryParse(portInput, out int parsedPort) ? parsedPort : Port;

        Console.Write("Your name (default Employee): ");
        string? nameInput = Console.ReadLine();
        string clientName = string.IsNullOrWhiteSpace(nameInput) ? "Employee" : nameInput.Trim();

        Console.WriteLine("Subscriptions: news, reminder, entertainment. Separate with commas; leave empty for all.");
        Console.Write("Your subscriptions: ");
        string? subscriptionInput = Console.ReadLine();
        IReadOnlyCollection<CompanyMessageKind> subscriptions = CompanyMessageProtocol.ParseSubscriptions(subscriptionInput ?? string.Empty);

        await using var client = new CompanyMessageClient();
        client.Log += message => Console.WriteLine(message);
        client.MessageReceived += packet => Console.WriteLine(CompanyMessageProtocol.FormatForDisplay(packet));
        client.ConnectionStateChanged += connected => Console.WriteLine(connected ? "Waiting for messages..." : "Client disconnected.");

        try
        {
            await client.ConnectAsync(host, port, clientName, subscriptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }

        Console.WriteLine("Type /exit to close the client.");

        while (true)
        {
            string? input = await Console.In.ReadLineAsync();
            if (input is null || input.Trim().Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        await client.DisconnectAsync();
    }
}