using System;
using System.Net.Http;
using System.Windows.Forms;

namespace HTTP_FORMS;

public sealed class MainForm : Form
{
    private readonly TextBox _uriTextBox;
    private readonly Button _sendButton;
    private readonly TextBox _resultTextBox;
    private static readonly HttpClient HttpClient = new();

    public MainForm()
    {
        Text = "HTTP Raw Page Viewer";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        _uriTextBox = new TextBox
        {
            Left = 12,
            Top = 12,
            Width = 730,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Text = "http://localhost:8080/"
        };

        _sendButton = new Button
        {
            Left = 755,
            Top = 10,
            Width = 120,
            Height = 28,
            Text = "Send request",
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _sendButton.Click += SendButtonOnClick;

        _resultTextBox = new TextBox
        {
            Left = 12,
            Top = 48,
            Width = 863,
            Height = 500,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            WordWrap = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.Add(_uriTextBox);
        Controls.Add(_sendButton);
        Controls.Add(_resultTextBox);
    }

    private async void SendButtonOnClick(object? sender, EventArgs e)
    {
        try
        {
            var uriText = _uriTextBox.Text.Trim();
            if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("Invalid URI format.");
            }

            _sendButton.Enabled = false;
            _resultTextBox.Text = "Loading...";

            var html = await HttpClient.GetStringAsync(uri);
            _resultTextBox.Text = html;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Request failed: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            _sendButton.Enabled = true;
        }
    }
}
