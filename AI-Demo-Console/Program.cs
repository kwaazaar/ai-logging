using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedServices;
using System.Threading.Tasks;

namespace AI_Demo_Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    if (args != null) { config.AddCommandLine(args); }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddApplicationInsightsKubernetesEnricher();
                    services.Configure<DemoConfig>(hostContext.Configuration.GetSection("DemoConfig"));
                    services.AddSingleton<IHostedService, AIDemoService>();
                })

                // https://docs.microsoft.com/en-Us/azure//azure-monitor/app/ilogger
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration;

                    logging.AddConfiguration(config.GetSection("Logging"));
                    logging.AddApplicationInsights(config["ApplicationInsights:InstrumentationKey"]); // default options are fine
                    logging.AddConsole();
                });
           

            await builder.RunConsoleAsync();
        }
    }
}
