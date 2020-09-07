using Microsoft.ApplicationInsights;
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
            // https://docs.microsoft.com/en-Us/azure//azure-monitor/app/ilogger
            
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    if (args != null) { config.AddCommandLine(args); }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var config = hostContext.Configuration;

                    // ASP.NET Core: Microsoft.Extensions.Logging.ApplicationInsights
                    //services.AddApplicationInsightsTelemetry(config["ApplicationInsights:InstrumentationKey"]);

                    // Console app: Microsoft.ApplicationInsights.WorkerService
                    services.AddApplicationInsightsTelemetryWorkerService(config["ApplicationInsights:InstrumentationKey"]);

                    services.AddApplicationInsightsKubernetesEnricher();

                    services.Configure<DemoConfig>(hostContext.Configuration.GetSection("DemoConfig"));
                    services.AddSingleton<IHostedService, AIDemoService>();
                    
                    services.AddSingleton<IEventLogger, EventLogger>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration;

                    logging.AddConfiguration(config.GetSection("Logging"));
                    logging.AddConsole();

                    // Expliciet toevoegen van ApplicationInsightsLoggerProvider is niet meer nodig. Zie: https://docs.microsoft.com/nl-nl/azure/azure-monitor/app/ilogger#aspnet-core-applications
                    // In een ASP.NET Core app blijkt het echter niet te werken, zonder deze call :-s
                    //logging.AddApplicationInsights(config["ApplicationInsights:InstrumentationKey"]); // default options are fine
                });

            await builder.RunConsoleAsync();
        }
    }
}
