using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busard.Core.Monitoring
{
    public class TimedQueueService : BackgroundService
    {
        private readonly BlockingCollection<ITimedWatcherService> _queue = new BlockingCollection<ITimedWatcherService>();
        private readonly Object _locker = new Object();
        private readonly TimedWatchersConcurrentPriorityQueue _entryQueue;
        private readonly IEnumerable<ITimedWatcherService> _watchers;

        public TimedQueueService(TimedWatchersConcurrentPriorityQueue entryQueue, IEnumerable<ITimedWatcherService> watchers)
        {
            Log.Information("TimedQueueService is starting");
            this._entryQueue = entryQueue ?? throw new ArgumentNullException(nameof(entryQueue));
            this._entryQueue.Callback = this.TakeFromQueue;
            this._watchers = watchers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Debug("TimedQueueService is running");
            //throw new NotImplementedException(); TODO

            // starting listening to the queue
            await Task.Run(() =>
            {
                foreach (var watcher in _queue.GetConsumingEnumerable())
                {
                    Log.Debug($"Executing TimedWatcher [{watcher.Name}] now");
                    watcher.RunAsync();
                    Task.Run(() => Thread.Sleep(100));
                }
            });
        }

        public void TakeFromQueue(ITimedWatcherService watcher)
        {
            Log.Debug($"TimedWatcher [{watcher.Name}] is entering execution stage at {DateTime.Now}");
            _queue.Add(watcher);
        }

    }
}
