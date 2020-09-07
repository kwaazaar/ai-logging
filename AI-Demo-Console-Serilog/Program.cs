using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
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

                // https://github.com/serilog/serilog-aspnetcore (serilog-extensions-hosting is voor console apps, maar serilog-aspnetcore werkt net zo goed)
                .UseSerilog((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration;

                    logging
                        .MinimumLevel.Verbose() // Lowest level
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext();
                    logging.WriteTo.Console();

                    logging.WriteTo.ApplicationInsights(config["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces);
                });

            await builder.RunConsoleAsync();
        }
    }
}
