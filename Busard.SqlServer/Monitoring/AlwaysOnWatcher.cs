using Busard.Core.Monitoring;
using Busard.Core.Notification;
using Busard.SqlServer.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.XEvent.XELite;
using Serilog;
using System;
using System.Collections.Generic;

namespace Busard.SqlServer.Monitoring
{
    public class AlwaysOnWatcher : XEventsWatcherBase
    {
        internal struct ReplicaStateChangeRule
        {
            internal Tools.HadrAvailabilityReplicaRole PreviousState;
            internal Tools.HadrAvailabilityReplicaRole CurrentState;
            internal Core.Notification.MessageSeverity Severity;
        }

        internal List<ReplicaStateChangeRule> ReplicaStateChangeRules { get; set; } = new List<ReplicaStateChangeRule>();

        public AlwaysOnWatcher(IOptions<Core.GlobalConfiguration> config) : base(config)
        {
            this.SessionName = "AlwaysOn_health";
            Log.Information("AlwaysOnWatcher is starting");

            this.SetReplicaStateChangeRules();
        }

        private void SetReplicaStateChangeRules()
        {
            // TODO -- from config
            ReplicaStateChangeRules.Add(
                new ReplicaStateChangeRule 
                { 
                    PreviousState = HadrAvailabilityReplicaRole.PRIMARY_PENDING,
                    CurrentState = HadrAvailabilityReplicaRole.RESOLVING_NORMAL,
                    Severity = MessageSeverity.Critical
                }
            );
        }

        private void CheckAlwaysOnHealth()
        {
            // pas envie ce soir
        }

        public override void ReceiveXEvent(IXEvent xevent)
        {
            string msg = "";
            MessageSeverity messageSeverity = MessageSeverity.Critical;

            switch (xevent.Name)
            {
                case "error_reported":
                    if (xevent.Fields["is_intercepted"] is bool is_intercepted && !is_intercepted)
                    msg = $@"[{xevent.Timestamp}, AlwaysOn Error] {xevent.Fields["error_number"]}, {xevent.Fields["severity"]} - {xevent.Fields["message"]}";
                    break;
                case "availability_replica_state_change":
                    HadrAvailabilityReplicaRole currentState = (HadrAvailabilityReplicaRole)Enum.Parse(typeof(HadrAvailabilityReplicaRole), xevent.Fields["current_state"].ToString());

                    if (
                        currentState == HadrAvailabilityReplicaRole.PRIMARY_PENDING || 
                        currentState == HadrAvailabilityReplicaRole.RESOLVING_NORMAL ||
                        currentState == HadrAvailabilityReplicaRole.RESOLVING_PENDING_FAILOVER ||
                        currentState == HadrAvailabilityReplicaRole.NOT_AVAILABLE
                        )
                    {
                        msg = $@"[{xevent.Timestamp}, ALWAYSON STATE CHANGE] Replica {xevent.Fields["availability_replica_name"]} 
                            is changing from {xevent.Fields["previous_state"]} to {xevent.Fields["current_state"]} in group {xevent.Fields["availability_group_name"]}";
                    }
                    else
                    {
                        return;
                    }
                    break;
                case "availability_group_lease_expired":
                    msg = $@"[{xevent.Timestamp}, ALWAYSON LEASE EXPIRED] {xevent.Fields["state"]} for group {xevent.Fields["availability_group_name"]} (lease interval : {xevent.Fields["lease_interval"]})";
                    break;
                default:
                    msg = $"no message in xevent {this.SessionName}";
                    messageSeverity = MessageSeverity.Information;
                    break;
            }
            var m = new NotificationMessage(msg, $"[{this.ServerName}] AlwaysOn Alert", messageSeverity);
            Log.Information($"AlwaysOnWatcher is generating a message : {m}");
            this.SendNotification(m);
        }
    }
}
