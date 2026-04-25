using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatServer;

internal static class Program
{
    private const int Port = 5050;

    private static readonly ChatStore Store = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static async Task Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        var listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();

        Console.WriteLine($"Chat server started on 127.0.0.1:{Port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        string? username = null;

        try
        {
            await using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            string? firstLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                await WriteResponseAsync(writer, false, "Empty request.");
                return;
            }

            ChatRequest? connectRequest = JsonSerializer.Deserialize<ChatRequest>(firstLine, JsonOptions);

            if (connectRequest is null || !string.Equals(connectRequest.Command, "connect", StringComparison.OrdinalIgnoreCase))
            {
                await WriteResponseAsync(writer, false, "First request must be 'connect'.");
                return;
            }

            username = (connectRequest.Username ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                await WriteResponseAsync(writer, false, "Username is required.");
                return;
            }

            if (!Store.TryJoin(username))
            {
                await WriteResponseAsync(writer, false, "Username is already connected.");
                return;
            }

            Store.AddMessage(username, "joined the chat");
            Console.WriteLine($"[{username}] connected.");

            await WriteResponseAsync(writer, true, "Connected.");

            while (true)
            {
                string? line = await reader.ReadLineAsync();

                if (line is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    await WriteResponseAsync(writer, false, "Empty request.");
                    continue;
                }

                ChatRequest? request = JsonSerializer.Deserialize<ChatRequest>(line, JsonOptions);

                if (request is null)
                {
                    await WriteResponseAsync(writer, false, "Invalid request.");
                    continue;
                }

                if (string.Equals(request.Command, "send", StringComparison.OrdinalIgnoreCase))
                {
                    string messageText = (request.Message ?? string.Empty).Trim();

                    if (string.IsNullOrWhiteSpace(messageText))
                    {
                        await WriteResponseAsync(writer, false, "Message cannot be empty.");
                        continue;
                    }

                    Store.AddMessage(username, messageText);
                    await WriteResponseAsync(writer, true, "Message saved.");
                }
                else if (string.Equals(request.Command, "refresh", StringComparison.OrdinalIgnoreCase))
                {
                    int fromIdExclusive = request.LastMessageId;
                    IReadOnlyList<ChatMessage> messages = Store.GetMessagesForUser(username, fromIdExclusive);

                    var response = new ChatResponse(
                        Success: true,
                        Message: "Messages fetched.",
                        Messages: messages);

                    string responseJson = JsonSerializer.Serialize(response, JsonOptions);
                    await writer.WriteLineAsync(responseJson);
                }
                else
                {
                    await WriteResponseAsync(writer, false, "Unknown command.");
                }
            }
        }
        catch (IOException)
        {
            // Client disconnected abruptly.
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected server error: {ex.Message}");
        }
        finally
        {
            client.Close();

            if (!string.IsNullOrWhiteSpace(username))
            {
                Store.Leave(username);
                Store.AddMessage(username, "left the chat");
                Console.WriteLine($"[{username}] disconnected.");
            }
        }
    }

    private static async Task WriteResponseAsync(StreamWriter writer, bool success, string message)
    {
        var response = new ChatResponse(success, message, Array.Empty<ChatMessage>());
        string json = JsonSerializer.Serialize(response, JsonOptions);
        await writer.WriteLineAsync(json);
    }
}

internal sealed class ChatStore
{
    private readonly object _lock = new();
    private readonly List<ChatMessage> _messages = new();
    private readonly ConcurrentDictionary<string, byte> _connectedUsers = new();
    private int _nextId = 1;

    public bool TryJoin(string username) => _connectedUsers.TryAdd(username, 0);

    public void Leave(string username) => _connectedUsers.TryRemove(username, out _);

    public void AddMessage(string username, string message)
    {
        lock (_lock)
        {
            _messages.Add(new ChatMessage(_nextId++, username, message));
        }
    }

    public IReadOnlyList<ChatMessage> GetMessagesForUser(string username, int fromIdExclusive)
    {
        lock (_lock)
        {
            return _messages
                .Where(m => m.Id > fromIdExclusive)
                .Where(m => !string.Equals(m.Username, username, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}

internal sealed record ChatRequest(string Command, string? Username, string? Message, int LastMessageId);

internal sealed record ChatResponse(bool Success, string Message, IReadOnlyList<ChatMessage> Messages);

internal sealed record ChatMessage(int Id, string Username, string Message);