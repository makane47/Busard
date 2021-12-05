using Busard.Core.Notification;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busard.Core.Monitoring
{
    /// <summary>
    /// Base class for watchers services
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
    public abstract class WatcherService : BackgroundService
    {
        /// <summary>
        /// Adds a notification to the message queue (<see cref="Core.SharedState"/>)
        /// </summary>
        /// <param name="message">The message to add to the send queue (NotificationMessage class).</param>
        public void SendNotification(Core.Notification.NotificationMessage message)
        {
            Core.SharedState.NotificationChannel.Add(message);
        }
    }
}
