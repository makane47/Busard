using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busard.Core.Notification
{
    public interface INotifier: IDisposable
    {
        public void AddNotification(NotificationMessage message);
        
        public Task NotifyAsync();

    }
}
