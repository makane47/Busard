using Busard.Core.Monitoring;
using Busard.Core.Notification;
using Busard.SqlServer.Tools;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Busard.SqlServer.Monitoring
{
    public class ErrorLogWatcher : TimedWatcherService
    {
        private DateTime _lastDateTime;

        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly Core.WatchersConfiguration _config;

        private Dictionary<string, uint> _loginsFailed;

        public override string Name => "ErrorLogWatcher";

        public ErrorLogWatcher(TimedWatchersConcurrentPriorityQueue queue, IOptions<Core.GlobalConfiguration> config) : base(queue)
        {
            _config = config.Value.Watchers;
            Log.Information("ErrorLogWatcher constructor");

            this._connectionString = new SqlConnectionStringBuilder(@$"Data Source={_config.SqlServer.ServerName};Initial Catalog = master;
                User Id={_config.SqlServer.UserId}; Password={_config.SqlServer.UserPassword};");

            _lastDateTime = GetCurrentDateTime().AddHours(-1);
        }

        public override Task RunAsync()
        {
            this.GetErrorLog(_lastDateTime);

            // checks
            this.CheckLoginFailed();

            return base.RunAsync();
        }

        private void GetErrorLog(DateTime startDT)
        {
            var sql = string.Format(Resources.Queries.GetXpReadErrorlog, startDT.ToString("yyyyMMdd HH:mm:ss.fff"));
            DateTime when = DateTime.MinValue;

            using var cn = new SqlConnection(this._connectionString.ConnectionString);
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Tools.ErrorLogRow
                    {
                        LogDate = reader.GetDateTime("LogDate"),
                        ProcessInfo = reader.GetString("ProcessInfo"),
                        Text = reader.GetString("Text").Trim()
                    };
                    Log.Information($"[{row.LogDate}] [{row.ProcessInfo}] {row.Text}");
                    if (Tools.ErrorLogParser.GetType(row) == Tools.ErrorLogRowType.LoginFailed )
                    {
                        var lfi = Tools.ErrorLogParser.ParseLoginFailed(row);
                        if (lfi.HasValue)
                        {
                            this.RecordLoginFailed(lfi.Value);
                        }    
                    }
                    when = row.LogDate;
                }
                if (when > DateTime.MinValue)
                {
                    _lastDateTime = when.AddMilliseconds(3);
                }
            }
            cn.Close();

        }

        private DateTime GetCurrentDateTime()
        {
            var result = DateTime.MinValue;
            using var cn = new SqlConnection(this._connectionString.ConnectionString);
            using (var cmd = new SqlCommand("SELECT CURRENT_TIMESTAMP", cn))
            {
                cn.Open();
                using var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    result = reader.GetDateTime(0);
                }
            }
            cn.Close();
            return result;
        }

        private void RecordLoginFailed(LoginFailedInfo lfi)
        {
            if (_loginsFailed == null) { _loginsFailed = new Dictionary<string, uint>(); }

            if (_loginsFailed.ContainsKey(lfi.Client)) { 
                _loginsFailed[lfi.Client]++;
            }
            else
            {
                _loginsFailed.Add(lfi.Client, 1);
            }
        }

        private void CheckLoginFailed()
        {
            if (_loginsFailed == null) { return; }
            foreach (var i in _loginsFailed.Where(x => x.Value > 10).ToList())
            {
                string msg = $"There are {i.Value} occurrences of login failed attempts from client [{i.Key}]";
                var m = new NotificationMessage(msg, $"[{_config.SqlServer.ServerName}] SQL Server Login Failed Alert", MessageSeverity.Warning);
                Log.Debug($"ErrorLogWatcher is generating a message : {m}");
                this.SendNotification(m);
                _loginsFailed[i.Key] = 0;
            }
        }

    }
}
