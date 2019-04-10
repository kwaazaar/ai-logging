using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedServices
{
    public class AIDemoService : IHostedService, IDisposable
    {
        private readonly IApplicationLifetime _lifetimeManager;
        private readonly ILogger _logger;
        private readonly IOptions<DemoConfig> _demoConfig;
        private Timer _timer;
        private int _timerHitCount;
        private Dictionary<int, LogLevel> _config = new Dictionary<int, LogLevel>
        {
            { 1, LogLevel.Trace },
            { 2, LogLevel.Debug },
            { 3, LogLevel.Information },
            { 4, LogLevel.Warning },
            { 5, LogLevel.Error },
            { 6, LogLevel.Critical },
            { 7, LogLevel.None },
        };

        public AIDemoService(IApplicationLifetime lifetimeManager, ILogger<AIDemoService> logger, IOptions<DemoConfig> demoConfig)
        {
            _lifetimeManager = lifetimeManager;
            _logger = logger;
            _demoConfig = demoConfig;
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
            var level = GetLogLevel(_timerHitCount);
            if (level < LogLevel.Error)
            {
                _logger.Log(level, "Structured message @ level:{level} (hitcount={hitcount})", level, _timerHitCount);
            }
            else
            {
                _logger.Log(level,
                    new InvalidOperationException(String.Format("Structured message @ level:{0} (hitcount ={1})", level, _timerHitCount)),
                    "Structured message @ level:{level} (hitcount={hitcount})", level, _timerHitCount);
            }

            if (_timerHitCount >= _demoConfig.Value.StopAfter)
            {
                _lifetimeManager.StopApplication();
            }
        }

        private LogLevel GetLogLevel(int timerHitCount)
        {
            var key = _config.Keys.Reverse().FirstOrDefault(k => timerHitCount % k == 0);
            return _config[key];
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
