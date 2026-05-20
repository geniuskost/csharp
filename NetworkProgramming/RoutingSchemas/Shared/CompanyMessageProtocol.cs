using System.Text.Json;
using System.Text.Json.Serialization;

namespace RoutingSchemas.Shared;

internal static class CompanyMessageProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static CompanyMessagePacket CreateMessage(CompanyMessageKind kind, string sender, string text)
    {
        return new CompanyMessagePacket(kind, sender.Trim(), text.Trim(), DateTimeOffset.UtcNow);
    }

    public static CompanyMessagePacket CreateHello(string sender, IReadOnlyCollection<CompanyMessageKind> subscriptions)
    {
        string payload = string.Join(',', subscriptions.Select(kind => kind.ToString()));
        return new CompanyMessagePacket(CompanyMessageKind.News, sender.Trim(), $"HELLO:{payload}", DateTimeOffset.UtcNow);
    }

    public static string Serialize(CompanyMessagePacket packet)
    {
        return JsonSerializer.Serialize(packet, JsonOptions);
    }

    public static CompanyMessagePacket? Deserialize(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CompanyMessagePacket>(line, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static string FormatForDisplay(CompanyMessagePacket packet)
    {
        string stamp = packet.SentAtUtc.ToLocalTime().ToString("HH:mm:ss");
        string prefix = packet.Kind == CompanyMessageKind.Emergency ? "[EMERGENCY] " : string.Empty;
        return packet.Kind == CompanyMessageKind.Emergency
            ? $"[{stamp}] {prefix}{packet.Sender}: {packet.Text}"
            : $"[{stamp}] {packet.Kind}: {packet.Sender}: {packet.Text}";
    }

    public static IReadOnlyCollection<CompanyMessageKind> ParseSubscriptions(string text)
    {
        HashSet<CompanyMessageKind> subscriptions = new();

        foreach (string item in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryParseKind(item, out CompanyMessageKind kind) && kind != CompanyMessageKind.Emergency)
            {
                subscriptions.Add(kind);
            }
        }

        return subscriptions.Count == 0
            ? new[] { CompanyMessageKind.News, CompanyMessageKind.Reminder, CompanyMessageKind.Entertainment }
            : subscriptions;
    }

    public static string FormatSubscriptions(IReadOnlyCollection<CompanyMessageKind> subscriptions)
    {
        return subscriptions.Count == 0
            ? "none"
            : string.Join(',', subscriptions.OrderBy(kind => kind.ToString()));
    }

    public static bool TryParseKind(string text, out CompanyMessageKind kind)
    {
        kind = default;

        if (Enum.TryParse(text, ignoreCase: true, out CompanyMessageKind parsed) && Enum.IsDefined(typeof(CompanyMessageKind), parsed))
        {
            kind = parsed;
            return true;
        }

        return false;
    }
}
