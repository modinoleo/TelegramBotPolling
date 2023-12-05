using Moq;
using Telegram.Bot;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact(DisplayName ="Send reply to user help command")]
        public void Should_Answer_To_Help_Command()
        {
            var mockBotClient = new Mock<ITelegramBotClient>();
        }
    }
}