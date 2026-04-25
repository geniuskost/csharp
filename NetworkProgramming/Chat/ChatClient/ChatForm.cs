using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace ChatClient;

public sealed class ChatForm : Form
{
    private readonly TextBox _usernameTextBox;
    private readonly Button _connectButton;
    private readonly TextBox _chatHistoryTextBox;
    private readonly TextBox _messageTextBox;
    private readonly Button _sendButton;
    private readonly Button _refreshButton;
    private readonly Label _statusLabel;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private string _username = string.Empty;
    private int _lastSeenMessageId;
    private readonly SemaphoreSlim _requestLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ChatForm()
    {
        Text = "Simple TCP Chat";
        Width = 700;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;

        var usernameLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 90,
            Text = "Username"
        };

        _usernameTextBox = new TextBox
        {
            Left = 110,
            Top = 16,
            Width = 220
        };

        _connectButton = new Button
        {
            Left = 350,
            Top = 14,
            Width = 100,
            Text = "Connect"
        };
        _connectButton.Click += async (_, _) => await ConnectAsync();

        _chatHistoryTextBox = new TextBox
        {
            Left = 20,
            Top = 55,
            Width = 640,
            Height = 290,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        _messageTextBox = new TextBox
        {
            Left = 20,
            Top = 360,
            Width = 470,
            Height = 30
        };

        _sendButton = new Button
        {
            Left = 505,
            Top = 358,
            Width = 75,
            Text = "Send",
            Enabled = false
        };
        _sendButton.Click += async (_, _) => await SendMessageAsync();

        _refreshButton = new Button
        {
            Left = 585,
            Top = 358,
            Width = 75,
            Text = "Refresh",
            Enabled = false
        };
        _refreshButton.Click += async (_, _) => await RefreshAsync();

        _statusLabel = new Label
        {
            Left = 20,
            Top = 405,
            Width = 640,
            Height = 40,
            Text = "Not connected"
        };

        Controls.Add(usernameLabel);
        Controls.Add(_usernameTextBox);
        Controls.Add(_connectButton);
        Controls.Add(_chatHistoryTextBox);
        Controls.Add(_messageTextBox);
        Controls.Add(_sendButton);
        Controls.Add(_refreshButton);
        Controls.Add(_statusLabel);

        FormClosing += (_, _) => Disconnect();
    }

    private async Task ConnectAsync()
    {
        if (_client is { Connected: true })
        {
            SetStatus("Already connected.", false);
            return;
        }

        string username = _usernameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            SetStatus("Username is required.", false);
            return;
        }

        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5050);
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
            _writer = new StreamWriter(_stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            _username = username;

            ChatResponse response = await SendRequestAsync(new ChatRequest("connect", _username, null, _lastSeenMessageId));

            if (!response.Success)
            {
                SetStatus(response.Message, false);
                Disconnect();
                return;
            }

            _connectButton.Enabled = false;
            _usernameTextBox.Enabled = false;
            _sendButton.Enabled = true;
            _refreshButton.Enabled = true;

            SetStatus("Connected. Click Refresh to get new messages.", true);
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Connection error: {ex.Message}", false);
            Disconnect();
        }
    }

    private async Task SendMessageAsync()
    {
        if (!IsConnected())
        {
            SetStatus("Connect to the server first.", false);
            return;
        }

        string text = _messageTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            SetStatus("Message cannot be empty.", false);
            return;
        }

        try
        {
            ChatResponse response = await SendRequestAsync(new ChatRequest("send", _username, text, _lastSeenMessageId));

            if (!response.Success)
            {
                SetStatus(response.Message, false);
                return;
            }

            AppendMessage(_username, text);
            _messageTextBox.Clear();
            SetStatus("Message sent.", true);
        }
        catch (Exception ex)
        {
            SetStatus($"Send error: {ex.Message}", false);
        }
    }

    private async Task RefreshAsync()
    {
        if (!IsConnected())
        {
            SetStatus("Connect to the server first.", false);
            return;
        }

        try
        {
            ChatResponse response = await SendRequestAsync(new ChatRequest("refresh", _username, null, _lastSeenMessageId));

            if (!response.Success)
            {
                SetStatus(response.Message, false);
                return;
            }

            foreach (ChatMessage message in response.Messages)
            {
                AppendMessage(message.Username, message.Message);
                _lastSeenMessageId = Math.Max(_lastSeenMessageId, message.Id);
            }

            SetStatus("Chat updated.", true);
        }
        catch (Exception ex)
        {
            SetStatus($"Refresh error: {ex.Message}", false);
        }
    }

    private async Task<ChatResponse> SendRequestAsync(ChatRequest request)
    {
        if (_writer is null || _reader is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }

        await _requestLock.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(request, JsonOptions);
            await _writer.WriteLineAsync(json);

            string? line = await _reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
            {
                throw new IOException("Empty response from server.");
            }

            ChatResponse? response = JsonSerializer.Deserialize<ChatResponse>(line, JsonOptions);

            if (response is null)
            {
                throw new IOException("Invalid response format.");
            }

            return response;
        }
        finally
        {
            _requestLock.Release();
        }
    }

    private bool IsConnected() => _client is { Connected: true };

    private void AppendMessage(string username, string message)
    {
        _chatHistoryTextBox.AppendText($"[{username}]: {message}{Environment.NewLine}");
    }

    private void SetStatus(string text, bool success)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = success ? Color.DarkGreen : Color.DarkRed;
    }

    private void Disconnect()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _client?.Close();

        _writer = null;
        _reader = null;
        _stream = null;
        _client = null;
    }
}

internal sealed record ChatRequest(string Command, string? Username, string? Message, int LastMessageId);

internal sealed record ChatResponse(bool Success, string Message, IReadOnlyList<ChatMessage> Messages);

internal sealed record ChatMessage(int Id, string Username, string Message);