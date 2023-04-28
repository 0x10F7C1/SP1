
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;

namespace Projekat;


public static class HttpServer
{
    static async Task Main(String[] args)
    {
        var listener = new HttpListener();
        //listener.Prefixes.Add("https://127.0.0.1:8080/");
        listener.Prefixes.Add("http://127.0.0.1:8080/");
        listener.Start();

        while (listener.IsListening)
        {
            var context = listener.GetContext();

            new Thread(() =>
            {
                
                Console.WriteLine("OK");
            }).Start();
            
        }
        
    }
}