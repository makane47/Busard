using Busard.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Busard.Watcher
{
    public class NotifierService : IHostedService
    {
        private readonly GlobalConfiguration _config;
        // TODO - look at ISchedulerPeriodic
        private List<Core.Notification.INotifier> _notifiers { get; set; }

        private Timer _timer;

        public NotifierService(IOptions<GlobalConfiguration> config) => _config = config.Value;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = _config.Notification;
            Log.Information("NotifierService is starting.");
            //----------------Notifiers----------------
            _notifiers = new List<Core.Notification.INotifier>(conf.Notifiers.Count());
            foreach (var n in conf.Notifiers)
            {
                switch (n)
                {
                    case "Email":
                        var email = new Busard.Core.Notification.EmailNotifier(conf.Email.EmailFrom, conf.Email.CriticalEmailTo,
                            conf.Email.Subject, conf.Email.SmtpServer, conf.Email.SmtpPort);
                        _notifiers.Add(email);
                        break;
                    case "Telegram":
                        _notifiers.Add(new Busard.Core.Notification.TelegramNotifier(conf.Telegram.TelegramToken, conf.Telegram.CriticalChatId));
                        break;
                    case "SqlServerTable":
                        _notifiers.Add(new Busard.Core.Notification.SqlServerTableNotifier(conf.SqlServerTable));
                        break;
                    default:
                        Log.Warning($"NotifierService : the notifier {n} is unknown");
                        break;
                }
            }

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(conf.FrequencySeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            this.SendNotificationAsync().Wait();
            //Log.Information("Timed Background Service is working.");
        }

        public async Task SendNotificationAsync()
        {
            bool toSend = false;

            while (Core.SharedState.NotificationChannel.Count > 0)
            {
                Core.Notification.NotificationMessage msg;
                if (Core.SharedState.NotificationChannel.TryTake(out msg))
                {
                    toSend = true;
                    foreach (var n in _notifiers) { n.AddNotification(msg); }
                }
            }

            if (toSend)
            {
                Log.Information("sending message to {nb} notifiers.", _notifiers.Count);
                foreach (var n in _notifiers) { await n.NotifyAsync(); }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Timed Background Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            //TODO - how to make sure all notifications were sent ?

            if (_notifiers != null && _notifiers.Count > 0)
            {
                foreach (var n in _notifiers)
                {
                    n.Dispose();
                }
            }
            _timer?.Dispose();
        }
    }
}
