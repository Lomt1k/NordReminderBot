
namespace NordDailyReminder
{
    public class Message
    {
        public string? fileId;
        public string text;

        public Message(string? fileId, string text)
        {
            this.fileId = fileId;
            this.text = text;
        }
    }

    internal class TargetTime
    {
        public int hour;
        public int minute;
        public int second;
    }

    internal class Config
    {
        public string API_URL = "https://api.internal.myteam.mail.ru/bot/v1";
        public string token = "YOUR_TOKEN_HERE";
        public string chatId = "CHAT_ID_FOR_SENDING_MESSAGES";
        public TargetTime targetTime = new TargetTime { hour = 10, minute = 58, second = 30 };

        public Message[] messages = new Message[]
        {
            // Массив Message настраиваем в самом файле config.json! Эти указаны чисто для примера по умолчанию
            new Message(null, "Привет, летучка!"),
            new Message("02q3w000ZXToTSIiTM8DHp6298d02e1bc", "Пропущу летучку, на созвоне...")
        };
    }

}
