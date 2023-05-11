using System.Runtime.Caching;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Web;
using System.Xml.XPath;
using System.Diagnostics;

namespace Projekat;
public class MainEntry
{
    static void Main(String[] args)
    {
        HttpServer.Run();
    }
}