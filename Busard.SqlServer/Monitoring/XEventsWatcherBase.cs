using Busard.Core.Monitoring;
using Busard.Core.Notification;
using Busard.SqlServer.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.XEvent.XELite;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Busard.SqlServer.Monitoring
{
    /// <summary>
    /// Base class for SQL Server watchers based on an Extended Event session
    /// </summary>
    /// <seealso cref="Busard.Core.Monitoring.ResilientWatcherService" />
    public abstract class XEventsWatcherBase : ResilientWatcherService
    {
        public readonly SqlConnectionStringBuilder ConnectionString;
        public readonly string ServerName;
        public string SessionName { get; protected set; }
        public ReadXEventStream XEventsSession { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XEventsWatcherBase"/> class.
        /// Set the connection string using options passed by constructor injection from the DI container
        /// </summary>
        /// <param name="config">The configuration passed by IOptions (constructor injection).</param>
        public XEventsWatcherBase(IOptions<Core.GlobalConfiguration> config): base(config)
        {
            this.ServerName = _config.SqlServer.ServerName;
            var userId = _config.SqlServer.UserId;
            var userPassword = _config.SqlServer.UserPassword;

            this.ConnectionString = new SqlConnectionStringBuilder(@$"Data Source={this.ServerName};Initial Catalog = master;
                User Id={userId}; Password={userPassword};");
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            this.XEventsSession = new ReadXEventStream(this.ConnectionString.ConnectionString, this.SessionName);
            cancellationToken.Register(() =>
            {
                //Log.Debug($"Service {_serviceType} is stopping.");
                Log.Debug($"Service XEvents [{this.SessionName}] is stopping.");
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await XEventsSession.ReadStreamAsync(this.ReceiveXEvent, cancellationToken);
                }
                catch (Exception e)
                {
                    Log.Error($"Watcher [{this.SessionName}] stopped by error, restarting.", e);
                    throw;
                }
            }
            Log.Information($"Watcher [{this.SessionName}] stopped by cancellation request.");
        }

        public abstract void ReceiveXEvent(IXEvent xevent);

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            Log.Information($"Watcher [{this.SessionName}] stopped by StopAsync method.");
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            base.Dispose();
            XEventsSession = null;
        }

    }
}
