using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busard.Core.Notification
{
    public enum MessageFormatType : byte
    {
        Text,
        HTML,
        MarkDown,
        JSON
    }

    public abstract class NotifierBase : INotifier
    {
        public MessageFormatType MessageFormat { get; set; } = MessageFormatType.Text;

        protected List<NotificationMessage> _messages = new List<NotificationMessage>();

        protected virtual string FormatMessage(MessageItem item)
        {
            throw new NotImplementedException();
        }

        public virtual void AddNotification(NotificationMessage message)
        {
            _messages.Add(message);
        }

        public async Task NotifyAsync()
        {
            await SendNotification();
            _messages.Clear();
        }

        protected abstract Task SendNotification();

        public abstract void Dispose();
    }
}
