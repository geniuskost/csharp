using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace Email;

public sealed class MainForm : Form
{
    private readonly TextBox _txtSmtpHost = new() { Text = "smtp.gmail.com", Width = 220 };
    private readonly TextBox _txtSmtpPort = new() { Text = "587", Width = 80 };
    private readonly TextBox _txtUser = new() { Width = 220 };
    private readonly TextBox _txtPassword = new() { Width = 220, UseSystemPasswordChar = true };
    private readonly TextBox _txtFrom = new() { Width = 220 };
    private readonly TextBox _txtTo = new() { Width = 220 };
    private readonly TextBox _txtSubject = new() { Dock = DockStyle.Fill };
    private readonly TextBox _txtBody = new() { Multiline = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill };
    private readonly ListBox _lstAttachments = new() { Dock = DockStyle.Fill };
    private readonly Button _btnAddAttachment = new() { Text = "Add attachment", Width = 130 };
    private readonly Button _btnRemoveAttachment = new() { Text = "Remove selected", Width = 130 };
    private readonly Button _btnSend = new() { Text = "Send email", Width = 120 };
    private readonly Label _lblStatus = new() { Dock = DockStyle.Bottom, Height = 24, Text = "Ready." };

    private readonly List<string> _attachments = new();

    public MainForm()
    {
        Text = "Email Sender";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 680);

        BuildUi();
        BindEvents();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        root.Controls.Add(BuildSmtpPanel(), 0, 0);
        root.Controls.Add(BuildMessagePanel(), 0, 1);
        root.Controls.Add(BuildAttachmentsPanel(), 0, 2);
        root.Controls.Add(BuildSendPanel(), 0, 3);

        Controls.Add(root);
        Controls.Add(_lblStatus);
    }

    private Control BuildSmtpPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            AutoSize = true
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        panel.Controls.Add(new Label { Text = "SMTP host:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        panel.Controls.Add(_txtSmtpHost, 1, 0);
        panel.Controls.Add(new Label { Text = "Port:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 0);
        panel.Controls.Add(_txtSmtpPort, 3, 0);

        panel.Controls.Add(new Label { Text = "Username:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        panel.Controls.Add(_txtUser, 1, 1);
        panel.Controls.Add(new Label { Text = "Password:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 1);
        panel.Controls.Add(_txtPassword, 3, 1);

        panel.Controls.Add(new Label { Text = "From:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        panel.Controls.Add(_txtFrom, 1, 2);
        panel.Controls.Add(new Label { Text = "To:", AutoSize = true, Anchor = AnchorStyles.Left }, 2, 2);
        panel.Controls.Add(_txtTo, 3, 2);

        panel.Controls.Add(new Label
        {
            Text = "Fill SMTP login and sender/recipient addresses, then add attachments below.",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 3);

        panel.SetColumnSpan(panel.GetControlFromPosition(0, 3), 4);
        return panel;
    }

    private Control BuildMessagePanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(new Label { Text = "Subject:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        panel.Controls.Add(_txtSubject, 1, 0);
        panel.Controls.Add(new Label { Text = "Body:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        panel.Controls.Add(_txtBody, 1, 1);

        return panel;
    }

    private Control BuildAttachmentsPanel()
    {
        var panel = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Attachments"
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(8)
        };

        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        inner.Controls.Add(_lstAttachments, 0, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        buttons.Controls.Add(_btnAddAttachment);
        buttons.Controls.Add(_btnRemoveAttachment);
        inner.Controls.Add(buttons, 1, 0);

        panel.Controls.Add(inner);
        return panel;
    }

    private Control BuildSendPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        _btnSend.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        _btnSend.Left = panel.Width - _btnSend.Width;
        panel.Controls.Add(_btnSend);
        return panel;
    }

    private void BindEvents()
    {
        _btnAddAttachment.Click += (_, _) => AddAttachment();
        _btnRemoveAttachment.Click += (_, _) => RemoveSelectedAttachment();
        _btnSend.Click += async (_, _) => await SendEmailAsync();
    }

    private void AddAttachment()
    {
        using var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Title = "Choose attachment files"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (string fileName in dialog.FileNames)
        {
            if (_attachments.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _attachments.Add(fileName);
            _lstAttachments.Items.Add(fileName);
        }

        SetStatus($"Added {_attachments.Count} attachment(s).");
    }

    private void RemoveSelectedAttachment()
    {
        if (_lstAttachments.SelectedItem is not string selected)
        {
            return;
        }

        _attachments.Remove(selected);
        _lstAttachments.Items.Remove(selected);
        SetStatus("Attachment removed.");
    }

    private async Task SendEmailAsync()
    {
        if (!int.TryParse(_txtSmtpPort.Text, out int port) || port is < 1 or > 65535)
        {
            MessageBox.Show("Invalid SMTP port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!MailAddress.TryCreate(_txtFrom.Text.Trim(), out MailAddress? fromAddress))
        {
            MessageBox.Show("Invalid From address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!MailAddress.TryCreate(_txtTo.Text.Trim(), out MailAddress? toAddress))
        {
            MessageBox.Show("Invalid To address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtUser.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            MessageBox.Show("Username and password are required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var message = new MailMessage();
        message.From = fromAddress;
        message.To.Add(toAddress);
        message.Subject = _txtSubject.Text.Trim();
        message.Body = _txtBody.Text;

        foreach (string attachmentPath in _attachments)
        {
            message.Attachments.Add(new Attachment(attachmentPath));
        }

        using var client = new SmtpClient(_txtSmtpHost.Text.Trim(), port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_txtUser.Text.Trim(), _txtPassword.Text)
        };

        try
        {
            SetStatus("Sending email...");
            await client.SendMailAsync(message);
            SetStatus("Email sent successfully.");
        }
        catch (Exception ex)
        {
            SetStatus("Send failed.");
            MessageBox.Show($"Failed to send email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetStatus(string text)
    {
        _lblStatus.Text = text;
    }
}