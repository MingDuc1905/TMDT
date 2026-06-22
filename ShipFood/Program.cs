using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShipFood
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:8080/";
            
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(baseAddress);
            listener.Start();
            
            Console.WriteLine($"✓ Development Server running at {baseAddress}");
            Console.WriteLine("  Press Ctrl+C to stop...\n");
            
            try
            {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    ProcessRequest(context);
                }
            }
            catch (HttpListenerException)
            {
                // Expected when stopping
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }

        static void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";
            response.StatusCode = 200;

            string html = @"
<!DOCTYPE html>
<html>
<head>
    <title>ShipFood - Development Server</title>
    <style>
        body { font-family: Arial; margin: 50px; }
        .success { color: green; font-size: 24px; }
        .info { color: #666; margin-top: 20px; }
    </style>
</head>
<body>
    <div class='success'>✓ Server is running!</div>
    <div class='info'>
        <p>ShipFood Development Server</p>
        <p>URL: " + context.Request.RawUrl + @"</p>
        <p>This is a minimal HTTP server for development.</p>
        <p>To serve ASP.NET MVC, use Visual Studio or IIS.</p>
    </div>
</body>
</html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
