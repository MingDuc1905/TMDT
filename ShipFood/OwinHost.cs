using System;
using Microsoft.Owin.Hosting;
using Owin;

namespace ShipFood
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:8080/";

            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine($"✓ Server running at {baseAddress}");
                Console.WriteLine("  Press Ctrl+C to stop...\n");
                Console.ReadLine();
            }
        }
    }
}
