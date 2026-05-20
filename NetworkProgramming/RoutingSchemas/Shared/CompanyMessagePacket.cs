namespace RoutingSchemas.Shared;

internal sealed record CompanyMessagePacket(
    CompanyMessageKind Kind,
    string Sender,
    string Text,
    DateTimeOffset SentAtUtc);

internal sealed record ConnectedClientInfo(string Id, string DisplayName, string EndPoint, string Subscriptions)
{
    public override string ToString() => $"{DisplayName} ({EndPoint}) [{Subscriptions}]";
}
