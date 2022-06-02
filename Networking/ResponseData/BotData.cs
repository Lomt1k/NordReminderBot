namespace NordDailyReminder.Networking.ResponseData
{
    internal class BotData
    {
        public bool ok;
        public string description;
        public string userId;
        public string nick;
        public string firstName;
        public string about;
        public BotPhoto? photo;
    }

    internal class BotPhoto
    {
        public string url;
    }
}
