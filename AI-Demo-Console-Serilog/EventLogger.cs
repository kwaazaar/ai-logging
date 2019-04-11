using Microsoft.ApplicationInsights;
using SharedServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AI_Demo_Console
{
    public class EventLogger : IEventLogger
    {
        private readonly TelemetryClient _telemetryClient;
        public EventLogger(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public Task LogEvent(string eventName, string user = null)
        {
            var parameters = new Dictionary<string, string>();
            if (user != null) parameters.Add("username", user);

            _telemetryClient.TrackEvent(eventName, parameters);

            return Task.CompletedTask;
        }
    }
}
