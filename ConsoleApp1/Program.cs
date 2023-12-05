using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots;
using Telegram.Bots.Http;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    private static ITelegramBotClient _botClient;
    private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    static async Task Main()
    {
        try
        {
            _botClient = new TelegramBotClient("6515335903:AAEsVDpD_hkDqm1P3I939W1REtMqN7Yvq0w");

            var me = await _botClient.GetMeAsync();

            Console.Title = me.Username;

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
    //In order to avoid getting duplicate updates, recalculate offset after each server response
    private static async Task DiscardNewUpdateAsync(CancellationToken cancellationToken = default)
    {
        CancellationTokenSource? cts = default;

        try
        {
            if (cancellationToken == default)
            {
                cts = new(TimeSpan.FromSeconds(30));
                cancellationToken = cts.Token;
            }

            int offset = -1;

            while (!cancellationToken.IsCancellationRequested)
            {
                var updates = await _botClient.GetUpdatesAsync(
                    offset: offset,
                    allowedUpdates: Array.Empty<UpdateType>(),
                    cancellationToken: cancellationToken
                );

                if (updates.Length == 0) break;

                offset = updates[^1].Id + 1;
            }
        }
        finally
        {
            cts?.Dispose();
        }
    }

   static async Task<Update[]> GetOnlyAllowedUpdatesAsync(
        int offset,
        CancellationToken cancellationToken,
        params UpdateType[] types)
    {
        var updates = await _botClient.GetUpdatesAsync(
            offset: offset,
            timeout: 120,
            allowedUpdates: types,
            cancellationToken: cancellationToken
        );

        return updates.ToArray();
    }
    private static async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        await Task.Delay(10, cancellationTokenSource.Token);
        var offset = 0;
        if (update.Message != null)
        {
            Update[] matchingUpdates = Array.Empty<Update>();

            if (update.Message.Text == "/menu")
            {
                string callbackQueryData_Ok = "btn_Ok";
                string callbackQueryData_Cancel = "btn_Cancel";

                Message message = await _botClient.SendTextMessageAsync(update?.Message?.Chat,
                    "Choose an option:",
                    replyToMessageId: update?.Message?.MessageId,
                    parseMode: ParseMode.Markdown,
                     replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("OK", callbackQueryData_Ok),
                             InlineKeyboardButton.WithCallbackData("Cancel", callbackQueryData_Cancel)
                        }));
                var query = new[] { UpdateType.CallbackQuery };

                await DiscardNewUpdateAsync(cancellationTokenSource.Token);

                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Update[] updates = await GetOnlyAllowedUpdatesAsync(offset: 0, types: new[] { UpdateType.CallbackQuery },
                       cancellationToken: cancellationTokenSource.Token);

                    matchingUpdates = updates.Where(a => query.Contains(a.Type)).ToArray();

                    if (matchingUpdates.Length > 0) { break; }

                    offset = updates.LastOrDefault()?.Id + 1 ?? 0;

                    await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationTokenSource.Token);
                }
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                await DiscardNewUpdateAsync(cancellationTokenSource.Token);

                CallbackQuery callbackQuery = matchingUpdates[0].CallbackQuery!;

                await _botClient.AnswerCallbackQueryAsync(
                   callbackQueryId: callbackQuery!.Id,
                   text: callbackQueryData_Ok
               );
                if(callbackQuery != null )
                {
                    if(callbackQuery.Data == "btn_Ok")
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Button Ok clicked!");
                    }
                    else if(callbackQuery.Data == "btn_Cancel")
                    {
                        await _botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Button Cancel clicked!");
                    }
                }
            }
            else
            {
                Console.WriteLine($"We received message from {update?.Message?.From.FirstName}, Message: {update.Message.Text}");
                await _botClient.SendTextMessageAsync(update?.Message?.Chat?.Id, GenerateReply(update.Message.Text));

            }
        }
        else if (update.ChannelPost != null)
        {
            await _botClient.SendTextMessageAsync(update?.ChannelPost?.Chat?.Id, GenerateReply(update.ChannelPost.Text));
        }


    }
    private static InlineKeyboardMarkup? GenerateReply2(string text)
    {

        if (text == "/menu")
        {
            string callbackQueryData = 'a' + new Random().Next(5_000).ToString();
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
               {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("✅ Meron", "option1"),
                        InlineKeyboardButton.WithCallbackData("❎ Wala", "option2")
                    }
        });
            return inlineKeyboard;

        }
        return null;
    }
    private static string GenerateReply(string receivedMessage)
    {

        if (receivedMessage == "/start")
        {
            return "Welcome to my life";
        }
        if (receivedMessage == "/help")
        {
            return "Feel free to ask if you need more assistance!";
        }

        return $"You said: {receivedMessage}";
    }
}