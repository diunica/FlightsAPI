using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FlightsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NLogBuilder.ConfigureNLog("nlog.config");

            BuildWebHost(args, !(Debugger.IsAttached || args.Contains("--console"))).Run();

            NLog.LogManager.Shutdown();
        }

        public static IHost BuildWebHost(string[] args, bool isService)
        {
            var pathToContentRoot = Directory.GetCurrentDirectory();

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }
            args = args.Where(s => s != "--console").ToArray();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(pathToContentRoot);
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseHttpSys(options =>
                    {
                        options.MaxConnections = 100;
                        options.MaxRequestBodySize = null;
                        options.UrlPrefixes.Add("http://localhost:5252/flyapi");
                        options.UrlPrefixes.Add("https://+/flyapi");
                    });
                })
                .UseWindowsService()
                .UseNLog()
                .Build();
        }

    }
}
