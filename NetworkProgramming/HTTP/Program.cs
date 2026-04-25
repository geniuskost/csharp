using System.Net;
using System.Text;

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/");

listener.Start();
Console.WriteLine("HTTP server started on http://localhost:8080/");
Console.WriteLine("Press Ctrl+C to stop.");

while (true)
{
    var context = await listener.GetContextAsync();
    _ = Task.Run(() => HandleRequest(context));
}

static async Task HandleRequest(HttpListenerContext context)
{
    var path = context.Request.Url?.AbsolutePath?.TrimEnd('/') ?? "/";
    if (string.IsNullOrWhiteSpace(path))
    {
        path = "/";
    }

    switch (path.ToLowerInvariant())
    {
        case "/":
            await WriteHtml(context.Response, HomePageHtml());
            break;
        case "/autobiography":
            await WriteHtml(context.Response, AutobiographyPageHtml());
            break;
        case "/fav_countries":
            await WriteHtml(context.Response, FavoriteCountriesPageHtml());
            break;
        case "/pc_data":
            await WriteJson(context.Response, PcDataJson());
            break;
        default:
            context.Response.StatusCode = 404;
            await WriteHtml(context.Response, NotFoundPageHtml());
            break;
    }
}

static async Task WriteHtml(HttpListenerResponse response, string html)
{
    response.ContentType = "text/html; charset=utf-8";
    var data = Encoding.UTF8.GetBytes(html);
    response.ContentLength64 = data.Length;
    await response.OutputStream.WriteAsync(data);
    response.Close();
}

static async Task WriteJson(HttpListenerResponse response, string json)
{
    response.ContentType = "application/json; charset=utf-8";
    var data = Encoding.UTF8.GetBytes(json);
    response.ContentLength64 = data.Length;
    await response.OutputStream.WriteAsync(data);
    response.Close();
}

static string HomePageHtml() =>
    """
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <title>Home</title>
    </head>
    <body>
      <h1>Home page</h1>
      <ul>
        <li><a href="/autobiography">Autobiography</a></li>
        <li><a href="/fav_countries">Favorite countries</a></li>
        <li><a href="/pc_data">PC data (JSON)</a></li>
      </ul>
    </body>
    </html>
    """;

static string AutobiographyPageHtml() =>
    """
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <title>Autobiography</title>
    </head>
    <body>
      <h1>Autobiography</h1>
      <p>
        I am a student who studies C# and networking. I enjoy building simple
        client-server applications and learning how HTTP and sockets work.
      </p>
      <p><a href="/">Back to home</a></p>
    </body>
    </html>
    """;

static string FavoriteCountriesPageHtml() =>
    """
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <title>Favorite countries</title>
    </head>
    <body>
      <h1>My favorite countries</h1>
      <ol>
        <li>Japan</li>
        <li>Italy</li>
        <li>Norway</li>
      </ol>
      <p><a href="/">Back to home</a></p>
    </body>
    </html>
    """;

static string NotFoundPageHtml() =>
    """
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <title>404</title>
    </head>
    <body>
      <h1>404 - Page not found</h1>
      <p><a href="/">Back to home</a></p>
    </body>
    </html>
    """;

static string PcDataJson() =>
    """
    {
      "os": "macOS",
      "cpu": "Apple Silicon",
      "ram_gb": 16,
      "storage": "512 GB SSD",
      "note": "Static JSON response"
    }
    """;
