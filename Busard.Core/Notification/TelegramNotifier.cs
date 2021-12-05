using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Busard.Core.Notification
{
    public class TelegramNotifier : NotifierBase
    {
        private readonly string _telegramToken;
        private readonly long _chatId;

        private ITelegramBotClient _botClient;

        public TelegramNotifier(string telegramToken, long chatId)
        {
            _telegramToken = telegramToken;
            _chatId = chatId;
            _botClient = new TelegramBotClient(_telegramToken);

            var me = _botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );
        }

        public override void AddNotification(NotificationMessage message)
        {
            _messages.Add(message);
        }

        public override void Dispose()
        {
            _botClient = null;
        }
        
        protected override async Task SendNotification()
        {
            // https://telegrambots.github.io/book/2/send-msg/text-msg.html
            // https://stackoverflow.com/questions/45414021/get-telegram-channel-group-id

            // https://web.telegram.org/#/im?p=g339389378

            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                _ = await _botClient.SendTextMessageAsync(
                  chatId: _chatId,
                  //text: String.Join('\n', _messages),
                  text: _messages[i].ToString(),
                  parseMode: ParseMode.Markdown,
                  disableNotification: true
                  );
                _messages.RemoveAt(i);
            }
        }

    }
}
