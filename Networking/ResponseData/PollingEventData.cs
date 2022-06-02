
namespace NordDailyReminder.Networking.ResponseData
{
    internal class PollingEventsData
    {
        public PollingEventData[] events;
        public bool ok;
        public string description;
    }

    internal class PollingEventData
    {
        public int eventId;
        public string type;
        public Payload? payload;
    }

     internal class Payload
    {
        public string? msgId;
        public double? timestamp;
        public string? text;
        public From? from;
        public Chat? chat;
        public Part[] parts;
    }

    public class From
    {
        public string userId;
        public string firstName;
        public string lastName;
    }

    public class Chat
    {
        public string chatId;
        public string type;
    }

    internal class Part
    {
        public PartsPayload payload;
    }

    internal class PartsPayload
    {
        public string? fileId;
        public string? type;
    }

}
