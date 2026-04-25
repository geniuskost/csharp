using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace GuessClient;

public sealed class GuessForm : Form
{
    private readonly TextBox _serverIpTextBox;
    private readonly TextBox _serverPortTextBox;
    private readonly TextBox _guessTextBox;
    private readonly Button _sendButton;
    private readonly Button _connectButton;
    private readonly TextBox _logTextBox;
    private readonly Label _statusLabel;

    private UdpClient? _udpClient;
    private IPEndPoint? _serverEndpoint;
    private CancellationTokenSource? _listenCts;
    private bool _gameFinished;

    public GuessForm()
    {
        Text = "UDP Guess Client";
        Width = 700;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;

        var serverLabel = new Label
        {
            Left = 20,
            Top = 20,
            Width = 80,
            Text = "Server IP"
        };

        _serverIpTextBox = new TextBox
        {
            Left = 100,
            Top = 16,
            Width = 140,
            Text = "127.0.0.1"
        };

        var portLabel = new Label
        {
            Left = 260,
            Top = 20,
            Width = 50,
            Text = "Port"
        };

        _serverPortTextBox = new TextBox
        {
            Left = 305,
            Top = 16,
            Width = 80,
            Text = "5055"
        };

        _connectButton = new Button
        {
            Left = 400,
            Top = 14,
            Width = 120,
            Text = "Connect"
        };
        _connectButton.Click += (_, _) => Connect();

        _logTextBox = new TextBox
        {
            Left = 20,
            Top = 55,
            Width = 640,
            Height = 300,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        _guessTextBox = new TextBox
        {
            Left = 20,
            Top = 370,
            Width = 470,
            Enabled = false
        };
        _guessTextBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendGuessAsync();
            }
        };

        _sendButton = new Button
        {
            Left = 505,
            Top = 368,
            Width = 155,
            Text = "Send guess",
            Enabled = false
        };
        _sendButton.Click += async (_, _) => await SendGuessAsync();

        _statusLabel = new Label
        {
            Left = 20,
            Top = 410,
            Width = 640,
            Height = 40,
            Text = "Not connected"
        };

        Controls.Add(serverLabel);
        Controls.Add(_serverIpTextBox);
        Controls.Add(portLabel);
        Controls.Add(_serverPortTextBox);
        Controls.Add(_connectButton);
        Controls.Add(_logTextBox);
        Controls.Add(_guessTextBox);
        Controls.Add(_sendButton);
        Controls.Add(_statusLabel);

        FormClosing += (_, _) => Disconnect();
    }

    private void Connect()
    {
        if (_udpClient is not null)
        {
            SetStatus("Already connected.", false);
            return;
        }

        if (!IPAddress.TryParse(_serverIpTextBox.Text.Trim(), out IPAddress? ip))
        {
            SetStatus("Некоректна IP-адреса сервера.", false);
            return;
        }

        if (!int.TryParse(_serverPortTextBox.Text.Trim(), out int port) || port is < 1 or > 65535)
        {
            SetStatus("Некоректний порт сервера.", false);
            return;
        }

        _serverEndpoint = new IPEndPoint(ip, port);
        _udpClient = new UdpClient(0);
        _udpClient.Client.ReceiveTimeout = 0;
        _listenCts = new CancellationTokenSource();
        _gameFinished = false;

        _connectButton.Enabled = false;
        _serverIpTextBox.Enabled = false;
        _serverPortTextBox.Enabled = false;
        _guessTextBox.Enabled = true;
        _sendButton.Enabled = true;

        AppendLog($"Підключено. Локальний порт: {((IPEndPoint)_udpClient.Client.LocalEndPoint!).Port}");
        SetStatus("Підключено до сервера. Введіть число 1..100.", true);

        _ = Task.Run(() => ListenAsync(_listenCts.Token));
    }

    private async Task SendGuessAsync()
    {
        if (_udpClient is null || _serverEndpoint is null)
        {
            SetStatus("Спочатку натисніть Connect.", false);
            return;
        }

        if (_gameFinished)
        {
            SetStatus("Гру завершено. Очікуйте закриття або перезапустіть клієнт.", false);
            return;
        }

        string guessText = _guessTextBox.Text.Trim();

        if (!int.TryParse(guessText, out int guess) || guess is < 1 or > 100)
        {
            SetStatus("Введіть ціле число від 1 до 100.", false);
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(guess.ToString());
            await _udpClient.SendAsync(data, data.Length, _serverEndpoint);
            AppendLog($"Відправлено: {guess}");
            _guessTextBox.Clear();
            SetStatus("Спробу надіслано.", true);
        }
        catch (Exception ex)
        {
            SetStatus($"Помилка надсилання: {ex.Message}", false);
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        if (_udpClient is null)
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync(cancellationToken);
                string message = Encoding.UTF8.GetString(result.Buffer).Trim();

                BeginInvoke(() =>
                {
                    AppendLog($"Сервер: {message}");

                    if (message.StartsWith("Гравець із", StringComparison.OrdinalIgnoreCase))
                    {
                        _gameFinished = true;
                        _guessTextBox.Enabled = false;
                        _sendButton.Enabled = false;
                        SetStatus("Гру завершено.", false);
                        return;
                    }

                    if (string.Equals(message, "Вгадав!", StringComparison.OrdinalIgnoreCase))
                    {
                        SetStatus("Вітаємо! Ви вгадали число.", true);
                        return;
                    }

                    SetStatus(message, true);
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (ObjectDisposedException)
        {
            // Socket already disposed.
        }
        catch (Exception ex)
        {
            BeginInvoke(() => SetStatus($"Помилка прийому: {ex.Message}", false));
        }
    }

    private void AppendLog(string text)
    {
        _logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} | {text}{Environment.NewLine}");
    }

    private void SetStatus(string text, bool success)
    {
        _statusLabel.Text = text;
        _statusLabel.ForeColor = success ? Color.DarkGreen : Color.DarkRed;
    }

    private void Disconnect()
    {
        _listenCts?.Cancel();
        _listenCts?.Dispose();
        _listenCts = null;

        _udpClient?.Dispose();
        _udpClient = null;
        _serverEndpoint = null;
    }
}
