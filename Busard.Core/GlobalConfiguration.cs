using System;
using System.Collections.Generic;
using System.Text;

namespace Busard.Core
{
    public class GlobalConfiguration
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public string Language { get; set; }
        public bool DebugMode { get; set; }
        public NotificationConfiguration Notification { get; set; }
        public WatchersConfiguration Watchers { get; set; }

    }

    public class NotificationConfiguration
    {
        public double FrequencySeconds { get; set; }
        public string[] Notifiers { get; set; }
        public EmailConfiguration Email { get; set; }
        public TelegramConfiguration Telegram { get; set; }
        public SqlServerTableNotificationConfiguration SqlServerTable { get; set; }

    }

    public class EmailConfiguration
    {
        public string SmtpServer { get; set; }
        public ushort SmtpPort { get; set; } = 25;
        public string EmailFrom { get; set; }
        public string InformationEmailTo { get; set; }
        public string WarningEmailTo { get; set; }
        public string CriticalEmailTo { get; set; }
        public Notification.MessageSeverity[] NotifyOn { get; set; }
        public string Subject { get; set; }
    }

    public class TelegramConfiguration
    {
        public string TelegramToken { get; set; }
        public long InformationChatId { get; set; }
        public long WarningChatId { get; set; }
        public long CriticalChatId { get; set; }
        public Notification.MessageSeverity[] NotifyOn { get; set; }
    }

    public class WatchersConfiguration
    {
        public string[] Modules { get; set; }
        public ushort Retries { get; set; }
        public ushort RetryIntervalSeconds { get; set; }
        public ushort TimedWatchersLoopSeconds { get; set; }
        
        public SqlServerWatcherConfiguration SqlServer { get; set; }
    }

    public class SqlServerWatcherConfiguration
    {
        public string[] Watchers { get; set; }
        public string ServerName { get; set; }
        public string UserId { get; set; }
        public string UserPassword { get; set; }
        public string IssuesSessionName { get; set; }
    }

    public class SqlServerTableNotificationConfiguration
    {
        public string ServerName { get; set; }
        public string UserId { get; set; }
        public string UserPassword { get; set; }
        public string TableName { get; set; }
    }

    //Serilog { get; set; }
    //  # Using = [ "Serilog.Sinks.Console", "Serilog.Sinks.Email" ]
    //  Using { get; set; } [ "Serilog.Sinks.Console" ]
    //  MinimumLevel { get; set; } "Debug"
}
