using System.Drawing;
using System.Windows.Forms;
using RoutingSchemas.Shared;

namespace RoutingSchemas.Task2.ClientWinForms;

public sealed class MainForm : Form
{
    private readonly TextBox _txtHost = new() { Width = 160, Text = "127.0.0.1" };
    private readonly TextBox _txtPort = new() { Width = 90, Text = "5055" };
    private readonly TextBox _txtName = new() { Width = 140, Text = "Employee" };
    private readonly CheckBox _chkNews = new() { Text = "News", Checked = true, AutoSize = true };
    private readonly CheckBox _chkReminder = new() { Text = "Reminder", Checked = true, AutoSize = true };
    private readonly CheckBox _chkEntertainment = new() { Text = "Entertainment", Checked = true, AutoSize = true };
    private readonly Button _btnConnect = new() { Text = "Connect", Width = 100 };
    private readonly Button _btnDisconnect = new() { Text = "Disconnect", Width = 100, Enabled = false };
    private readonly RichTextBox _rtbMessages = new() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.WhiteSmoke };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Bottom, Height = 24, Text = "Client disconnected." };

    private readonly CompanyMessageClient _client = new();

    public MainForm()
    {
        Text = "Company Messages - Client";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(780, 520);

        BuildUi();
        BindEvents();
    }

    private void BuildUi()
    {
        var topPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 54,
            ColumnCount = 8,
            Padding = new Padding(10),
            AutoSize = false
        };

        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        topPanel.Controls.Add(new Label { Text = "Host:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        topPanel.Controls.Add(_txtHost, 1, 0);
        topPanel.Controls.Add(new Label { Text = "Port:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        topPanel.Controls.Add(_txtPort, 3, 0);
        topPanel.Controls.Add(new Label { Text = "Name:", AutoSize = true, Anchor = AnchorStyles.Left }, 4, 0);
        topPanel.Controls.Add(_txtName, 5, 0);
        topPanel.Controls.Add(_btnConnect, 6, 0);
        topPanel.Controls.Add(_btnDisconnect, 7, 0);

        var subscriptionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(10, 0, 10, 0),
            WrapContents = false,
            AutoSize = false
        };
        subscriptionPanel.Controls.Add(new Label { Text = "Subscriptions:", AutoSize = true, Padding = new Padding(0, 6, 10, 0) });
        subscriptionPanel.Controls.Add(_chkNews);
        subscriptionPanel.Controls.Add(_chkReminder);
        subscriptionPanel.Controls.Add(_chkEntertainment);

        var contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10) };
        contentPanel.Controls.Add(_rtbMessages);

        Controls.Add(contentPanel);
        Controls.Add(_lblStatus);
        Controls.Add(subscriptionPanel);
        Controls.Add(topPanel);
    }

    private void BindEvents()
    {
        _btnConnect.Click += async (_, _) => await ConnectAsync();
        _btnDisconnect.Click += async (_, _) => await DisconnectAsync();

        _client.Log += message => Ui(() => AppendLine(message));
        _client.MessageReceived += packet => Ui(() => AppendLine(CompanyMessageProtocol.FormatForDisplay(packet)));
        _client.ConnectionStateChanged += connected => Ui(() => SetConnectedState(connected));

        FormClosing += async (_, _) => await DisconnectAsync();
    }

    private async Task ConnectAsync()
    {
        if (_client.IsConnected)
        {
            return;
        }

        if (!int.TryParse(_txtPort.Text, out int port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Invalid port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string host = string.IsNullOrWhiteSpace(_txtHost.Text) ? "127.0.0.1" : _txtHost.Text.Trim();
        string clientName = string.IsNullOrWhiteSpace(_txtName.Text) ? "Employee" : _txtName.Text.Trim();
        IReadOnlyCollection<CompanyMessageKind> subscriptions = GetSelectedSubscriptions();

        try
        {
            SetBusy(true);
            await _client.ConnectAsync(host, port, clientName, subscriptions);
            AppendLine($"Connected to {host}:{port}.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetBusy(false);
        }
    }

    private async Task DisconnectAsync()
    {
        if (!_client.IsConnected)
        {
            return;
        }

        await _client.DisconnectAsync();
        SetBusy(false);
    }

    private void AppendLine(string text)
    {
        _rtbMessages.AppendText(text + Environment.NewLine);
        _rtbMessages.SelectionStart = _rtbMessages.TextLength;
        _rtbMessages.ScrollToCaret();
    }

    private void SetConnectedState(bool connected)
    {
        _lblStatus.Text = connected ? "Connected." : "Client disconnected.";
        _btnConnect.Enabled = !connected;
        _btnDisconnect.Enabled = connected;
        _txtHost.Enabled = !connected;
        _txtPort.Enabled = !connected;
        _txtName.Enabled = !connected;
        _chkNews.Enabled = !connected;
        _chkReminder.Enabled = !connected;
        _chkEntertainment.Enabled = !connected;
    }

    private void SetBusy(bool busy)
    {
        _btnConnect.Enabled = !busy && !_client.IsConnected;
        _btnDisconnect.Enabled = !busy && _client.IsConnected;
        _lblStatus.Text = busy ? "Connecting..." : _lblStatus.Text;
    }

    private void Ui(Action action)
    {
        if (IsHandleCreated && !IsDisposed)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }

    private IReadOnlyCollection<CompanyMessageKind> GetSelectedSubscriptions()
    {
        List<CompanyMessageKind> subscriptions = new();

        if (_chkNews.Checked)
        {
            subscriptions.Add(CompanyMessageKind.News);
        }

        if (_chkReminder.Checked)
        {
            subscriptions.Add(CompanyMessageKind.Reminder);
        }

        if (_chkEntertainment.Checked)
        {
            subscriptions.Add(CompanyMessageKind.Entertainment);
        }

        return subscriptions.Count == 0
            ? new[] { CompanyMessageKind.News, CompanyMessageKind.Reminder, CompanyMessageKind.Entertainment }
            : subscriptions;
    }
}