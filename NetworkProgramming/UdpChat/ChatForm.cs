using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UdpChat;

public sealed class ChatForm : Form
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly Dictionary<string, ChatSession> _sessionsById = new();
    private readonly Dictionary<string, string> _sessionIdByEndpoint = new(StringComparer.OrdinalIgnoreCase);

    private readonly TextBox _txtUsername = new() { Width = 140, Text = "User" };
    private readonly ComboBox _cmbColor = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtLocalPort = new() { Width = 80, Text = "9000" };
    private readonly Button _btnStart = new() { Text = "Запустити", Width = 90 };

    private readonly TextBox _txtChatName = new() { Width = 120, PlaceholderText = "Назва чату" };
    private readonly TextBox _txtRemoteIp = new() { Width = 120, Text = "127.0.0.1" };
    private readonly TextBox _txtRemotePort = new() { Width = 80, Text = "9001" };
    private readonly Button _btnAddChat = new() { Text = "Додати чат", Width = 100 };

    private readonly ListBox _lstChats = new() { Width = 220, Dock = DockStyle.Left };
    private readonly RichTextBox _rtbMessages = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly TextBox _txtMessage = new() { Dock = DockStyle.Fill };
    private readonly Button _btnSend = new() { Text = "Надіслати", Dock = DockStyle.Right, Width = 100 };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Bottom, Height = 24, Text = "UDP чат не запущено." };

    private UdpClient? _receiver;
    private UdpClient? _sender;
    private CancellationTokenSource? _cts;

    public ChatForm()
    {
        Text = "UDP Chat (GUI)";
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;

        InitializeUi();
        BindEvents();
    }

    private void InitializeUi()
    {
        _cmbColor.Items.AddRange(Enum.GetNames<ConsoleColor>());
        _cmbColor.SelectedItem = ConsoleColor.White.ToString();

        var topConfigPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(8),
            WrapContents = false
        };
        topConfigPanel.Controls.Add(new Label { Text = "Username:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        topConfigPanel.Controls.Add(_txtUsername);
        topConfigPanel.Controls.Add(new Label { Text = "Колір:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        topConfigPanel.Controls.Add(_cmbColor);
        topConfigPanel.Controls.Add(new Label { Text = "Локальний порт:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        topConfigPanel.Controls.Add(_txtLocalPort);
        topConfigPanel.Controls.Add(_btnStart);

        var topChatPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(8),
            WrapContents = false
        };
        topChatPanel.Controls.Add(new Label { Text = "Чат:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        topChatPanel.Controls.Add(_txtChatName);
        topChatPanel.Controls.Add(new Label { Text = "IP:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        topChatPanel.Controls.Add(_txtRemoteIp);
        topChatPanel.Controls.Add(new Label { Text = "Порт:", AutoSize = true, Padding = new Padding(10, 6, 0, 0) });
        topChatPanel.Controls.Add(_txtRemotePort);
        topChatPanel.Controls.Add(_btnAddChat);

        var mainPanel = new Panel { Dock = DockStyle.Fill };
        var messagesPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 8) };
        var sendPanel = new Panel { Dock = DockStyle.Bottom, Height = 36 };

        sendPanel.Controls.Add(_txtMessage);
        sendPanel.Controls.Add(_btnSend);
        messagesPanel.Controls.Add(_rtbMessages);
        messagesPanel.Controls.Add(sendPanel);

        mainPanel.Controls.Add(messagesPanel);
        mainPanel.Controls.Add(_lstChats);

        Controls.Add(mainPanel);
        Controls.Add(_lblStatus);
        Controls.Add(topChatPanel);
        Controls.Add(topConfigPanel);
    }

    private void BindEvents()
    {
        _btnStart.Click += (_, _) => StartChatClient();
        _btnAddChat.Click += (_, _) => AddChatFromInputs();
        _lstChats.SelectedIndexChanged += (_, _) => ShowSelectedChatHistory();
        _btnSend.Click += (_, _) => SendCurrentMessage();
        _txtMessage.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendCurrentMessage();
            }
        };
        FormClosing += (_, _) => StopNetworking();
    }

    private void StartChatClient()
    {
        if (_receiver is not null)
        {
            SetStatus("UDP чат вже запущено.");
            return;
        }

        if (!int.TryParse(_txtLocalPort.Text, out int localPort) || localPort <= 0 || localPort > 65535)
        {
            MessageBox.Show("Некоректний локальний порт.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _receiver = new UdpClient(localPort);
            _sender = new UdpClient();
            _cts = new CancellationTokenSource();

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
            SetStatus($"Слухаю UDP порт {localPort}.");
        }
        catch (Exception ex)
        {
            StopNetworking();
            MessageBox.Show($"Не вдалося запустити UDP клієнт: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        if (_receiver is null)
        {
            return;
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await _receiver.ReceiveAsync(token);
                string payload = Encoding.UTF8.GetString(result.Buffer);
                ChatMessage message = DeserializeMessage(payload);

                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    continue;
                }

                string endpointKey = ToEndpointKey(result.RemoteEndPoint.Address, result.RemoteEndPoint.Port);
                BeginInvoke(() =>
                {
                    ChatSession session = EnsureSessionForIncoming(endpointKey, result.RemoteEndPoint);
                    session.History.Add(FormatLine(message.Username, message.Text, incoming: true));

                    if (GetSelectedSession()?.Id == session.Id)
                    {
                        RenderSession(session);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                BeginInvoke(() => SetStatus($"Помилка отримання: {ex.Message}"));
            }
        }
    }

    private void AddChatFromInputs()
    {
        string chatName = _txtChatName.Text.Trim();
        if (string.IsNullOrWhiteSpace(chatName))
        {
            chatName = $"Chat {_sessionsById.Count + 1}";
        }

        if (!IPAddress.TryParse(_txtRemoteIp.Text.Trim(), out IPAddress? remoteIp))
        {
            MessageBox.Show("Некоректна IP адреса.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(_txtRemotePort.Text, out int remotePort) || remotePort <= 0 || remotePort > 65535)
        {
            MessageBox.Show("Некоректний порт чату.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string endpointKey = ToEndpointKey(remoteIp, remotePort);
        if (_sessionIdByEndpoint.ContainsKey(endpointKey))
        {
            SetStatus("Чат з таким endpoint вже існує.");
            SelectChatByEndpoint(endpointKey);
            return;
        }

        var session = new ChatSession(Guid.NewGuid().ToString("N"), chatName, remoteIp, remotePort);
        _sessionsById.Add(session.Id, session);
        _sessionIdByEndpoint[endpointKey] = session.Id;
        _lstChats.Items.Add(session);
        _lstChats.SelectedItem = session;

        SetStatus($"Додано чат '{chatName}' ({endpointKey}).");
    }

    private void SendCurrentMessage()
    {
        if (_sender is null)
        {
            MessageBox.Show("Спочатку натисніть 'Запустити'.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ChatSession? session = GetSelectedSession();
        if (session is null)
        {
            MessageBox.Show("Оберіть чат або створіть новий.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string text = _txtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        string username = string.IsNullOrWhiteSpace(_txtUsername.Text) ? "User" : _txtUsername.Text.Trim();
        string color = (_cmbColor.SelectedItem?.ToString() ?? ConsoleColor.White.ToString()).Trim();

        var message = new ChatMessage(username, color, text);
        string json = JsonSerializer.Serialize(message, JsonOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        var endpoint = new IPEndPoint(session.RemoteIp, session.RemotePort);

        try
        {
            _sender.Send(bytes, bytes.Length, endpoint);
            session.History.Add(FormatLine(username, text, incoming: false));
            _txtMessage.Clear();
            RenderSession(session);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не вдалося надіслати повідомлення: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private ChatSession EnsureSessionForIncoming(string endpointKey, IPEndPoint remoteEndPoint)
    {
        if (_sessionIdByEndpoint.TryGetValue(endpointKey, out string? sessionId)
            && _sessionsById.TryGetValue(sessionId, out ChatSession? existingSession))
        {
            return existingSession;
        }

        var session = new ChatSession(
            Guid.NewGuid().ToString("N"),
            $"From {remoteEndPoint.Address}:{remoteEndPoint.Port}",
            remoteEndPoint.Address,
            remoteEndPoint.Port);

        _sessionsById[session.Id] = session;
        _sessionIdByEndpoint[endpointKey] = session.Id;
        _lstChats.Items.Add(session);

        if (_lstChats.SelectedItem is null)
        {
            _lstChats.SelectedItem = session;
        }

        return session;
    }

    private ChatSession? GetSelectedSession() => _lstChats.SelectedItem as ChatSession;

    private void ShowSelectedChatHistory()
    {
        ChatSession? session = GetSelectedSession();
        if (session is null)
        {
            _rtbMessages.Clear();
            return;
        }

        RenderSession(session);
    }

    private void RenderSession(ChatSession session)
    {
        _rtbMessages.Clear();
        foreach (string line in session.History)
        {
            _rtbMessages.AppendText(line + Environment.NewLine);
        }

        _rtbMessages.SelectionStart = _rtbMessages.TextLength;
        _rtbMessages.ScrollToCaret();
    }

    private void SelectChatByEndpoint(string endpointKey)
    {
        if (!_sessionIdByEndpoint.TryGetValue(endpointKey, out string? sessionId)
            || !_sessionsById.TryGetValue(sessionId, out ChatSession? session))
        {
            return;
        }

        _lstChats.SelectedItem = session;
    }

    private static string ToEndpointKey(IPAddress ip, int port) => $"{ip}:{port}";

    private static string FormatLine(string username, string text, bool incoming)
    {
        string direction = incoming ? "<-" : "->";
        return $"[{DateTime.Now:HH:mm:ss}] {direction} {username}: {text}";
    }

    private static ChatMessage DeserializeMessage(string payload)
    {
        try
        {
            ChatMessage? message = JsonSerializer.Deserialize<ChatMessage>(payload, JsonOptions);
            if (message is not null)
            {
                return message;
            }
        }
        catch
        {
            // compatibility with plain text messages
        }

        return new ChatMessage("Unknown", ConsoleColor.Gray.ToString(), payload);
    }

    private void StopNetworking()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _receiver?.Close();
        _receiver?.Dispose();
        _receiver = null;

        _sender?.Close();
        _sender?.Dispose();
        _sender = null;
    }

    private void SetStatus(string text)
    {
        _lblStatus.Text = text;
    }

    private sealed record ChatMessage(string Username, string Color, string Text);

    private sealed class ChatSession
    {
        public ChatSession(string id, string name, IPAddress remoteIp, int remotePort)
        {
            Id = id;
            Name = name;
            RemoteIp = remoteIp;
            RemotePort = remotePort;
        }

        public string Id { get; }
        public string Name { get; }
        public IPAddress RemoteIp { get; }
        public int RemotePort { get; }
        public List<string> History { get; } = new();

        public override string ToString() => $"{Name} ({RemoteIp}:{RemotePort})";
    }
}
