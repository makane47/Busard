using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Options;

namespace Busard.Core.Monitoring
{
    //public delegate void 
    public class TimedWatchersConcurrentPriorityQueue : ConcurrentPriorityQueue<DateTime, ITimedWatcherService>, IDisposable
    {
        protected readonly Core.WatchersConfiguration _config;

        public Action<ITimedWatcherService> Callback { get; set; }
        private readonly Timer _timer;

        public TimedWatchersConcurrentPriorityQueue(IOptions<GlobalConfiguration> config)
        {
            this._config = config.Value.Watchers;
            this._timer =  new Timer(Tick, null, 30000, _config.TimedWatchersLoopSeconds); // starts after 30 seconds, runs every 'TimedWatchersLoopSeconds' seconds
        }

        private void Tick (object data)
        {
            // test if some events are ready and send them to the delegate function
            while (this.Callback != null && !this.IsEmpty && this.First().Key.Ticks < DateTime.Now.Ticks 
                && this.TryDequeue(out KeyValuePair<DateTime, ITimedWatcherService> result))
            {
                this.Callback(result.Value);
            }
        }

        public void Remove(string watcherName)
        {
            if (this.IsEmpty) return;

            foreach (var item in this.Where(x => x.Value.Name.Equals(watcherName)))
            {
                this.RemoveAt(item.Key);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
