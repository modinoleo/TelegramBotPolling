using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _botClient = new TelegramBotClient("<API Token>");

                    var me = await _botClient.GetMeAsync();

                    Console.Title = me.Username ?? "Manage Bot";

                    var cancellationToken = cancellationTokenSource.Token;

                    var receiverOptions1 = new ReceiverOptions
                    {
                        AllowedUpdates = { } // receive all update types
                    };

                    var updateHandler = new DefaultUpdateHandler(
                       updateHandler: HandleUpdate,
                       pollingErrorHandler: async (_, _, token) => await Task.Delay(10, token)
                   );

                    Console.WriteLine($"Bot '{me.Username}' is listening");


                    await _botClient.ReceiveAsync(updateHandler, receiverOptions: receiverOptions1, cancellationToken: cancellationToken);

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }
        private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceived(botClient, message),
                { ChannelPost: { } channelPost } => BotOnChannelPostReceived(botClient, channelPost),
                _ => throw new NotImplementedException()
            };
            await handler;
        }
        private async Task BotOnMessageReceived(ITelegramBotClient client, Message message)
        {
            Console.WriteLine($"From: {message.From.Username}, Message: {message.Text}");
        }
        private async Task BotOnChannelPostReceived(ITelegramBotClient client, Message channelPost)
        {
            Console.WriteLine($"From: {channelPost.From.Username}, Message: {channelPost.Text}");
        }
    }
}
