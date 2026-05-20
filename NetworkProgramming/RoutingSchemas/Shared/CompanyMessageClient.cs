using System.Net.Sockets;
using System.Text;

namespace RoutingSchemas.Shared;

internal sealed class CompanyMessageClient : IAsyncDisposable
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoopTask;

    public event Action<string>? Log;

    public event Action<CompanyMessagePacket>? MessageReceived;

    public event Action<bool>? ConnectionStateChanged;

    public bool IsConnected => _client is not null;

    public async Task ConnectAsync(string host, int port, string clientName, IReadOnlyCollection<CompanyMessageKind> subscriptions)
    {
        if (_client is not null)
        {
            return;
        }

        _client = new TcpClient();
        await _client.ConnectAsync(host, port).ConfigureAwait(false);

        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        _writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true)
        {
            AutoFlush = true
        };

        _cts = new CancellationTokenSource();

        await _writer.WriteLineAsync(CompanyMessageProtocol.Serialize(CompanyMessageProtocol.CreateHello(clientName, subscriptions)))
            .ConfigureAwait(false);

        ConnectionStateChanged?.Invoke(true);
        Log?.Invoke($"Connected to {host}:{port} as {clientName}.");
        Log?.Invoke($"Subscriptions: {CompanyMessageProtocol.FormatSubscriptions(subscriptions)}");

        _receiveLoopTask = ReceiveLoopAsync(_cts.Token);
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();

        try
        {
            _client?.Close();
        }
        catch
        {
        }

        if (_receiveLoopTask is not null)
        {
            try
            {
                await _receiveLoopTask.ConfigureAwait(false);
            }
            catch
            {
            }
        }

        _receiveLoopTask = null;
        _reader?.Dispose();
        _writer?.Dispose();
        _client?.Dispose();

        _reader = null;
        _writer = null;
        _client = null;

        _cts?.Dispose();
        _cts = null;

        ConnectionStateChanged?.Invoke(false);
        Log?.Invoke("Disconnected.");
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(DisconnectAsync());
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        if (_reader is null)
        {
            return;
        }

        try
        {
            while (!token.IsCancellationRequested)
            {
                string? line = await _reader.ReadLineAsync().ConfigureAwait(false);
                if (line is null)
                {
                    break;
                }

                CompanyMessagePacket? packet = CompanyMessageProtocol.Deserialize(line);
                if (packet is null)
                {
                    continue;
                }

                if (packet.Kind == CompanyMessageKind.Emergency || packet.Kind == CompanyMessageKind.News || packet.Kind == CompanyMessageKind.Reminder || packet.Kind == CompanyMessageKind.Entertainment)
                {
                    MessageReceived?.Invoke(packet);
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
            if (_client is not null)
            {
                ConnectionStateChanged?.Invoke(false);
            }
        }
    }
}