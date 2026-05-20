using System.Net;
using RoutingSchemas.Shared;

namespace RoutingSchemas.Task1.ServerConsole;

internal static class Program
{
    private const int Port = 5055;

    private static async Task Main()
    {
        await using var server = new CompanyMessageServer(IPAddress.Any, Port, "Admin");

        server.Log += message => Console.WriteLine(message);
        server.ClientsChanged += clients => Console.WriteLine($"Clients connected: {clients.Count}");

        await server.StartAsync();

        Console.WriteLine("Company message server is running.");
        Console.WriteLine("Type: news text, reminder text, entertainment text, emergency text.");
        Console.WriteLine("Commands: /clients, /exit");

        while (true)
        {
            string? input = await Console.In.ReadLineAsync();
            if (input is null)
            {
                break;
            }

            string command = input.Trim();
            if (string.IsNullOrWhiteSpace(command))
            {
                continue;
            }

            if (command.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (command.Equals("/clients", StringComparison.OrdinalIgnoreCase))
            {
                IReadOnlyList<ConnectedClientInfo> clients = server.GetClientsSnapshot();
                Console.WriteLine(clients.Count == 0 ? "No connected clients." : "Connected clients:");

                foreach (ConnectedClientInfo client in clients)
                {
                    Console.WriteLine($" - {client}");
                }

                continue;
            }

            if (TryParseMessageCommand(command, out CompanyMessageKind kind, out string text))
            {
                await server.BroadcastAsync(kind, text);
                continue;
            }

            await server.BroadcastAsync(CompanyMessageKind.News, command);
        }

        await server.StopAsync();
    }

    private static bool TryParseMessageCommand(string input, out CompanyMessageKind kind, out string text)
    {
        kind = CompanyMessageKind.News;
        text = input;

        string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        if (!CompanyMessageProtocol.TryParseKind(parts[0], out kind))
        {
            return false;
        }

        text = parts[1];
        return true;
    }
}