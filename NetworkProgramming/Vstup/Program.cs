using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Vstup;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Enter a site address");
        string? input = Console.ReadLine();

        if (!TryCreateHttpUri(input, out Uri uri))
        {
            Console.WriteLine("Invalid address. Use an HTTP address like http://example.com.");
            return;
        }

        try
        {
            SendHttpRequest(uri);
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request failed: {ex.Message}");
        }
    }

    private static bool TryCreateHttpUri(string? input, out Uri uri)
    {
        string value = string.IsNullOrWhiteSpace(input) ? "http://example.com" : input.Trim();

        if (!value.Contains("://", StringComparison.Ordinal))
        {
            value = "http://" + value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out uri!))
        {
            return false;
        }

        return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);
    }

    private static void SendHttpRequest(Uri uri)
    {
        int targetPort = uri.IsDefaultPort ? 80 : uri.Port;
        IPAddress ip = ResolveIpv4(uri.Host);
        IPEndPoint ep = new IPEndPoint(ip, targetPort);

        using Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        s.Connect(ep);

        if (s.Connected)
        {
            string request =
                $"GET {GetRequestTarget(uri)} HTTP/1.1\r\nHost: {uri.Host}\r\nConnection: close\r\n\r\n";
            s.Send(Encoding.ASCII.GetBytes(request));

            byte[] buffer = new byte[1024];
            int bytesRead;
            var responseBuilder = new StringBuilder();

            do
            {
                bytesRead = s.Receive(buffer);

                if (bytesRead > 0)
                {
                    responseBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                }
            } while (bytesRead > 0);

            string response = responseBuilder.ToString();
            int headerEnd = response.IndexOf("\r\n\r\n", StringComparison.Ordinal);

            if (headerEnd >= 0)
            {
                response = response[(headerEnd + 4)..];
            }

            Console.WriteLine(response);
        }
    }

    private static IPAddress ResolveIpv4(string host)
    {
        IPAddress[] addresses = Dns.GetHostAddresses(host);

        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return address;
            }
        }

        throw new SocketException((int)SocketError.AddressFamilyNotSupported);
    }

    private static string GetRequestTarget(Uri uri)
    {
        string target = uri.PathAndQuery;
        return string.IsNullOrEmpty(target) ? "/" : target;
    }
}