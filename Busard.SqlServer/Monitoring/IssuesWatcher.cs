using Busard.Core.Monitoring;
using Busard.Core.Notification;
using Busard.SqlServer.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.XEvent.XELite;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busard.SqlServer.Monitoring
{
    public class IssuesWatcher : XEventsWatcherBase
    {
        public IssuesWatcher(IOptions<Core.GlobalConfiguration> config) : base(config)
        {
            this.SessionName = _config.SqlServer.IssuesSessionName;

            Log.Information("IssuesWatcher is starting");
        }

        public override void ReceiveXEvent(IXEvent xevent)
        {
            string msg;
            MessageSeverity messageSeverity = MessageSeverity.Critical;

            Log.Debug($"IssuesWatcher ReceiveXEvent method is called.");

            switch (xevent.Name)
            {
                case "error_reported":
                    var severity = (int)xevent.Fields["severity"];

                    if (severity >= 11 && severity <= 16) { messageSeverity = MessageSeverity.Warning;  }
                    else if (severity > 16) { messageSeverity = MessageSeverity.Critical; }
                    else { messageSeverity = MessageSeverity.Information; }

                    msg = $@"[{xevent.Timestamp}, {xevent.Actions["username"]} from {xevent.Actions["client_hostname"]} 
                        ({xevent.Actions["client_app_name"]})] ERROR {xevent.Fields["error_number"]}, {xevent.Fields["severity"]} - {xevent.Fields["message"]}";
                    break;
                case "blocked_process_report":
                    var bpr = new BlockedProcessReportReader(xevent.Fields["blocked_process"].ToString());

                    msg = $@"{xevent.Timestamp}, {xevent.Actions["username"]}]
                        a process is blocked since {Int64.Parse(xevent.Fields["duration"].ToString()) / 1000 / 1000} sec. in db [{xevent.Fields["database_name"]}] \n
                        blocking query : {bpr.BlockedProcessReport.BlockingProcess.InputBuffer} \n
                        blocked query  : {bpr.BlockedProcessReport.BlockedProcess.InputBuffer}";
                    break;
                default:
                    msg = $"no message in xevent {this.SessionName}";
                    break;
            }
            var m = new NotificationMessage(msg, $"[{this.ServerName}] SQL Server Alert", messageSeverity);
            Log.Information($"IssuesWatcher is generating a message : {m}");
            this.SendNotification(m);
        }
    }
}
