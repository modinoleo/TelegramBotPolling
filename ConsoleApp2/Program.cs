using Telegram.Bot;

class Program
{
    private static ITelegramBotClient _botClient;

    static async Task Main()
    {
        _botClient = new TelegramBotClient("<API TOKEN>");

        var me = await _botClient.GetMeAsync();
        Console.Title = me.Username;

        //_botClient.OnMessage += Bot_OnMessage;
        //_botClient.StartReceiving();

        Console.WriteLine($"Bot '{me.Username}' is listening. Press any key to exit.");
        Console.ReadKey();

       // _botClient.StopReceiving();
    }

    //private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
    //{
    //    if (e.Message.Text != null)
    //    {
    //        Console.WriteLine($"Received a message from {e.Message.Chat.Id}: {e.Message.Text}");

    //        // Process the message and send a reply
    //        string replyMessage = GenerateReply(e.Message.Text);
    //        await _botClient.SendTextMessageAsync(e.Message.Chat.Id, replyMessage);
    //    }
    //}

    private static string GenerateReply(string receivedMessage)
    {
        // Add your logic here to generate a reply based on the received message
        // For simplicity, just echo the received message
        return $"You said: {receivedMessage}";
    }
}
