using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Serilog;

namespace Busard.Core.Notification
{
    public class EmailNotifier : NotifierBase
    {
        public readonly string SmtpServer;
        public readonly ushort SmtpPort;
        public readonly string Subject;

        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }

        public EmailNotifier(string emailFrom, string emailTo, string subject, string smtpServer, ushort smtpPort = 25)
        {
            this.SmtpServer = smtpServer;
            this.SmtpPort = smtpPort;
            this.EmailFrom = emailFrom;
            this.EmailTo = emailTo;
            this.Subject = subject;

            // TODO : Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            //client.UseDefaultCredentials = true;

        }
        protected override async Task SendNotification()
        {
            Debug.Assert(EmailFrom != null);
            Debug.Assert(EmailTo != null);

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                var message = _messages[i];
                var mail = new MailMessage(EmailFrom, EmailTo)
                {
                    Subject = " [" + message.Severity.ToString().ToUpper() + "] - " + message.Subject,
                    Body = message.ToString()
                };

                // use one instance per mail, not global. SmtpClient does not allow you to execute multiple asynchronous operations at the same time
                // keeping a client open in the instance could lead to :
                // InvalidOperationException : An asynchronous call is already in progress. It must be completed or canceled before you can call this method.
                using SmtpClient smtpClient = new SmtpClient(this.SmtpServer, this.SmtpPort);
                try
                {
                    // https://stackoverflow.com/questions/7276375/what-are-best-practices-for-using-smtpclient-sendasync-and-dispose-under-net-4
                    //await _smtpClient.SendMailAsync(message);
                    await smtpClient.SendMailAsync(mail);
                }
                catch (Exception ex)
                {
                    Log.Error("Exception caught in SendNotification(): {0}", ex.ToString());
                    throw ex;
                }

                message = null;
                _messages.RemoveAt(i);
            }

            /*
            var message = new MailMessage(EmailFrom, EmailTo)
            {
                Subject = this.Subject + " ["
                    + _messages.OrderByDescending(m => (ushort)m.Severity).First().Severity.ToString().ToUpper() + "] - " 
                    + String.Join(" | ", _messages.Select(m => m.Subject)),
                Body = String.Join('\n', _messages.Select(m => m.ToString()))
            };

            // use one instance per mail, not global. SmtpClient does not allow you to execute multiple asynchronous operations at the same time
            // keeping a client open in the instance could lead to :
            // InvalidOperationException : An asynchronous call is already in progress. It must be completed or canceled before you can call this method.
            using SmtpClient smtpClient = new SmtpClient(this.SmtpServer, this.SmtpPort);
            try
            {
                // https://stackoverflow.com/questions/7276375/what-are-best-practices-for-using-smtpclient-sendasync-and-dispose-under-net-4
                //await _smtpClient.SendMailAsync(message);
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Log.Error("Exception caught in SendNotification(): {0}", ex.ToString());
                throw ex;
            }
            */

        }

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
