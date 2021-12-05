using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Busard.Core
{
    public static class SharedState
    {
        // thread-safe communication channel between watchers and notifiers
        public static readonly ConcurrentBag<Core.Notification.NotificationMessage> NotificationChannel = new ConcurrentBag<Core.Notification.NotificationMessage>();

    }
}
