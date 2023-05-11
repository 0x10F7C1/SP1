using System.Runtime.Caching;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace Projekat;

public class HttpServerThreadPool
{
    private static MemoryCache cache = MemoryCache.Default;
    private static HttpListener listener = new HttpListener();
    private static HttpClient client = new HttpClient();
    private static readonly object lockObj = new object();
    public static void Run()
    { 
        listener.Prefixes.Add("http://127.0.0.1:8080/");
        listener.Start();

        while (listener.IsListening)
        {
            var context = listener.GetContext();
            Stopwatch stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;
            var rawUrl = request.RawUrl;
            
            string responseData = null;
            Boolean readSuccess = false;
            lock (lockObj)
            {
                if (cache.Contains(rawUrl))
                {
                    responseData = (string)cache.Get(rawUrl);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    SendResponse(responseData, response);
                    readSuccess = true;
                }
            }

            if (!readSuccess)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var url = $"https://api.deezer.com{rawUrl}";
                    var result = client.GetAsync(url).Result;
                    var resultBody = result.Content.ReadAsStringAsync().Result;
                    var json = JObject.Parse(resultBody);

                    JToken data = json["data"];
                    if (data == null)
                    {
                        responseData = "<html>" +
                        "<head></head>" +
                        "<body>" +
                        "<h3>ERROR 404: INVALID REQUEST</h3>" +
                        "</body>" +
                        "</html>";

                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        SendResponse(responseData, response);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append("<html> <head> <meta charset=\"UTF-8\"></head> <body>");
                        stringBuilder.Append("<ul>");
                        foreach (var song in data)
                        {
                            stringBuilder.Append($"<li>{song["title"]}</li>");
                        }
                        stringBuilder.Append("</ul>");
                        stringBuilder.Append("</body>");
                        stringBuilder.Append("</html>");
                        responseData = stringBuilder.ToString();
                        response.StatusCode = (int)HttpStatusCode.OK;
                        SendResponse(responseData, response);
                        lock (lockObj)
                        {
                            cache.Set(rawUrl, responseData, DateTimeOffset.UtcNow.AddMinutes(10));
                        }
                    }
                });
            }
            stopwatch.Stop();
            Console.WriteLine($"Opsluzivanje zahteva ka {rawUrl} je trajalo {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    private static void SendResponse(string responseData, HttpListenerResponse response)
    {
        response.ContentType = "text/html";
        var contentBody = System.Text.Encoding.UTF8.GetBytes(responseData);
        using (var outputStream = response.OutputStream)
        {
            outputStream.Write(contentBody, 0, contentBody.Length);
        }
    }
}