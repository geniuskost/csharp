using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace RegistrationClient;

public sealed class RegistrationForm : Form
{
    private readonly TextBox _usernameTextBox;
    private readonly TextBox _passwordTextBox;
    private readonly Button _registerButton;
    private readonly Label _statusLabel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RegistrationForm()
    {
        Text = "Registration Client";
        Width = 380;
        Height = 250;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var usernameLabel = new Label
        {
            Left = 20,
            Top = 25,
            Width = 100,
            Text = "Username"
        };

        _usernameTextBox = new TextBox
        {
            Left = 120,
            Top = 20,
            Width = 220
        };

        var passwordLabel = new Label
        {
            Left = 20,
            Top = 75,
            Width = 100,
            Text = "Password"
        };

        _passwordTextBox = new TextBox
        {
            Left = 120,
            Top = 70,
            Width = 220,
            UseSystemPasswordChar = true
        };

        _registerButton = new Button
        {
            Left = 120,
            Top = 120,
            Width = 120,
            Text = "Register"
        };
        _registerButton.Click += async (_, _) => await RegisterAsync();

        _statusLabel = new Label
        {
            Left = 20,
            Top = 165,
            Width = 320,
            Height = 30,
            Text = string.Empty
        };

        Controls.Add(usernameLabel);
        Controls.Add(_usernameTextBox);
        Controls.Add(passwordLabel);
        Controls.Add(_passwordTextBox);
        Controls.Add(_registerButton);
        Controls.Add(_statusLabel);
    }

    private async Task RegisterAsync()
    {
        string username = _usernameTextBox.Text.Trim();
        string password = _passwordTextBox.Text;

        string validationError = ValidateInputs(username, password);

        if (!string.IsNullOrEmpty(validationError))
        {
            _statusLabel.Text = validationError;
            _statusLabel.ForeColor = Color.DarkRed;
            return;
        }

        _registerButton.Enabled = false;
        _statusLabel.Text = "Sending data to server...";
        _statusLabel.ForeColor = Color.Black;

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);

            await using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            var request = new RegistrationRequest(username, password);
            string requestJson = JsonSerializer.Serialize(request, JsonOptions);
            await writer.WriteLineAsync(requestJson);

            string? responseLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(responseLine))
            {
                _statusLabel.Text = "Empty response from server.";
                _statusLabel.ForeColor = Color.DarkRed;
                return;
            }

            RegistrationResponse? response = JsonSerializer.Deserialize<RegistrationResponse>(responseLine, JsonOptions);

            if (response is null)
            {
                _statusLabel.Text = "Invalid server response.";
                _statusLabel.ForeColor = Color.DarkRed;
                return;
            }

            _statusLabel.Text = response.Message;
            _statusLabel.ForeColor = response.Success ? Color.DarkGreen : Color.DarkRed;

            if (response.Success)
            {
                _usernameTextBox.Clear();
                _passwordTextBox.Clear();
            }
        }
        catch (SocketException ex)
        {
            _statusLabel.Text = $"Socket error: {ex.Message}";
            _statusLabel.ForeColor = Color.DarkRed;
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
            _statusLabel.ForeColor = Color.DarkRed;
        }
        finally
        {
            _registerButton.Enabled = true;
        }
    }

    private static string ValidateInputs(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username cannot be empty.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password cannot be empty.";
        }

        if (password.Length <= 6)
        {
            return "Password must be longer than 6 characters.";
        }

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            return "Password must contain uppercase, lowercase, digit, and special character.";
        }

        return string.Empty;
    }
}

internal sealed record RegistrationRequest(string Username, string Password);

internal sealed record RegistrationResponse(bool Success, string Message);