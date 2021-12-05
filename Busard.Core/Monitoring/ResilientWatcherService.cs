using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Busard.Core.Monitoring
{
    /// <summary>
    /// Base class for resilient watchers services.
    /// Resilient watchers will try to execute the <code ExecuteAsync>ExecuteAsync</code> 
    /// method for a configured amount of time if it fails due to an exception.
    /// </summary>
    /// <seealso cref="Busard.Core.Monitoring.WatcherService" />
    public abstract class ResilientWatcherService : WatcherService
    {
        protected readonly Core.WatchersConfiguration _config;

        public override void Dispose()
        {
            base.Dispose();
        }

        private async Task DoOrTimeoutAsync(Func<Task> doWork)
        {
            bool stopTrying = false;
            DateTime time = DateTime.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(_config.RetryIntervalSeconds * _config.Retries);
            while (!stopTrying)
            {
                try
                {
                    Log.Debug("task is trying to restart");
                    await Task.Delay(TimeSpan.FromSeconds(_config.RetryIntervalSeconds));
                    await Task.Run(doWork);
                    stopTrying = true;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now.Subtract(time).Milliseconds > timeout.TotalMilliseconds)
                    {
                        stopTrying = true;
                        throw;
                    }
                }
            }
        }

        public ResilientWatcherService(IOptions<GlobalConfiguration> config): base()
        {
            _config = config.Value.Watchers;
        }

        protected override sealed async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await this.DoWorkAsync(stoppingToken);
            }
            catch (Exception e)
            {
                try
                {
                    await this.DoOrTimeoutAsync(() => this.DoWorkAsync(stoppingToken));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            //var executingTask = this.DoWorkAsync(stoppingToken);

            //// If the task is completed then return it, this will bubble cancellation and failure to the caller
            //if (executingTask.IsCompleted)
            //{
            //    return;
            //}

            //if (executingTask.IsFaulted || executingTask.Exception != null)
            //{
            //    // spinning maybe too heavy on the CPU
            //    //bool spinUntil = SpinWait.SpinUntil(() => this.DoWorkAsync(stoppingToken).IsCompleted, TimeSpan.FromSeconds(_config.Retries * _config.RetryIntervalSeconds));
            //    await this.DoOrTimeoutAsync(() => this.DoWorkAsync(stoppingToken));
            //}

            //// Otherwise it's running
            //return;
        }

        protected abstract Task DoWorkAsync(CancellationToken stoppingToken);
    }
}
