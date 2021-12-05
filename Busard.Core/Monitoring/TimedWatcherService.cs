using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busard.Core.Monitoring
{
    public abstract class TimedWatcherService : IHostedService, ITimedWatcherService
    {
        private readonly TimedWatchersConcurrentPriorityQueue _queue;

        public abstract string Name { get; }
        public int FrequencySeconds { get; set; } = 30;

        protected TimedWatcherService(TimedWatchersConcurrentPriorityQueue queue)
        {
            this._queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._queue.Enqueue(DateTime.Now.AddSeconds(30), this);
            return Task.CompletedTask;
        }

        public virtual Task RunAsync()
        {
            var nextRun = DateTime.Now.AddSeconds(FrequencySeconds);
            Log.Information($"Next run will be at {nextRun}");
            this._queue.Enqueue(nextRun, this);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._queue.Remove(this.Name);
            return Task.CompletedTask;
        }

        public void SendNotification(Core.Notification.NotificationMessage message)
        {
            Core.SharedState.NotificationChannel.Add(message);
        }

    }
}
