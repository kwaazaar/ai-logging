using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharedServices
{
    public class AIDemoService : IHostedService, IDisposable
    {
        private readonly IApplicationLifetime _lifetimeManager;
        private readonly ILogger _logger;
        private readonly IOptions<DemoConfig> _demoConfig;
        private readonly IEventLogger _evtLogger;

        private Timer _timer;
        private int _timerHitCount;

        public AIDemoService(IApplicationLifetime lifetimeManager, ILogger<AIDemoService> logger, IOptions<DemoConfig> demoConfig, IEventLogger evtLogger)
        {
            _lifetimeManager = lifetimeManager;
            _logger = logger;
            _demoConfig = demoConfig;
            _evtLogger = evtLogger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            _timerHitCount = 0;
            _timer = new Timer(DoWork, null, TimeSpan.Zero, _demoConfig.Value.Every);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _timerHitCount++;
            _logger.Log(LogLevel.Trace, "DoWork (hitcount={hitcount})", _timerHitCount);

            if (_timerHitCount % 2 == 0)
            {
                _evtLogger.LogEvent("KlantIngelogd", "robert");
            }

            if (_timerHitCount % 3 == 0)
            {
                _evtLogger.LogEvent("ProductInWinkelwagen", "robert");
            }

            if (_timerHitCount % 10 == 0)
            {
                _evtLogger.LogEvent("BestellingGeplaatst", "robert");
                _logger.Log(LogLevel.Information, "Bestelling is geplaatst (hitcount={hitcount})", _timerHitCount);
            }

            if (_timerHitCount % 11 == 0)
            {
                _evtLogger.LogEvent("BestellingBetaald", "robert");
            }

            if (_timerHitCount % 30 == 0)
            {
                _logger.Log(LogLevel.Error, 
                    new InvalidOperationException("Fout opgetreden in bestelproces"), 
                    "Fout opgetreden (hitcount={hitcount})", _timerHitCount);
            }

            if (_timerHitCount >= _demoConfig.Value.StopAfter)
            {
                _lifetimeManager.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
