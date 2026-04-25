using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace RegistrationServer;

internal static class Program
{
    private const int Port = 5000;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static void Main()
    {
        string dbPath = Path.Combine(AppContext.BaseDirectory, "users.db");
        var database = new UserDatabase($"Data Source={dbPath}");
        database.Initialize();

        var listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();

        Console.WriteLine($"Registration server started on 127.0.0.1:{Port}");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            _ = Task.Run(() => HandleClient(client, database));
        }
    }

    private static async Task HandleClient(TcpClient client, UserDatabase database)
    {
        using (client)
        {
            await using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            try
            {
                string? line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    await WriteResponse(writer, false, "Empty request.");
                    return;
                }

                RegistrationRequest? request = JsonSerializer.Deserialize<RegistrationRequest>(line, JsonOptions);

                if (request is null)
                {
                    await WriteResponse(writer, false, "Invalid request format.");
                    return;
                }

                string validationError = ValidateRegistration(request, database);

                if (!string.IsNullOrEmpty(validationError))
                {
                    await WriteResponse(writer, false, validationError);
                    return;
                }

                string passwordHash = HashPassword(request.Password!);
                database.CreateUser(request.Username!, passwordHash);

                await WriteResponse(writer, true, "User successfully registered.");
                Console.WriteLine($"New user registered: {request.Username}");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                await WriteResponse(writer, false, "Username already exists.");
            }
            catch (Exception ex)
            {
                await WriteResponse(writer, false, "Server error.");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static async Task WriteResponse(StreamWriter writer, bool success, string message)
    {
        var response = new RegistrationResponse(success, message);
        string json = JsonSerializer.Serialize(response, JsonOptions);
        await writer.WriteLineAsync(json);
    }

    private static string ValidateRegistration(RegistrationRequest request, UserDatabase database)
    {
        string username = request.Username?.Trim() ?? string.Empty;
        string password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username cannot be empty.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password cannot be empty.";
        }

        if (database.UsernameExists(username))
        {
            return "Username already exists.";
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

    private static string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}

internal sealed record RegistrationRequest(string? Username, string? Password);

internal sealed record RegistrationResponse(bool Success, string Message);

internal sealed class UserDatabase
{
    private readonly string _connectionString;
    private readonly object _sync = new();

    public UserDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Initialize()
    {
        lock (_sync)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            const string sql = """
                               CREATE TABLE IF NOT EXISTS users (
                                   id INTEGER PRIMARY KEY AUTOINCREMENT,
                                   username TEXT NOT NULL UNIQUE,
                                   password_hash TEXT NOT NULL,
                                   created_at TEXT NOT NULL
                               );
                               """;

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }

    public bool UsernameExists(string username)
    {
        lock (_sync)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM users WHERE username = $username;";
            command.Parameters.AddWithValue("$username", username);

            long count = (long)(command.ExecuteScalar() ?? 0L);
            return count > 0;
        }
    }

    public void CreateUser(string username, string passwordHash)
    {
        lock (_sync)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO users (username, password_hash, created_at) VALUES ($username, $passwordHash, $createdAt);";
            command.Parameters.AddWithValue("$username", username);
            command.Parameters.AddWithValue("$passwordHash", passwordHash);
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        }
    }
}