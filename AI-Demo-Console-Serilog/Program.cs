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
                    services.AddApplicationInsightsKubernetesEnricher();
                    services.Configure<DemoConfig>(hostContext.Configuration.GetSection("DemoConfig"));
                    services.AddSingleton<IHostedService, AIDemoService>();
                })

                // https://github.com/serilog/serilog-extensions-hosting
                .UseSerilog((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration;

                    TelemetryConfiguration.Active.InstrumentationKey = config["ApplicationInsights:InstrumentationKey"];
                    logging
                        .MinimumLevel.Verbose() // Lowest level
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                        .WriteTo.Console();
                });

            await builder.RunConsoleAsync();
        }
    }
}
