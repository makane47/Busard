using Busard.Core.Monitoring;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busard.SqlServer.Monitoring
{
    public class PerformanceCountersWatcher : TimedWatcherService
    {
        public override string Name => "PerformanceCountersWatcher";
        public PerformanceCountersWatcher(TimedWatchersConcurrentPriorityQueue queue) : base(queue)
        {
            Log.Information($"{this.Name} constructor");
        }

        public override Task RunAsync()
        {
            Log.Information($"[{this.Name}] wants to do something");
            return base.RunAsync();
        }
    }
}
