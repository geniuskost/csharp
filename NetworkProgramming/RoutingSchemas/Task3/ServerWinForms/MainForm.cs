using System.Drawing;
using System.Net;
using System.Windows.Forms;
using RoutingSchemas.Shared;

namespace RoutingSchemas.Task3.ServerWinForms;

public sealed class MainForm : Form
{
    private readonly TextBox _txtPort = new() { Width = 90, Text = "5055" };
    private readonly TextBox _txtServerName = new() { Width = 160, Text = "Admin" };
    private readonly Button _btnStart = new() { Text = "Start", Width = 100 };
    private readonly Button _btnStop = new() { Text = "Stop", Width = 100, Enabled = false };
    private readonly ComboBox _cmbKind = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly TextBox _txtMessage = new() { Dock = DockStyle.Fill };
    private readonly Button _btnSend = new() { Text = "Broadcast", Dock = DockStyle.Right, Width = 120, Enabled = false };
    private readonly ListBox _lstClients = new() { Dock = DockStyle.Fill };
    private readonly RichTextBox _rtbLog = new() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.WhiteSmoke };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Bottom, Height = 24, Text = "Server stopped." };

    private CompanyMessageServer? _server;

    public MainForm()
    {
        Text = "Company Messages - Server";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 600);

        _cmbKind.Items.AddRange(new object[] { CompanyMessageKind.News, CompanyMessageKind.Reminder, CompanyMessageKind.Entertainment, CompanyMessageKind.Emergency });
        _cmbKind.SelectedItem = CompanyMessageKind.News;

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
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        topPanel.Controls.Add(new Label { Text = "Port:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        topPanel.Controls.Add(_txtPort, 1, 0);
        topPanel.Controls.Add(new Label { Text = "Server name:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        topPanel.Controls.Add(_txtServerName, 3, 0);
        topPanel.Controls.Add(new Label { Text = "Kind:", AutoSize = true, Anchor = AnchorStyles.Left }, 4, 0);
        topPanel.Controls.Add(_cmbKind, 5, 0);
        topPanel.Controls.Add(_btnStart, 6, 0);
        topPanel.Controls.Add(_btnStop, 7, 0);

        var clientsGroup = new GroupBox { Dock = DockStyle.Left, Width = 280, Text = "Connected clients", Padding = new Padding(8) };
        clientsGroup.Controls.Add(_lstClients);

        var logPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 10) };
        logPanel.Controls.Add(_rtbLog);

        var messagesPanel = new Panel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(0, 8, 0, 0) };
        messagesPanel.Controls.Add(_txtMessage);
        messagesPanel.Controls.Add(_btnSend);

        logPanel.Controls.Add(messagesPanel);

        var centerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 10) };
        centerPanel.Controls.Add(logPanel);
        centerPanel.Controls.Add(clientsGroup);

        Controls.Add(centerPanel);
        Controls.Add(_lblStatus);
        Controls.Add(topPanel);
    }

    private void BindEvents()
    {
        _btnStart.Click += async (_, _) => await StartServerAsync();
        _btnStop.Click += async (_, _) => await StopServerAsync();
        _btnSend.Click += async (_, _) => await BroadcastAsync();

        FormClosing += async (_, _) => await StopServerAsync();
    }

    private async Task StartServerAsync()
    {
        if (_server is not null && _server.IsRunning)
        {
            return;
        }

        if (!int.TryParse(_txtPort.Text, out int port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Invalid port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string serverName = string.IsNullOrWhiteSpace(_txtServerName.Text) ? "Admin" : _txtServerName.Text.Trim();
        _server = new CompanyMessageServer(IPAddress.Any, port, serverName);
        _server.Log += message => Ui(() => AppendLine(message));
        _server.ClientsChanged += clients => Ui(() => RefreshClients(clients));

        try
        {
            await _server.StartAsync();
            SetRunningState(true);
            AppendLine($"Server started on port {port}.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not start server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetRunningState(false);
            await DisposeServerAsync();
        }
    }

    private async Task StopServerAsync()
    {
        if (_server is null)
        {
            return;
        }

        await DisposeServerAsync();
        SetRunningState(false);
        RefreshClients(Array.Empty<ConnectedClientInfo>());
    }

    private async Task BroadcastAsync()
    {
        if (_server is null || !_server.IsRunning)
        {
            MessageBox.Show("Start the server first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string text = _txtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        CompanyMessageKind kind = _cmbKind.SelectedItem is CompanyMessageKind selectedKind ? selectedKind : CompanyMessageKind.News;

        try
        {
            await _server.BroadcastAsync(kind, text);
            _txtMessage.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Broadcast failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshClients(IReadOnlyList<ConnectedClientInfo> clients)
    {
        _lstClients.BeginUpdate();
        _lstClients.Items.Clear();

        foreach (ConnectedClientInfo client in clients)
        {
            _lstClients.Items.Add(client);
        }

        _lstClients.EndUpdate();
        _lblStatus.Text = $"Connected clients: {clients.Count}";
    }

    private void AppendLine(string text)
    {
        _rtbLog.AppendText(text + Environment.NewLine);
        _rtbLog.SelectionStart = _rtbLog.TextLength;
        _rtbLog.ScrollToCaret();
    }

    private void SetRunningState(bool running)
    {
        _btnStart.Enabled = !running;
        _btnStop.Enabled = running;
        _btnSend.Enabled = running;
        _txtPort.Enabled = !running;
        _txtServerName.Enabled = !running;
        _cmbKind.Enabled = running;
        _lblStatus.Text = running ? "Server running." : "Server stopped.";
    }

    private async Task DisposeServerAsync()
    {
        if (_server is null)
        {
            return;
        }

        await _server.DisposeAsync();
        _server = null;
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
}