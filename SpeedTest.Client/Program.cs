using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpeedTest.Models;

namespace SpeedTest.Client
{
    class Program
    {
        private static SpeedTestClient client;
        private static Settings settings;
        static List<int> invalid_server_ids = new List<int>(10);
        static async Task Main()
        {
            Console.WriteLine("Getting speedtest.net settings and server list...");
            client = new SpeedTestClient();
            settings = await client.GetSettingsAsync();

            var servers = await SelectServers();
            var bestServer = SelectBestServer(servers);

            Console.WriteLine("Testing speed...");
            var downloadSpeed = await client.TestDownloadSpeedAsync(bestServer, settings.Download.ThreadsPerUrl);
            PrintSpeed("Download", downloadSpeed);
            var uploadSpeed = await client.TestUploadSpeedAsync(bestServer, settings.Upload.ThreadsPerUrl);
            PrintSpeed("Upload", uploadSpeed);

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        private static Server SelectBestServer(IEnumerable<Server> servers)
        {
            Console.WriteLine();
            Console.WriteLine("Best server by latency:");
            var bestServer = servers.Where(x => !invalid_server_ids.Contains(x.Id)).OrderBy(x => x.Latency).First();
            PrintServerDetails(bestServer);
            Console.WriteLine();
            return bestServer;
        }

        private static async Task<IEnumerable<Server>> SelectServers()
        {
            Console.WriteLine();
            Console.WriteLine("Selecting best server by distance...");
            var servers = settings.Servers.Take(10).ToList();

            foreach (var server in servers)
            {
                try
                {
                    server.Latency = await client.TestServerLatencyAsync(server);
                    PrintServerDetails(server);
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid server ...");
                    PrintServerDetails(server);
                    System.Diagnostics.Debug.WriteLine("Invalid server ...");
                    invalid_server_ids.Add(server.Id);
                    continue;
                }
            }
            return servers;
        }

        private static void PrintServerDetails(Server server)
        {
            Console.WriteLine("Hosted by {0} ({1}/{2}), distance: {3}km, latency: {4}ms", server.Sponsor, server.Name,
                server.Country, (int)server.Distance / 1000, server.Latency);
        }

        private static void PrintSpeed(string type, double speed)
        {
            if (speed > 1024)
            {
                Console.WriteLine("{0} speed: {1} Mbps", type, Math.Round(speed / 1024, 2));
            }
            else
            {
                Console.WriteLine("{0} speed: {1} Kbps", type, Math.Round(speed, 2));
            }
        }
    }
}
