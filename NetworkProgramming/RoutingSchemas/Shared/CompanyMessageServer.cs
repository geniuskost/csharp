using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RoutingSchemas.Shared;

internal sealed class CompanyMessageServer : IAsyncDisposable
{
    private sealed class ClientSession : IAsyncDisposable
    {
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        public ClientSession(
            string id,
            string displayName,
            string endPoint,
            TcpClient client,
            StreamWriter writer,
            IReadOnlyCollection<CompanyMessageKind> subscriptions)
        {
            Id = id;
            DisplayName = displayName;
            EndPoint = endPoint;
            Client = client;
            Writer = writer;
            Subscriptions = subscriptions;
        }

        public string Id { get; }

        public string DisplayName { get; private set; }

        public string EndPoint { get; }

        public TcpClient Client { get; }

        public IReadOnlyCollection<CompanyMessageKind> Subscriptions { get; private set; }

        private StreamWriter Writer { get; }

        public void Rename(string displayName)
        {
            DisplayName = displayName.Trim();
        }

        public void UpdateSubscriptions(IReadOnlyCollection<CompanyMessageKind> subscriptions)
        {
            Subscriptions = subscriptions;
        }

        public bool Accepts(CompanyMessageKind kind)
        {
            return kind == CompanyMessageKind.Emergency || Subscriptions.Contains(kind);
        }

        public async Task SendAsync(CompanyMessagePacket packet)
        {
            await _sendLock.WaitAsync().ConfigureAwait(false);

            try
            {
                string payload = CompanyMessageProtocol.Serialize(packet);
                await Writer.WriteLineAsync(payload).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public ValueTask DisposeAsync()
        {
            _sendLock.Dispose();
            Writer.Dispose();
            Client.Close();
            Client.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private readonly ConcurrentDictionary<string, ClientSession> _clients = new();
    private readonly IPAddress _address;
    private readonly int _port;
    private readonly string _serverName;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoopTask;

    public CompanyMessageServer(IPAddress address, int port, string serverName)
    {
        _address = address;
        _port = port;
        _serverName = string.IsNullOrWhiteSpace(serverName) ? "Server" : serverName.Trim();
    }

    public event Action<string>? Log;

    public event Action<IReadOnlyList<ConnectedClientInfo>>? ClientsChanged;

    public bool IsRunning => _listener is not null;

    public Task StartAsync()
    {
        if (_listener is not null)
        {
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(_address, _port);
        _listener.Start();

        Log?.Invoke($"Server started on {_address}:{_port}.");
        _acceptLoopTask = AcceptLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_listener is null && _cts is null)
        {
            return;
        }

        _cts?.Cancel();

        try
        {
            _listener?.Stop();
        }
        catch
        {
        }

        List<ClientSession> sessions = _clients.Values.ToList();
        foreach (ClientSession session in sessions)
        {
            await RemoveClientAsync(session.Id, $"Disconnected: {session.DisplayName} ({session.EndPoint}).").ConfigureAwait(false);
        }

        if (_acceptLoopTask is not null)
        {
            try
            {
                await _acceptLoopTask.ConfigureAwait(false);
            }
            catch
            {
            }
        }

        _acceptLoopTask = null;
        _listener = null;

        _cts?.Dispose();
        _cts = null;
    }

    public IReadOnlyList<ConnectedClientInfo> GetClientsSnapshot()
    {
        return _clients.Values
            .Select(client => new ConnectedClientInfo(client.Id, client.DisplayName, client.EndPoint, CompanyMessageProtocol.FormatSubscriptions(client.Subscriptions)))
            .OrderBy(client => client.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(client => client.EndPoint, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task BroadcastAsync(CompanyMessageKind kind, string text)
    {
        string message = text.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        CompanyMessagePacket packet = CompanyMessageProtocol.CreateMessage(kind, _serverName, message);
        List<ClientSession> sessions = _clients.Values.ToList();

        foreach (ClientSession session in sessions)
        {
            if (!session.Accepts(kind))
            {
                continue;
            }

            try
            {
                await session.SendAsync(packet).ConfigureAwait(false);
            }
            catch
            {
                await RemoveClientAsync(session.Id, $"Dropped {session.DisplayName} ({session.EndPoint}).").ConfigureAwait(false);
            }
        }

        Log?.Invoke(CompanyMessageProtocol.FormatForDisplay(packet));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        if (_listener is null)
        {
            return;
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                _ = HandleClientAsync(client);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Log?.Invoke("Accept loop encountered a socket error.");
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        string id = Guid.NewGuid().ToString("N");
        string endPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        string displayName = endPoint;
        IReadOnlyCollection<CompanyMessageKind> subscriptions = new[] { CompanyMessageKind.News, CompanyMessageKind.Reminder, CompanyMessageKind.Entertainment };

        using (client)
        {
            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
            {
                AutoFlush = true
            };

            ClientSession session = new(id, displayName, endPoint, client, writer, subscriptions);

            try
            {
                string? helloLine = await reader.ReadLineAsync().ConfigureAwait(false);
                CompanyMessagePacket? helloPacket = CompanyMessageProtocol.Deserialize(helloLine);

                if (helloPacket is not null && helloPacket.Kind == CompanyMessageKind.News && helloPacket.Text.StartsWith("HELLO:", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = helloPacket.Sender.Trim();
                    session.Rename(displayName);
                    subscriptions = CompanyMessageProtocol.ParseSubscriptions(helloPacket.Text[6..]);
                    session.UpdateSubscriptions(subscriptions);
                }

                _clients[id] = session;
                NotifyClientsChanged();
                Log?.Invoke($"Connected: {session.DisplayName} ({session.EndPoint}) -> [{CompanyMessageProtocol.FormatSubscriptions(session.Subscriptions)}].");

                while (true)
                {
                    string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line is null)
                    {
                        break;
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                await RemoveClientAsync(id, $"Disconnected: {displayName} ({endPoint}).").ConfigureAwait(false);
            }
        }
    }

    private async Task RemoveClientAsync(string id, string reason)
    {
        if (_clients.TryRemove(id, out ClientSession? session))
        {
            await session.DisposeAsync().ConfigureAwait(false);
            Log?.Invoke(reason);
            NotifyClientsChanged();
        }
    }

    private void NotifyClientsChanged()
    {
        ClientsChanged?.Invoke(GetClientsSnapshot());
    }
}
